using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
	[DomainTest]
	[TestFixture]
	public class SkillStaffingDataLoaderTest : ISetup
	{
		public ISkillStaffingDataLoader Target;
		public ILoggedOnUser User;
		public MutableNow Now;
		public FakeSkillRepository SkillRepository;
		public FakeSkillTypeRepository SkillTypeRepository;
		public SkillIntradayStaffingFactory SkillIntradayStaffingFactory;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			var scenarioRepository = new FakeScenarioRepository();
			scenarioRepository.Has("Default");
			system.UseTestDouble(scenarioRepository).For<IScenarioRepository>();
		}

		[Test]
		public void ShouldRecalculateForecastedAgentsForEmailSkill()
		{
			var emailSkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId();

			var skill1 = createSkill("skill1", new TimePeriod(0, 24));
			skill1.DefaultResolution = 60;
			skill1.SkillType = emailSkillType;

			var utcDate = Now.UtcDateTime().Date;

			var staffingPeriodDataList = new List<StaffingPeriodData>
			{
				createStaffingPeriodData(1.25d, 0d, utcDate, 0, 1),
				createStaffingPeriodData(1.25d, 0d, utcDate, 1, 2),
				createStaffingPeriodData(1.25d, 0d, utcDate, 2, 3),
				createStaffingPeriodData(1.25d, 0d, utcDate, 3, 4),
				createStaffingPeriodData(0d, 0d, utcDate, 4, 5),
				createStaffingPeriodData(0d, 0d, utcDate, 5, 6),
				createStaffingPeriodData(0.012d, 0d, utcDate, 6, 7),
				createStaffingPeriodData(0.036d, 0d, utcDate, 7, 8),
				createStaffingPeriodData(0.093d, 1d, utcDate, 8, 9),
				createStaffingPeriodData(0.185d, 1d, utcDate, 9, 10),
				createStaffingPeriodData(0.286d, 0d, utcDate, 10, 11),
				createStaffingPeriodData(0.379d, 0d, utcDate, 11, 12),
				createStaffingPeriodData(0.421d, 1d, utcDate, 12, 13),
				createStaffingPeriodData(0.42d, 1d, utcDate, 13, 14),
				createStaffingPeriodData(0.39d, 1d, utcDate, 14, 15),
				createStaffingPeriodData(0.375d, 1d, utcDate, 15, 16),
				createStaffingPeriodData(0.343d, 1d, utcDate, 16, 17),
				createStaffingPeriodData(0.335d, 0d, utcDate, 17, 18),
				createStaffingPeriodData(0.33d, 0d, utcDate, 18, 19),
				createStaffingPeriodData(0.323d, 0d, utcDate, 19, 20),
				createStaffingPeriodData(0.315d, 0d, utcDate, 20, 21),
				createStaffingPeriodData(0.307d, 0d, utcDate, 21, 22),
				createStaffingPeriodData(0.23d, 0d, utcDate, 22, 23)
			};

			SkillIntradayStaffingFactory.SetupIntradayStaffingForSkill(skill1, new DateOnly(utcDate), staffingPeriodDataList,
				User.CurrentUser().PermissionInformation.DefaultTimeZone(), ServiceAgreement.DefaultValuesEmail());

			var skillStaffingDatas = Target.Load(new[] {skill1}, new DateOnly(utcDate).ToDateOnlyPeriod(), true);
			Assert.AreEqual(0.030,
				skillStaffingDatas.FirstOrDefault(p => p.Time.Equals(utcDate.AddHours(7))).ForecastedStaffing);
		}

		private ISkill createSkill(string name, TimePeriod? openHour = null)
		{
			var skill = SkillFactory.CreateSkill(name).WithId();
			skill.StaffingThresholds = createStaffingThresholds();
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, openHour ?? new TimePeriod(8, 00, 9, 30));
			SkillRepository.Has(skill);
			return skill;
		}

		private static StaffingThresholds createStaffingThresholds()
		{
			return new StaffingThresholds(new Percent(-0.3), new Percent(-0.1), new Percent(0.1));
		}

		private StaffingPeriodData createStaffingPeriodData(double forecastedStaffing, double scheduledStaffing, DateTime date, int startHour, int endHour)
		{
			return new StaffingPeriodData
			{
				ForecastedStaffing = forecastedStaffing,
				ScheduledStaffing = scheduledStaffing,
				Period = new DateTimePeriod(date.Date.AddHours(startHour), date.Date.AddHours(endHour))
			};
		}
	}
}
