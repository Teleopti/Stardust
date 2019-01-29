using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
	public class UpdateSkillForecastReadModelHandlerTest : IIsolateSystem
	{
		public UpdateSkillForecastReadModelHandler Target;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public MutableNow Now;
		public FakeSkillRepository SkillRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeScenarioRepository ScenarioRepository;
		public ISkillForecastReadModelRepository SkillForecastReadModelRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeTenants Tenants;

		public void Isolate(IIsolate isolate)
		{

		}

		[Test]
		public void VerifyIfTheJobIsSuccessful()
		{

			Tenants.Has("Teleopti WFM");
			var person = PersonFactory.CreatePerson().WithId(SystemUser.Id);
			PersonRepository.Add(person);
			var bu = BusinessUnitFactory.CreateWithId("something");
			BusinessUnitRepository.Add(bu);
			
			var dtp = new DateOnlyPeriod(new DateOnly(2019, 2, 16), new DateOnly(2019, 2, 19));
			var now = new DateTime(2019, 2, 17, 16, 0, 0);
			IntervalLengthFetcher.Has(15);
			Now.Is(now);
			var skillOpenHours = new TimePeriod(8, 9);
			var skill = createPhoneSkill(15, "phone", skillOpenHours);
			var scenario = ScenarioFactory.CreateScenarioWithId("x", true);
			scenario.SetBusinessUnit(bu);
			scenario.DefaultScenario = true;
			var skillDay = SkillSetupHelper.CreateSkillDayWithDemand(skill, scenario, new DateTime(2019, 2, 17), new TimePeriod(9, 10), 15.7, 10, 200);
			SkillRepository.Add(skill);
			SkillDayRepository.Add(skillDay);
			ScenarioRepository.Add(scenario);

			Target.Handle(new ForecastChangedEvent()
			{
				LogOnBusinessUnitId = bu.Id.GetValueOrDefault(),
				LogOnDatasource = "Teleopti WFM",
				SkillDayIds = new []{skillDay.Id.GetValueOrDefault()}
			});

			var skillStaffIntervals = SkillForecastReadModelRepository.LoadSkillForecast(new[] { skill.Id.GetValueOrDefault() }, new DateTimePeriod(dtp.StartDate.Utc(), dtp.EndDate.Date.Utc()));
			skillStaffIntervals.Count.Should().Be.EqualTo(4);

		}

		protected ISkill createPhoneSkill(int intervalLength, string skillName, TimePeriod openHours)
		{
			var skill =
				new Domain.Forecasting.Skill(skillName, skillName, Color.Empty, intervalLength, new SkillTypePhone(new Description("SkillTypeInboundTelephony"), ForecastSource.InboundTelephony))
				{
					TimeZone = TimeZoneInfo.Utc,
					Activity = new Activity("activity_" + skillName).WithId()
				}.WithId();
			var workload = WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHours);
			workload.SetId(Guid.NewGuid());

			return skill;
		}
	}
}