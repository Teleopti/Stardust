using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;

namespace Teleopti.Ccc.InfrastructureTest.NHibernateConfiguration
{
	[TestFixture]
	public class EnversConfigurationDelegateTest
	{
		private EnversConfiguration target;

		[SetUp]
		public void Setup()
		{
			target = new EnversConfiguration();
		}

		[Test]
		public void ShouldThrowIfAuditSettingRowDoesNotExist()
		{
			var session = MockRepository.GenerateMock<ISession>();
			var tempSession = MockRepository.GenerateMock<ISession>();
			session.Expect(s => s.GetSession(EntityMode.Poco)).Return(tempSession);
			tempSession.Expect(s => s.Get<AuditSetting>(AuditSetting.TheId)).Return(null);
			Assert.Throws<DataSourceException>(() =>
			            ((AuditSetter) target.AuditSettingProvider).Entity(session));
		}
	}
}