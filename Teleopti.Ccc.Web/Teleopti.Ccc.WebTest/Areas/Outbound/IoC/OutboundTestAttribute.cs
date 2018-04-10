using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.IoC;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.Areas.Outbound.Core;

namespace Teleopti.Ccc.WebTest.Areas.Outbound.IoC
{
	class OutboundTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			var principalAuthorization = new FullPermission();
			CurrentAuthorization.DefaultTo(principalAuthorization);

			system.AddModule(new WebModule(configuration, null));
			system.AddModule(new OutboundAreaModule(false));

			system.AddService<FakeStorage>();
			system.UseTestDouble(new FakeScenarioRepository(new Scenario("Default") {DefaultScenario = true}))
				.For<IScenarioRepository>();
			system.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			system.UseTestDouble<FakeWorkloadRepository>().For<IWorkloadRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			system.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			system.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
			system.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
			system.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
			system.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
			system.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
			system.UseTestDouble<FakeScheduleTagRepository>().For<IScheduleTagRepository>();
			system.UseTestDouble<FakeWorkflowControlSetRepository>().For<IWorkflowControlSetRepository>();
			system.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
			system.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
			system.UseTestDouble<FakeCampaignRepository>().For<IOutboundCampaignRepository>();
			system.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			system.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
			system.UseTestDouble<FakeOutboundScheduledResourcesCacher>().For<IOutboundScheduledResourcesCacher>();
		}
	}
}
