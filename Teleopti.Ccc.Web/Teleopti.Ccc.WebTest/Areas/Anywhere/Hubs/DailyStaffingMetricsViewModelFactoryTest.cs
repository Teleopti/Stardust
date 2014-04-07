using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Hubs
{
	[TestFixture]
	public class DailyStaffingMetricsViewModelFactoryTest
	{
		[Test]
		public void ShouldGetForecastedHours()
		{
			var dateTime = DateOnly.Today;
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("skill1");
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			
			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();
			
			var calculateSkillCommand = MockRepository.GenerateMock<IResourceCalculateSkillCommand>();
			var stateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var optimizationHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			var currentScenario = MockRepository.GenerateMock<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(scenario);

			skillRepository.Stub(x => x.Load(skillId)).Return(skill);
			
			skillDay.Stub(x => x.ForecastedIncomingDemand).Return(TimeSpan.FromHours(2.5));
			skillDay.Stub(x => x.CurrentDate).Return(dateTime);
			skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> {skillStaffPeriod}));
			skillStaffPeriod.Stub(x => x.Payload).Return(new SkillStaff(new Task(), ServiceAgreement.DefaultValues()));
			skillStaffPeriod.Stub(x => x.FStaffTime()).Return(TimeSpan.FromMinutes(2));
			stateHolder.Stub(x => x.SkillDays).Return(new Dictionary<ISkill, IList<ISkillDay>> {{skill, new List<ISkillDay> {skillDay}}});

			var factory = new DailyStaffingMetricsViewModelFactory(skillRepository, calculateSkillCommand, currentScenario, stateHolder, optimizationHelper);
			var result = factory.CreateViewModel(skillId, dateTime);

			result.ForecastedHours.Should().Be.EqualTo(2.5);
			calculateSkillCommand.AssertWasCalled(
				x =>
				x.Execute(Arg<IScenario>.Is.Equal(scenario),
				          Arg<DateTimePeriod>.Is.Equal(new DateOnlyPeriod(dateTime, dateTime).ToDateTimePeriod(skill.TimeZone)),
				          Arg<ISkill>.Is.Equal(skill),
				          Arg<ResourceCalculationDataContainerFromStorage>.Is.Anything));
		}

		[Test]
		public void ShouldGetScheduledHours()
		{
			var dateTime = DateOnly.Today;
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("skill1");
			var scenario = ScenarioFactory.CreateScenarioAggregate();

			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();

			var calculateSkillCommand = MockRepository.GenerateMock<IResourceCalculateSkillCommand>();
			var stateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var optimizationHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			var currentScenario = MockRepository.GenerateMock<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(scenario);

			skillRepository.Stub(x => x.Load(skillId)).Return(skill);

			skillDay.Stub(x => x.CurrentDate).Return(dateTime);
			skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillStaffPeriod.Stub(x => x.Payload).Return(new SkillStaff(new Task(), ServiceAgreement.DefaultValues()));
			skillStaffPeriod.Stub(x => x.FStaffTime()).Return(TimeSpan.FromMinutes(2));
			skillStaffPeriod.Stub(x => x.CalculatedResource).Return(2.0);
			skillStaffPeriod.Stub(x => x.Period).Return(new DateOnlyPeriod(dateTime, dateTime).ToDateTimePeriod(skill.TimeZone));
			stateHolder.Stub(x => x.SkillDays).Return(new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } });

			var factory = new DailyStaffingMetricsViewModelFactory(skillRepository, calculateSkillCommand, currentScenario, stateHolder, optimizationHelper);
			var result = factory.CreateViewModel(skillId, dateTime);

			var scheduledHours = SkillStaffPeriodHelper.ScheduledHours(skillDay.SkillStaffPeriodCollection);

			result.ScheduledHours.Should().Be.EqualTo(scheduledHours);
		}

		[Test]
		public void ShouldGetRelativeDifference()
		{
			var dateTime = DateOnly.Today;
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("skill1");
			var scenario = ScenarioFactory.CreateScenarioAggregate();

			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();

			var calculateSkillCommand = MockRepository.GenerateMock<IResourceCalculateSkillCommand>();
			var stateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var optimizationHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			var currentScenario = MockRepository.GenerateMock<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(scenario);

			skillRepository.Stub(x => x.Load(skillId)).Return(skill);

			skillDay.Stub(x => x.CurrentDate).Return(dateTime);
			skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillStaffPeriod.Stub(x => x.Payload).Return(new SkillStaff(new Task(), ServiceAgreement.DefaultValues()));
			skillStaffPeriod.Stub(x => x.FStaffTime()).Return(TimeSpan.FromDays(5));
			skillStaffPeriod.Stub(x => x.CalculatedResource).Return(2.0);
			skillStaffPeriod.Stub(x => x.Period).Return(new DateOnlyPeriod(dateTime, dateTime).ToDateTimePeriod(skill.TimeZone));
			stateHolder.Stub(x => x.SkillDays).Return(new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } });

			var factory = new DailyStaffingMetricsViewModelFactory(skillRepository, calculateSkillCommand, currentScenario, stateHolder, optimizationHelper);
			var result = factory.CreateViewModel(skillId, dateTime);

			var relativeDifference = SkillStaffPeriodHelper.RelativeDifferenceForDisplay(skillDay.SkillStaffPeriodCollection);

			result.RelativeDifference.Should().Be.EqualTo(relativeDifference.Value.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldGetAbsoluteDifference()
		{
			var dateTime = DateOnly.Today;
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("skill1");
			var scenario = ScenarioFactory.CreateScenarioAggregate();

			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();

			var calculateSkillCommand = MockRepository.GenerateMock<IResourceCalculateSkillCommand>();
			var stateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var optimizationHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			var currentScenario = MockRepository.GenerateMock<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(scenario);

			skillRepository.Stub(x => x.Load(skillId)).Return(skill);

			skillDay.Stub(x => x.CurrentDate).Return(dateTime);
			skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillStaffPeriod.Stub(x => x.Payload).Return(new SkillStaff(new Task(), ServiceAgreement.DefaultValues()));
			skillStaffPeriod.Stub(x => x.FStaffTime()).Return(TimeSpan.FromDays(5));
			skillStaffPeriod.Stub(x => x.AbsoluteDifference).Return(2.0);
			skillStaffPeriod.Stub(x => x.Period).Return(new DateOnlyPeriod(dateTime, dateTime).ToDateTimePeriod(skill.TimeZone));
			stateHolder.Stub(x => x.SkillDays).Return(new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } });

			var factory = new DailyStaffingMetricsViewModelFactory(skillRepository, calculateSkillCommand, currentScenario, stateHolder, optimizationHelper);
			var result = factory.CreateViewModel(skillId, dateTime);

			var absoluteDifference = SkillStaffPeriodHelper.AbsoluteDifference(skillDay.SkillStaffPeriodCollection, false, false).Value;

			result.AbsoluteDifferenceHours.Should().Be.EqualTo(absoluteDifference.TotalHours);
		}

		[Test]
		public void ShouldGetEstimatedServiceLevel()
		{
			var dateTime = DateOnly.Today;
			var skillId = Guid.NewGuid();
			var skill = SkillFactory.CreateSkill("skill1");
			var scenario = ScenarioFactory.CreateScenarioAggregate();

			var skillDay = MockRepository.GenerateMock<ISkillDay>();
			var skillStaffPeriod = MockRepository.GenerateMock<ISkillStaffPeriod>();

			var calculateSkillCommand = MockRepository.GenerateMock<IResourceCalculateSkillCommand>();
			var stateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			var skillRepository = MockRepository.GenerateMock<ISkillRepository>();
			var optimizationHelper = MockRepository.GenerateMock<IResourceOptimizationHelper>();
			var currentScenario = MockRepository.GenerateMock<ICurrentScenario>();
			currentScenario.Stub(x => x.Current()).Return(scenario);

			skillRepository.Stub(x => x.Load(skillId)).Return(skill);

			skillDay.Stub(x => x.CurrentDate).Return(dateTime);
			skillDay.Stub(x => x.SkillStaffPeriodCollection).Return(new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod }));
			skillStaffPeriod.Stub(x => x.Payload).Return(new SkillStaff(new Task(), ServiceAgreement.DefaultValues())
				{
					TaskData = new Task(2.0, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(2))
				});
			skillStaffPeriod.Stub(x => x.EstimatedServiceLevel).Return(new Percent(70.0));
			skillStaffPeriod.Stub(x => x.FStaffTime()).Return(TimeSpan.FromDays(5));
			skillStaffPeriod.Stub(x => x.AbsoluteDifference).Return(2.0);
			skillStaffPeriod.Stub(x => x.Period).Return(new DateOnlyPeriod(dateTime, dateTime).ToDateTimePeriod(skill.TimeZone));
			stateHolder.Stub(x => x.SkillDays).Return(new Dictionary<ISkill, IList<ISkillDay>> { { skill, new List<ISkillDay> { skillDay } } });

			var factory = new DailyStaffingMetricsViewModelFactory(skillRepository, calculateSkillCommand, currentScenario, stateHolder, optimizationHelper);
			var result = factory.CreateViewModel(skillId, dateTime);

			var estimatedServiceLevel = SkillStaffPeriodHelper.EstimatedServiceLevel(skillDay.SkillStaffPeriodCollection).Value;

			result.ESL.Should().Be.EqualTo(estimatedServiceLevel);
		}
	}
}