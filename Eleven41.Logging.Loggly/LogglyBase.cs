using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eleven41.Logging
{
	public abstract class LogglyBase
	{
		protected Loggly.ILogglyClient _client;

		public LogglyBase()
		{
			_client = new Loggly.LogglyClient();

			this.Data = new Dictionary<string, object>();
			this.DateTimeProvider = new Eleven41.Logging.DateTimeProviders.UtcDateTimeProvider();
		}

		public LogglyBase(Dictionary<string, object> data)
		{
			_client = new Loggly.LogglyClient();

			if (data != null)
				this.Data = new Dictionary<string, object>(data);
			else
				this.Data = new Dictionary<string, object>();
			this.DateTimeProvider = new Eleven41.Logging.DateTimeProviders.UtcDateTimeProvider();
		}

		public static void SetLogglyCustomerToken(string customerToken)
		{
			Loggly.Config.LogglyConfig.Instance.CustomerToken = customerToken;
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

		IDateTimeProvider _dateTimeProvider;

		public IDateTimeProvider DateTimeProvider
		{
			get
			{
				return _dateTimeProvider;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException();
				_dateTimeProvider = value;
			}
		}
	}
}
