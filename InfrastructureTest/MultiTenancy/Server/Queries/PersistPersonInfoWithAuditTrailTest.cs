using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Aspects;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	[TestFixture]
	[InfrastructureTest]
	[AllTogglesOn]
	public class PersistPersonInfoWithAuditTrailTest : IExtendSystem
	{
		private Tenant tenant;
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		public IPersistPersonInfo Target;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<PersistPersonInfo>(true);
			extend.AddService<TenantAuditAttribute>();
			extend.AddService<FakeCurrentHttpContext>();
		}

		[SetUp]
		public void InsertPreState()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			_tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
			tenant = new Tenant(RandomName.Make());
			_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
		}

		[TearDown]
		public void Cleanup()
		{
			_tenantUnitOfWorkManager.CurrentSession().CreateSQLQuery("TRUNCATE TABLE tenant.audit").ExecuteUpdate();
			_tenantUnitOfWorkManager.Dispose();
		}

		[Test]
		public void ShouldInsertNonExistingPersonInfoWithId()
		{
			var id = Guid.NewGuid();
			var session = _tenantUnitOfWorkManager.CurrentSession();

			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetIdentity("DOMAIN/User1");
			Target.Persist(new GenericPersistApiCallActionObj(){PersonInfo = personInfo});

			session.Flush();
			session.Clear();

			session.Get<PersonInfo>(personInfo.Id).Id.Should().Be.EqualTo(id);
		}



		//[Test]
		//public void ShouldNotPersistEmptyApplicationLogonPersonInfoRecords()
		//{
		//	var session = _tenantUnitOfWorkManager.CurrentSession();

		//	var personInfo1 = new PersonInfo(tenant, Guid.NewGuid());
		//	target.PersistApplicationLogonName(personInfo1);
		//	Assert.DoesNotThrow(session.Flush);

		//	var personInfo2 = new PersonInfo(tenant, Guid.NewGuid());
		//	target.PersistApplicationLogonName(personInfo2);
		//	Assert.DoesNotThrow(session.Flush);

		//	var result = session.Query<PersonInfo>().ToList();
		//	result.Count.Should().Be(0);
		//}




		//[Test]
		//public void IdentityUpdateShouldGenerateAuditTrailRecord()
		//{
		//	var session = _tenantUnitOfWorkManager.CurrentSession();

		//	var personInfo = new PersonInfo(tenant, Guid.NewGuid());
		//	personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), "password1", new OneWayEncryption());
		//	personInfo.SetIdentity(RandomName.Make());
		//	target.PersistIdentity(personInfo);

		//	var p1 = session.Get<PersonInfo>(personInfo.Id);
		//	var auditRecords = session.Query<TenantAudit>().ToList();

		//	auditRecords.Count.Should().Be.EqualTo(1);
		//	var auditRecord = auditRecords.Single();
		//	auditRecord.ActionPerformedOn.Should().Be.EqualTo(p1.Id);
		//	auditRecord.Action.Should().Be.EqualTo(PersistActionIntent.IdentityChange.ToString());
		//	auditRecord.Correlation.Should().Be.EqualTo(_tenantUnitOfWorkManager.CurrentSessionId());


		//}



	}
}