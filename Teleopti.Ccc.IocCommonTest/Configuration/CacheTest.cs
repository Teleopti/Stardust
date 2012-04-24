using System;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
    [TestFixture]
    public class CacheTest
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), 
		System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.IocCommon.Configuration.MbCacheModule"), Test]
        public void CacheProviderMustBeSet()
        {
			Assert.Throws<InvalidOperationException>(() => new MbCacheModule(null));
        }
    }
}