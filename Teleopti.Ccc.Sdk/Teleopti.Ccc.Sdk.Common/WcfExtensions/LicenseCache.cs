using System;
using System.Web;
using System.Web.Caching;
using log4net;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Common.WcfExtensions
{
    public class LicenseCache : ILicenseCache
    {
        private readonly Cache _cache = HttpRuntime.Cache;
        private readonly static ILog Logger = LogManager.GetLogger(typeof (LicenseCache));
        const string key = "License";

		  public void Add(LicenseVerificationResultDto licenseVerificationResultDto, string tenant)
        {
            Logger.Debug("Adding new license to cache.");
            _cache.Add(key+tenant, licenseVerificationResultDto, null, DateTime.Now.AddMinutes(60), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

		  public LicenseVerificationResultDto Get(string tenant)
        {
            Logger.Debug("Fetching license from cache.");
            return (LicenseVerificationResultDto)_cache[key+tenant];
        }
    }

    public interface ILicenseCache
    {
        void Add(LicenseVerificationResultDto licenseVerificationResultDto, string tenant);
        LicenseVerificationResultDto Get( string tenant);
    }
}