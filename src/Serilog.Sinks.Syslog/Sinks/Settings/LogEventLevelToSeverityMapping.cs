// Copyright 2018 Ionx Solutions (https://www.ionxsolutions.com)
// Ionx Solutions licenses this file to you under the Apache License, 
// Version 2.0. You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

namespace Serilog.Sinks.Syslog
{
    /// <summary>
    /// To try and compensate for a mistake in the original code that calculated the Syslog Priority by mapping
    /// <see cref="Events.LogEventLevel"/>s to <see cref="Severity"/>, we offer the ability to choose between
    /// three built-in mappings. There are also ways to provide your own mapping logic or simply implement your
    /// own <see cref="ISyslogFormatter"/> class.
    /// </summary>
    public enum LogEventLevelToSeverityMapping
    {
        /// <summary>
        /// Use the original mapping logic in which <see cref="Events.LogEventLevel.Verbose"/> was accidentally mapped
        /// to <see cref="Severity.Notice"/>. Otherwise, the mappings go from one to the other, name for name with
        /// <see cref="Events.LogEventLevel.Fatal"/> equaling <see cref="Severity.Emergency"/>.
        /// </summary>
        /// <remarks>
        /// LogEventLevel.Verbose => Severity.Notice
        /// LogEventLevel.Debug => Severity.Debug
        /// LogEventLevel.Information => Severity.Informational
        /// LogEventLevel.Warning => Severity.Warning
        /// LogEventLevel.Error => Severity.Error
        /// LogEventLevel.Fatal => Severity.Emergency
        /// </remarks>
        Original = 0,

        /// <summary>
        /// Change the mapping of the <see cref="Events.LogEventLevel.Verbose"/> to map to <see cref="Severity.Debug"/>
        /// instead of <see cref="Severity.Notice"/>. This means that both <see cref="Events.LogEventLevel.Verbose"/>
        /// and <see cref="Events.LogEventLevel.Debug"/> are mapped to <see cref="Severity.Debug"/>.
        /// </summary>
        /// <remarks>
        /// LogEventLevel.Verbose => Severity.Debug
        /// LogEventLevel.Debug => Severity.Debug
        /// LogEventLevel.Information => Severity.Informational
        /// LogEventLevel.Warning => Severity.Warning
        /// LogEventLevel.Error => Severity.Error
        /// LogEventLevel.Fatal => Severity.Emergency
        /// </remarks>
        VerboseToDebug = 1,

        /// <summary>
        /// Mapping between <see cref="Events.LogEventLevel"/> and <see cref="Severity"/> is done in increasing order of
        /// the enums. Most names will not match. This will become the default in the next major release.
        /// </summary>
        /// <remarks>
        /// LogEventLevel.Verbose => Severity.Debug
        /// LogEventLevel.Debug => Severity.Informational
        /// LogEventLevel.Information => Severity.Notice
        /// LogEventLevel.Warning => Severity.Warning
        /// LogEventLevel.Error => Severity.Error
        /// LogEventLevel.Fatal => Severity.Emergency
        /// </remarks>
        ValueOrder = 2,
    }
}
