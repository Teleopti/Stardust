using System;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class AuditSettingProviderTest
	{
		[Test]
		public void ShouldReturnDelegateValue()
		{
			var aSetting = new AuditSetting();
			Func<ISession, IAuditSetting> deleg = s => aSetting;
			var provider = new AuditSettingProvider(deleg);

			provider.Entity(null).Should().Be.SameInstanceAs(aSetting);
		}

		[Test]
		public void ShouldThrowIfNullIsPassedToCtor()
		{
			Assert.Throws<ArgumentNullException>(() => new AuditSettingProvider(null));
		}


		[Test]
		public void ShouldCallDelegateOnlyOnce()
		{
			var wasCalled = false;
			var provider = new AuditSettingProvider(s =>
			                                       {
			                                          if (wasCalled)
			                                             Assert.Fail("Should call delegate only once!");
			                                          wasCalled = true;
			                                          return new AuditSetting();
			                                       });
			provider.Entity(null);
			provider.Entity(null);
		}
	}
}