using System;

namespace Teleopti.Interfaces.Infrastructure
{
    /// <summary>
    /// Constants for encryption purposes in the application
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2010-02-27
    /// </remarks>
    public static class EncryptionConstants
    {
        /// <summary>
        /// Image1
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static byte[] Image1 = Convert.FromBase64String("qUlC6P7VcuXBMZpUf9UOmfIyYYkebxup18VDyOh5818=");

        /// <summary>
        /// Image2
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static byte[] Image2 = Convert.FromBase64String("gKzdNmOqJpIVfTen3orkjg==");
    }
}
