// Copyright 2018 Ionx Solutions (https://www.ionxsolutions.com)
// Ionx Solutions licenses this file to you under the Apache License,
// Version 2.0. You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog.Events;
using Serilog.Parsing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using static Serilog.Sinks.Syslog.Tests.Fixture;

namespace Serilog.Sinks.Syslog.Tests
{
    public class CustomSeverityMappingTests
    {
        private readonly ITestOutputHelper output;
        private readonly Rfc3164Formatter formatter = new Rfc3164Formatter(Facility.User);
        private readonly CustomSeverityMappingFormatter customFormatter = new CustomSeverityMappingFormatter(Facility.User);
        private readonly DateTimeOffset timestamp;
        private readonly Regex regex;

        public CustomSeverityMappingTests(ITestOutputHelper output)
        {
            this.output = output;

            // Prepare a regex object that can be used to check the output format
            // NOTE: The regex is in a text file instead of as a variable - it's a but large, and all the escaping required to
            // have it as a variable just makes it hard to grok
            var patternFilename = GetFullPath("Rfc3164Regex.txt");
            this.regex = new Regex(File.ReadAllText(patternFilename), RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);

            // Timestamp used in tests
            var instant = new DateTime(2013, 12, 19, 4, 1, 2, 357) + TimeSpan.FromTicks(8523);
            this.timestamp = new DateTimeOffset(instant);
        }

        [Fact]
        public void Should_format_message_without_source_context()
        {
            var template = new MessageTemplateParser().Parse("This is a test message");

            // Test with LogEventLevel set to Verbose since that is what the example CustomSeverityMappingFormatter
            // class differs from the regular Rfc3164Formatter class.
            var logEvent = new LogEvent(this.timestamp, LogEventLevel.Verbose, null, template, Enumerable.Empty<LogEventProperty>());

            var formatted = this.formatter.FormatMessage(logEvent);
            this.output.WriteLine($"RFC3164 without source context: {formatted}");

            var match = this.regex.Match(formatted);
            match.Success.ShouldBeTrue();

            match.Groups["pri"].Value.ShouldBe("<13>");

            formatted = this.customFormatter.FormatMessage(logEvent);
            this.output.WriteLine($"Derived RFC3164 formatter with custom severity mapping: {formatted}");

            match = this.regex.Match(formatted);
            match.Success.ShouldBeTrue();

            match.Groups["pri"].Value.ShouldBe("<15>");
        }
    }

    /// <summary>Override the Syslog <see cref="Severity"/> mapping such that <see cref="LogEventLevel.Verbose"/>
    /// is mapped to <see cref="Severity.Debug"/>.</summary>
    public class CustomSeverityMappingFormatter : Rfc3164Formatter
    {
        /// <summary>Override the Syslog <see cref="Severity"/> mapping such that <see cref="LogEventLevel.Verbose"/>
        /// is mapped to <see cref="Severity.Debug"/>.</summary>
        public CustomSeverityMappingFormatter(Facility facility) : base(facility)
        {

        }

        public override int CalculatePriority(LogEventLevel level)
        {
            var severity = MapLogLevelToSeverity(level);
            return ((int)this.facility * 8) + (int)severity;
        }

        private static Severity MapLogLevelToSeverity(LogEventLevel logEventLevel)
            => logEventLevel switch
            {
                // A change from the default is that we will explicitly map Verbose to Severity.Debug. In the default
                // mapping, Verbose got mapped to Severity.Notice.
                LogEventLevel.Verbose => Severity.Debug,
                LogEventLevel.Debug => Severity.Debug,
                LogEventLevel.Error => Severity.Error,
                LogEventLevel.Fatal => Severity.Emergency,
                LogEventLevel.Information => Severity.Informational,
                LogEventLevel.Warning => Severity.Warning,
                _ => Severity.Notice
            };
    }
}
