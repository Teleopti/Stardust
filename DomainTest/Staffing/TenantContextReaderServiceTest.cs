using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Staffing
{
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
	public class TenantContextReaderServiceTest 
		//: IIsolateSystem
	{
		//public ITenantAuditRepository TenantAuditRepository;
		//public TenantContextReaderService Target;
		//public FakeLoggedOnUser LoggedOnUser;
		//public FakeUserCulture UserCulture;

		//public void Isolate(IIsolate isolate)
		//{
		//	isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
		//	isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		//}

		//[Test]
		//public void ShouldLoadTenantAuditContext()
		//{
		//	TenantAuditRepository.Add(new TenantAudit(new Guid(), new Guid(), PersistActionIntent.AppLogonChange.ToString(), ""));
		//	Target.LoadAll().Should().Not.Be.Empty();
		//}
	}
}