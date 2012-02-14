using System;
using System.ServiceModel;

namespace Teleopti.Ccc.Sdk.Common.Contracts
{
    /// <summary>
    /// Common operations
    /// </summary>
    [ServiceContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/",
        Name = "TeleoptiCccSdkService",
        ConfigurationName = "Teleopti.Ccc.Sdk.Common.Contracts.ITeleoptiCccSdkService")]
    public interface ITeleoptiCccSdkService
    {
        /// <summary>
        /// Converts a local date and time in a given time zone to UTC date and time.
        /// </summary>
        /// <param name="localDateTime">The local date and time information.</param>
        /// <param name="fromTimeZoneId">The local time zone information.</param>
        /// <returns></returns>
        [OperationContract]
        [Obsolete("This will be removed")]
        DateTime ConvertToUtc(DateTime localDateTime, string fromTimeZoneId);

        /// <summary>
        /// Converts the UTC date and time to date and time in specified <paramref name="toTimeZoneId"/>.
        /// </summary>
        /// <param name="utcDateTime">The UTC date time.</param>
        /// <param name="toTimeZoneId">To target time zone id.</param>
        /// <returns></returns>
        [OperationContract]
        [Obsolete("This will be removed")]
        DateTime ConvertFromUtc(DateTime utcDateTime, string toTimeZoneId);
    }
}
