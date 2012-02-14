﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// Static class containing consts used in Message Brokern.
    /// </summary>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Consts")]
    public static class Consts
    {
        /// <summary>
        /// Default character encoding.
        /// </summary>
        public const String DefaultCharEncoding = "ascii";
        /// <summary>
        /// Max Wire Length is 1024.
        /// </summary>
        public const int MaxWireLength = 1024;
        /// <summary>
        /// Separator, new line.
        /// </summary>
        public const char Separator = '\n';
        /// <summary>
        /// Min Date for SQL Server.
        /// </summary>
        public static readonly DateTime MinDate = new DateTime(1753, 1, 1, 12, 0, 0);
        /// <summary>
        /// Max Date for SQL Server.
        /// </summary>
        public static readonly DateTime MaxDate = new DateTime(9999, 12, 31, 11, 59, 59);
        /// <summary>
        /// Send signal
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static byte[] SendSignal = Encoding.GetEncoding(DefaultCharEncoding).GetBytes("Send");
    }
}