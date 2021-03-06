// Copyright 2018 Ionx Solutions (https://www.ionxsolutions.com)
// Ionx Solutions licenses this file to you under the Apache License, 
// Version 2.0. You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;
using Shouldly;
using static Serilog.Sinks.Syslog.Tests.Fixture;

namespace Serilog.Sinks.Syslog.Tests
{
    public class CertificateFileProviderTests
    {
        [Fact]
        public void Can_open_certificate_from_disk()
        {
            var fileProvider = new CertificateFileProvider(ClientCertFilename, String.Empty);
            fileProvider.Certificate.ShouldNotBeNull();
            fileProvider.Certificate.Thumbprint.ShouldBe(ClientCertThumbprint, StringCompareShould.IgnoreCase);
        }

        [Fact]
        public void Should_throw_when_password_is_incorrect()
        {
            Should.Throw<CryptographicException>(() =>
                new CertificateFileProvider(ClientCertFilename, "myergen"));
        }

        [Fact]
        public void Should_throw_when_file_does_not_exist()
        {
            Should.Throw<FileNotFoundException>(() =>
                new CertificateFileProvider("myergen"));
        }

        [Fact]
        public void Should_throw_when_private_key_not_known()
        {
            Should.Throw<ArgumentException>(() =>
                new CertificateFileProvider(ClientCertWithoutKeyFilename));
        }
    }
}
