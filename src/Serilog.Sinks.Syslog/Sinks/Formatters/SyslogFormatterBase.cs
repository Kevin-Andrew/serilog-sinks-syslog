// Copyright 2018 Ionx Solutions (https://www.ionxsolutions.com)
// Ionx Solutions licenses this file to you under the Apache License,
// Version 2.0. You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;
using System.IO;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Serilog.Sinks.Syslog
{
    /// <summary>
    /// Base class for formatters that output Serilog events in syslog formats
    /// </summary>
    /// <remarks>
    /// We purposely don't use Serilog's ITextFormatter to format syslog messages, so that users of this library
    /// can use their own ITextFormatter instances to control the format of the 'body' part of each message
    /// </remarks>
    public abstract class SyslogFormatterBase : ISyslogFormatter
    {
        protected readonly Facility facility;
        protected readonly MessageTemplateTextFormatter templateFormatter;
        protected readonly LogEventLevelToSeverityMapping severityMapping;
        protected readonly string Host;
        protected static readonly string ProcessId = Process.GetCurrentProcess().Id.ToString();
        protected static readonly string ProcessName = Process.GetCurrentProcess().ProcessName;

        protected SyslogFormatterBase(
            Facility facility,
            MessageTemplateTextFormatter templateFormatter,
            string sourceHost = null,
            LogEventLevelToSeverityMapping severityMapping = LogEventLevelToSeverityMapping.VerboseToDebug)
        {
            this.facility = facility;
            this.templateFormatter = templateFormatter;
            this.severityMapping = severityMapping;

            // Use source hostname override, if specified
            this.Host = String.IsNullOrEmpty(sourceHost)
                ? Environment.MachineName.WithMaxLength(255)
                : sourceHost.WithMaxLength(255);
        }

        public abstract string FormatMessage(LogEvent logEvent);

        public virtual int CalculatePriority(LogEventLevel level)
        {
            var severity = this.severityMapping switch
            {
                LogEventLevelToSeverityMapping.Original => MapLogLevelToSeverityOriginal(level),
                LogEventLevelToSeverityMapping.VerboseToDebug => MapLogLevelToSeverityVerboseToDebug(level),
                LogEventLevelToSeverityMapping.ValueOrder => MapLogLevelToSeverityByValueOrder(level),
                _ => throw new ArgumentOutOfRangeException(nameof(this.severityMapping), $"The value {this.severityMapping} is not a valid LogEventLevelToSeverityMapping.")
            };

            return ((int)this.facility * 8) + (int)severity;
        }

        private static Severity MapLogLevelToSeverityOriginal(LogEventLevel logEventLevel)
            => logEventLevel switch
            {
                LogEventLevel.Debug => Severity.Debug,
                LogEventLevel.Information => Severity.Informational,
                LogEventLevel.Warning => Severity.Warning,
                LogEventLevel.Error => Severity.Error,
                LogEventLevel.Fatal => Severity.Emergency,
                _ => Severity.Notice
            };

        private static Severity MapLogLevelToSeverityVerboseToDebug(LogEventLevel logEventLevel)
            => logEventLevel switch
            {
                // Unfortunately, in previous versions, LogEventLevel.Verbose was mapped to Severity.Notice, which was not
                // intuitive. We are remapping it to Severity.Debug which may be considered a breaking change, but it is
                // in preparation for the next major release. We also realize that with this change, there are now two
                // LogEventLevel enum values that map to the same Syslog Severity value, thus not making them distinguishable
                // when looking at the Syslog's Priority property of the message.
                //
                // This behavior can be changed by using the new mapping logic that will become the default in the next major
                // release. Note however, that using the new mapping logic can also be considered a breaking change if you
                // utilize the Syslog's Priority property and are expecting certain values to map to your program's log messages.
                // Those values may have changed.
                LogEventLevel.Verbose => Severity.Debug,
                LogEventLevel.Debug => Severity.Debug,
                LogEventLevel.Information => Severity.Informational,
                LogEventLevel.Warning => Severity.Warning,
                LogEventLevel.Error => Severity.Error,
                LogEventLevel.Fatal => Severity.Emergency,
                _ => throw new ArgumentOutOfRangeException(nameof(logEventLevel), $"The value {logEventLevel} is not a valid LogEventLevel.")
            };

        private static Severity MapLogLevelToSeverityByValueOrder(LogEventLevel logEventLevel)
            => logEventLevel switch
            {
                LogEventLevel.Verbose => Severity.Debug,
                LogEventLevel.Debug => Severity.Informational,
                LogEventLevel.Information => Severity.Notice,
                LogEventLevel.Warning => Severity.Warning,
                LogEventLevel.Error => Severity.Error,
                LogEventLevel.Fatal => Severity.Emergency,
                _ => throw new ArgumentOutOfRangeException(nameof(logEventLevel), $"The value {logEventLevel} is not a valid LogEventLevel.")
            };

        protected string RenderMessage(LogEvent logEvent)
        {
            if (this.templateFormatter != null)
            {
                using var sw = new StringWriter();

                this.templateFormatter.Format(logEvent, sw);
                return sw.ToString();
            }

            return logEvent.RenderMessage();
        }
    }
}
