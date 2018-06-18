using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class PermissionsTestAttribute : IoCTestAttribute
	{
		protected override void Extend(IExtend extend, IIocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
			extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
			isolate.UseTestDouble<FakeAvailableDataRepository>().For<IAvailableDataRepository>();
			isolate.UseTestDouble<FakeApplicationFunctionRepository>().For<IApplicationFunctionRepository>();
			isolate.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepository>();
			isolate.UseTestDouble<FakeLicenseRepository>().For<ILicenseRepositoryForLicenseVerifier>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<FakePersonInRoleQuerier>().For<IPersonInRoleQuerier>();
			isolate.UseTestDouble<FakeApplicationFunctionsToggleFilter>().For<IApplicationFunctionsToggleFilter>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}
	}

	
}