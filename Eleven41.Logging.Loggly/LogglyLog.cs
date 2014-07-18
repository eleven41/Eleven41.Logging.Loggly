using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eleven41.Logging
{
    public class LogglyLog : ILog
    {
		private Loggly.Logger _logger;

		public LogglyLog()
		{
			string logglyKey = System.Configuration.ConfigurationManager.AppSettings["Loggly.Key"];
			if (String.IsNullOrEmpty(logglyKey))
				throw new Exception("Loggly.Key empty or missing from appSettings.");

			_logger = new Loggly.Logger(logglyKey);
			this.Data = new Dictionary<string, object>();
		}

		public LogglyLog(Dictionary<string, object> data)
		{
			string logglyKey = System.Configuration.ConfigurationManager.AppSettings["Loggly.Key"];
			if (String.IsNullOrEmpty(logglyKey))
				throw new Exception("Loggly.Key empty or missing from appSettings.");

			_logger = new Loggly.Logger(logglyKey);
			if (data != null)
				this.Data = new Dictionary<string, object>(data);
			else
				this.Data = new Dictionary<string, object>();
		}

		public LogglyLog(string logglyKey)
		{
			_logger = new Loggly.Logger(logglyKey);
			this.Data = new Dictionary<string, object>();
		}

		public LogglyLog(string logglyKey, Dictionary<string, object> data)
		{
			_logger = new Loggly.Logger(logglyKey);
			if (data != null)
				this.Data = new Dictionary<string, object>(data);
			else
				this.Data = new Dictionary<string, object>();
		}

		public LogglyLog(LogglyLog other)
		{
			_logger = other._logger;
			this.Data = new Dictionary<string, object>();
		}

		public LogglyLog(LogglyLog other, Dictionary<string, object> data)
		{
			_logger = other._logger;
			if (data != null)
				this.Data = new Dictionary<string, object>(data);
			else
				this.Data = new Dictionary<string, object>();
		}

		private Dictionary<string, object> _data;

		public Dictionary<string, object> Data
		{
			get { return _data; }
			set 
			{
				// Ensure we this.Data is never null
				if (value == null)
					throw new ArgumentNullException("Data", "Data must not be null");

				_data = value; 
			}
		}

		public bool IsSync { get; set; }

		public void Log(LogLevels level, string sFormat, params object[] args)
		{
			// Call the data version
			Log(DateTime.UtcNow, level, null, sFormat, args);
		}

		public void Log(DateTime date, LogLevels level, string sFormat, params object[] args)
		{
			Log(date, level, null, sFormat, args);
		}

		public void Log(LogLevels level, Dictionary<string, object> messageData, string sFormat, params object[] args)
		{
			Log(DateTime.UtcNow, level, null, sFormat, args);
		}

		public void Log(DateTime date, LogLevels level, Dictionary<string, object> messageData, string sFormat, params object[] args)
		{
			if (_logger == null)
				return;

			// Start with the standard properties
			Dictionary<string, object> data = new Dictionary<string, object>(this.Data);

			// These fields are allowed to be overwritten by the caller
			data["thread"] = System.Threading.Thread.CurrentThread.GetHashCode();

			// Add the message data
			if (messageData != null)
			{
				foreach (var kvp in messageData)
				{
					data[kvp.Key] = kvp.Value;
				}
			}

			// Add the new stuff for this message
			data["message"] = String.Format(sFormat, args);
			data["level"] = level.ToString();
			data["date"] = date;

			// Serialize and dispatch
			string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
			if (this.IsSync)
				_logger.LogSync(json, true);
			else
				_logger.Log(json, true);
		}
	}
}
