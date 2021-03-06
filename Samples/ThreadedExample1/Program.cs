﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eleven41.Logging;

namespace ThreadedExample1
{
	class Program
	{
		static void Main(string[] args)
		{
			var log = new ThreadedLogglyLog(new Dictionary<string, object>()
				{
					{ "app", "Test1" }
				});

			log.Log(LogLevels.Diagnostic, "Diagnostic {0}, {1}, {2}", 1, 2, 3);
			log.Log(LogLevels.Info, "Info {0}, {1}, {2}", 1, 2, 3);
			log.Log(LogLevels.Warning, "Warning {0}, {1}, {2}", 1, 2, 3);
			log.Log(LogLevels.Error, "Error {0}, {1}, {2}", 1, 2, 3);

			// Wait for all messages to be sent
			log.StopAndWait();
		}
	}
}
