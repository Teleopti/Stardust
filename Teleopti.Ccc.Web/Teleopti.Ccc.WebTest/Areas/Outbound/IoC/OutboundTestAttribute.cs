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
		protected override void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddModule(new WebModule(configuration, null));
			extend.AddModule(new OutboundAreaModule(false));
			extend.AddService<FakeStorage>();
		}

		protected override void Isolate(IIsolate isolate)
		{
			var principalAuthorization = new FullPermission();
			CurrentAuthorization.DefaultTo(principalAuthorization);

			isolate.UseTestDouble(new FakeScenarioRepository(new Scenario("Default") {DefaultScenario = true}))
				.For<IScenarioRepository>();
			isolate.UseTestDouble<FakeSkillRepository>().For<ISkillRepository>();
			isolate.UseTestDouble<FakeWorkloadRepository>().For<IWorkloadRepository>();
			isolate.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			isolate.UseTestDouble<FakeSkillDayRepository>().For<ISkillDayRepository>();
			isolate.UseTestDouble<FakeActivityRepository>().For<IActivityRepository>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
			isolate.UseTestDouble<FakeDayOffTemplateRepository>().For<IDayOffTemplateRepository>();
			isolate.UseTestDouble<FakeShiftCategoryRepository>().For<IShiftCategoryRepository>();
			isolate.UseTestDouble<FakeContractRepository>().For<IContractRepository>();
			isolate.UseTestDouble<FakeContractScheduleRepository>().For<IContractScheduleRepository>();
			isolate.UseTestDouble<FakeScheduleTagRepository>().For<IScheduleTagRepository>();
			isolate.UseTestDouble<FakeWorkflowControlSetRepository>().For<IWorkflowControlSetRepository>();
			isolate.UseTestDouble<FakePartTimePercentageRepository>().For<IPartTimePercentageRepository>();
			isolate.UseTestDouble<FakeMultiplicatorDefinitionSetRepository>().For<IMultiplicatorDefinitionSetRepository>();
			isolate.UseTestDouble<FakeCampaignRepository>().For<IOutboundCampaignRepository>();
			isolate.UseTestDouble<FakeRepositoryFactory>().For<IRepositoryFactory>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			isolate.UseTestDouble<FakeTimeZoneGuard>().For<ITimeZoneGuard>();
			isolate.UseTestDouble<FakeSkillTypeRepository>().For<ISkillTypeRepository>();
			isolate.UseTestDouble<FakeOutboundScheduledResourcesCacher>().For<IOutboundScheduledResourcesCacher>();
		}
	}
}
