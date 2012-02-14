using System;
using MbCache.Configuration;
using MbCache.Core;
using MbCache.DefaultImpl;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.SystemCheck
{
    public class CheckPermissionsForCastle : ISystemCheck
    {
        private static bool? _isOk;
        private static readonly object lockObject =new object();

        public bool IsRunningOk()
        {
            lock (lockObject)
            {
                if(!_isOk.HasValue)
                {
                    lock(lockObject)
                    {
                        setFlag();
                    }
                }
            }
            return _isOk.Value;
        }

        public static void Reset()
        {
            //to be able to run different tests
            _isOk = null;
        }

        private void setFlag()
        {
            var isOk = true;
            try
            {
                TryCreateTempPerson();
            }
            catch (ArgumentException)
            {
                isOk = false;
            }
            _isOk = isOk;
        }

        protected virtual void TryCreateTempPerson()
        {
            var cacheBuilder = new CacheBuilder();
            cacheBuilder.For<Person>()
                .As<IPerson>();
            var factory = cacheBuilder.BuildFactory(new AspNetCacheFactory(1), new ToStringMbCacheKey());
            factory.Create<IPerson>();
        }

        public string WarningText
        {
            get { return Resources.CheckPermissionsForCastle; }
        }
    }
}