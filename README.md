# Eleven41.Logging.Loggly

Copyright (C) 2016, Eleven41 Software Inc.

Eleven41.Logging.Loggly is an implementation of Eleven41.Logging.ILog for sending logs to Loggly.com

## Get It on NuGet!

	Install-Package Eleven41.Logging.Loggly

## LICENSE
[MIT License](https://github.com/eleven41/Eleven41.Logging.Loggly/blob/master/LICENSE.md)

## REQUIREMENTS

* Visual Studio 2013

## Configuration

Under the hood, this library uses [loggly-csharp](https://github.com/neutmute/loggly-csharp). Please see
that project for configuring `App.config` with your Loggly access key.

## Sample Code

	ILog log = new LogglyLog();
	log.Log(LogLevels.Diagnostic, "This is my diagnostic message ");
	