using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.People.IoC
{
	class WebPeopleTestAttribute:IoCTestAttribute
	{
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
			extend.AddModule(new PeopleAreaModule(configuration));
			extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
			isolate.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
			isolate.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
			isolate.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			isolate.UseTestDouble<FakeBusinessUnitRepository>().For<IBusinessUnitRepository>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble<FakeRuleSetBagRepository>().For<IRuleSetBagRepository>();
			isolate.UseTestDouble<FakeWorkShiftRuleSetRepository>().For<IWorkShiftRuleSetRepository>();
			isolate.UseTestDouble<FakeApplicationRoleRepository>().For<IApplicationRoleRepository>();
			isolate.UseTestDouble<FakePersonAccessRepository>().For<IRepository<IPersonAccess>>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakePermissions>().For<IAuthorization>();
		}
	}
}
