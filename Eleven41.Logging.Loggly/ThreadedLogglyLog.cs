using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Loggly;

namespace Eleven41.Logging
{
	public class ThreadedLogglyLog : LogglyBase, ILog
	{
		private ConcurrentQueue<LogRecord> _records;
		private Thread _thread;

		/// <summary>
		/// Constructs a ThreadedLogglyLog object.
		/// </summary>
		/// <param name="log"></param>
		public ThreadedLogglyLog()
		{
			this.MaxEvents = 100;

			_records = new ConcurrentQueue<LogRecord>();

			// Start the thread
			_thread = new Thread(new ThreadStart(Run));
			_thread.Start();
		}

		/// <summary>
		/// Constructs a ThreadedLogglyLog object with the supplied data.
		/// </summary>
		/// <param name="log"></param>
		public ThreadedLogglyLog(Dictionary<string, object> data)
			: base(data)
		{
			this.MaxEvents = 100;

			_records = new ConcurrentQueue<LogRecord>();

			// Start the thread
			_thread = new Thread(new ThreadStart(Run));
			_thread.Start();
		}

		public int MaxEvents { get; set; }

		#region ILog Members


		public void Log(LogLevels level, string sFormat, params Object[] args)
		{
			LogRecord record = new LogRecord(this.DateTimeProvider.GetCurrentDateTime(), level, null, sFormat, args);
			_records.Enqueue(record);
		}

		public void Log(DateTime date, LogLevels level, string sFormat, params object[] args)
		{
			LogRecord record = new LogRecord(date, level, null, sFormat, args);
			_records.Enqueue(record);
		}

		public void Log(LogLevels level, Dictionary<string, object> data, string sFormat, params object[] args)
		{
			LogRecord record = new LogRecord(this.DateTimeProvider.GetCurrentDateTime(), level, data, sFormat, args);
			_records.Enqueue(record);
		}

		public void Log(DateTime date, LogLevels level, Dictionary<string, object> data, string sFormat, params object[] args)
		{
			LogRecord record = new LogRecord(date, level, data, sFormat, args);
			_records.Enqueue(record);
		}

		#endregion

		/// <summary>
		/// Record of each message to be logged.
		/// </summary>
		private class LogRecord
		{
			private DateTime _date;
			private DateTime _insertionDate;
			private LogLevels _level;
			private string _messageFormat;
			private Object[] _args;
			private Dictionary<string, object> _data;

			/// <summary>
			/// Constructs a LogRecord object.
			/// </summary>
			/// <param name="level">Log level of the message.</param>
			/// <param name="messageFormat">Message to be logged.</param>
			public LogRecord(DateTime date, LogLevels level, Dictionary<string, object> data, string messageFormat, Object[] args)
			{
				_insertionDate = DateTime.UtcNow; // For bookkeeping
				_date = date;
				_level = level;

				// Make our own copy of the data
				if (data == null)
					_data = new Dictionary<string, object>();
				else
					_data = new Dictionary<string, object>(data);

				_messageFormat = messageFormat;
				_args = args;

				// Set this thread
				_data["thread"] = System.Threading.Thread.CurrentThread.GetHashCode().ToString();
			}

			/// <summary>
			/// Log the message to the supplied event log.
			/// </summary>
			/// <param name="log"></param>
			public LogglyEvent CreateEvent(Dictionary<string, object> logData)
			{
				// Log using our information
				// Start with the standard properties
				Dictionary<string, object> data = new Dictionary<string, object>(logData);

				// These fields are allowed to be overwritten by the caller
				data["thread"] = System.Threading.Thread.CurrentThread.GetHashCode().ToString();

				// Add the message data
				if (_data != null)
				{
					foreach (var kvp in _data)
					{
						data[kvp.Key] = kvp.Value;
					}
				}

				// Add the new stuff for this message
				data["message"] = String.Format(_messageFormat, _args);
				data["level"] = _level.ToString();
				data["date"] = _date;
				data["sendDelay"] = (DateTime.UtcNow - _insertionDate).TotalSeconds; // Bookkeeping

				// Serialize and dispatch
				var logglyEvent = new Loggly.LogglyEvent();
				foreach (var key in data.Keys)
				{
					logglyEvent.Data.Add(key, data[key]);
				}

				return logglyEvent;
			}
		}

		private bool _isSendAllMessages = false;
		private ManualResetEvent _evStop = new ManualResetEvent(false);

		/// <summary>
		/// Stops the thread.
		/// </summary>
		public void Stop()
		{
			_evStop.Set();
		}

		/// <summary>
		/// Stops the thread and waits for it to complete.
		/// </summary>
		public void StopAndWait()
		{
			_isSendAllMessages = true;
			_evStop.Set();
			_thread.Join();
		}

		private void Run()
		{
			while (true)
			{
				// Should we stop the thread?
				if (_evStop.WaitOne(5, false))
					break;

				// Process the logs until we run out
				while (ProcessLogs())
					;
			}

			// We need to stop the thread, but should
			// we send all left-over messages?
			if (_isSendAllMessages)
			{
				while (ProcessLogs())
					;
			}
		}

		private bool ProcessLogs()
		{
			// Get logs from the pending logs list
			List<LogglyEvent> events = new List<LogglyEvent>();
			for (int i = 0; i < this.MaxEvents; ++i)
			{
				LogRecord record = null;
				if (!_records.TryDequeue(out record))
					break;

				events.Add(record.CreateEvent(this.Data));
			}

			if (!events.Any())
				return false;

			if (this.IsSync)
			{
				var response = _client.Log(events).Result;
			}
			else
			{
				_client.Log(events).ConfigureAwait(false);
			}

			return true;
		}
	}
}
