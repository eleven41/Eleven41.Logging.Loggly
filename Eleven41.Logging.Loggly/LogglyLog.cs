using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eleven41.Logging
{
	public class LogglyLog : LogglyBase, ILog
    {
		public LogglyLog()
		{
		}

		public LogglyLog(Dictionary<string, object> data)
			: base(data)
		{
		}

		public void Log(LogLevels level, string sFormat, params object[] args)
		{
			// Call the data version
			Log(this.DateTimeProvider.GetCurrentDateTime(), level, null, sFormat, args);
		}

		public void Log(DateTime date, LogLevels level, string sFormat, params object[] args)
		{
			Log(date, level, null, sFormat, args);
		}

		public void Log(LogLevels level, Dictionary<string, object> messageData, string sFormat, params object[] args)
		{
			Log(this.DateTimeProvider.GetCurrentDateTime(), level, null, sFormat, args);
		}

		public void Log(DateTime date, LogLevels level, Dictionary<string, object> messageData, string sFormat, params object[] args)
		{
			if (_client == null)
				return;

			// Start with the standard properties
			Dictionary<string, object> data = new Dictionary<string, object>(this.Data);

			// These fields are allowed to be overwritten by the caller
			data["thread"] = System.Threading.Thread.CurrentThread.GetHashCode().ToString();

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
			var logglyEvent = new Loggly.LogglyEvent();
			foreach (var key in data.Keys)
			{
				logglyEvent.Data.Add(key, data[key]);
			}

			if (this.IsSync)
			{
				var response = _client.Log(logglyEvent).Result;
			}
			else
			{
				_client.Log(logglyEvent).ConfigureAwait(false);
			}
		}
	}
}
