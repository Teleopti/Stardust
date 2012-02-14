using System;
using System.Web;
using System.Web.Caching;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.WcfHost
{
    public class SdkState : State
    {
        private const string ApplicationDataKey = "applicationData";
        
        public override void SetSessionData(ISessionData sessionData)
        {
        }

        public override void SetApplicationData(IApplicationData applicationData)
        {
            base.SetApplicationData(applicationData);
            HttpRuntime.Cache.Add(ApplicationDataKey,applicationData,null,Cache.NoAbsoluteExpiration,TimeSpan.FromMinutes(30),CacheItemPriority.Normal,null);
        }

        public override IApplicationData ApplicationScopeData
        {
            get
            {
                var result = HttpRuntime.Cache.Get(ApplicationDataKey) as IApplicationData;
                return result ?? base.ApplicationScopeData;
            }
        }

        public override void ClearSession()
        {
        }

        public override ISessionData SessionScopeData
        {
            get { return new SessionData(); }
        }
    }
}