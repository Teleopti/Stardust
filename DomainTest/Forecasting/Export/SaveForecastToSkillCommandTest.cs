using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.DomainTest.Forecasting.Export
{
	[TestFixture]
	public class SaveForecastToSkillCommandTest
	{
		private ISkill _targetSkill;
		private ISaveForecastToSkillCommand _target;
		private MockRepository _mocks;
		private ISkillDayLoadHelper _skillDayLoadHelper;
		private IScenarioProvider _scenarioProvider;
		private ISkillDayRepository _skillDayRepository;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_targetSkill = SkillFactory.CreateSkillWithWorkloadAndSources();

			_skillDayRepository = _mocks.DynamicMock<ISkillDayRepository>();
			_scenarioProvider = _mocks.DynamicMock<IScenarioProvider>();
			_skillDayLoadHelper = _mocks.DynamicMock<ISkillDayLoadHelper>();

			_target = new SaveForecastToSkillCommand(_skillDayLoadHelper, _skillDayRepository, _scenarioProvider);
		}

		[Test]
		public void ShouldCopyForecastFromMultisiteSubSkillToSkill()
		{
			var period = new DateOnlyPeriod(2011, 1, 1, 2011, 6, 30);
			var targetScenario = ScenarioFactory.CreateScenarioAggregate();
			var skillDay = SkillDayFactory.CreateSkillDay(_targetSkill, period.StartDate, targetScenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(_targetSkill, new[] { skillDay }, period);
			skillDay.SplitSkillDataPeriods(skillDay.SkillDataPeriodCollection.ToList());

			_targetSkill.RemoveWorkload(_targetSkill.WorkloadCollection.First());
			_targetSkill.AddWorkload(skillDay.WorkloadDayCollection[0].Workload);

			var skillStaffPeriod = _mocks.DynamicMock<ISkillStaffPeriod>();
			var skillStaff = _mocks.DynamicMock<ISkillStaff>();
			var skillStaffPeriodDictionary = _mocks.DynamicMock<ISkillStaffPeriodDictionary>();
			var secondBusinessUnit = _mocks.DynamicMock<IBusinessUnit>();
			var intervalPeriod = new DateTimePeriod(skillDay.SkillDataPeriodCollection[0].Period.StartDateTime,
													skillDay.SkillDataPeriodCollection[0].Period.StartDateTime.AddMinutes(
														_targetSkill.DefaultResolution));

			ReflectionHelper.SetBusinessUnit(_targetSkill, secondBusinessUnit);
			ReflectionHelper.SetBusinessUnit(skillDay, secondBusinessUnit);

			using (_mocks.Record())
			{
				Expect.Call(_scenarioProvider.DefaultScenario(secondBusinessUnit)).Return(targetScenario);
				Expect.Call(_skillDayLoadHelper.LoadSchedulerSkillDays(period, new[] { _targetSkill }, targetScenario)).Return(
					new Dictionary<ISkill, IList<ISkillDay>> { { _targetSkill, new[] { skillDay } } });
				Expect.Call(_skillDayRepository.GetAllSkillDays(period, new[] { skillDay }, _targetSkill, targetScenario, false)).
					Return(new[] { skillDay });
				ISkillStaffPeriod dummyStaffPeriod;
				Expect.Call(skillStaffPeriodDictionary.TryGetValue(intervalPeriod,
																   out dummyStaffPeriod)).OutRef(skillStaffPeriod).Return(true);
				Expect.Call(skillStaffPeriod.Period).Return(intervalPeriod);
				Expect.Call(skillStaffPeriod.Payload).Return(skillStaff).Repeat.AtLeastOnce();
				Expect.Call(skillStaff.TaskData).Return(new Task(2, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4)));
				Expect.Call(skillStaff.ForecastedIncomingDemand).Return(3d);
				Expect.Call(skillStaff.Shrinkage).Return(new Percent(0.1));
			}
			using (_mocks.Playback())
			{
				_target.Execute(period, _targetSkill, skillStaffPeriodDictionary);
				skillDay.SkillDataPeriodCollection[0].ManualAgents.Should().Be.EqualTo(3d);
				skillDay.SkillDataPeriodCollection[0].Shrinkage.Value.Should().Be.EqualTo(0.1d);

				var taskPeriod = skillDay.WorkloadDayCollection[0].TaskPeriodList.FirstOrDefault(p => p.Period.StartDateTime == intervalPeriod.StartDateTime);
				taskPeriod.Tasks.Should().Be.EqualTo(2d);
				taskPeriod.AverageTaskTime.Should().Be.EqualTo(TimeSpan.FromSeconds(3));
				taskPeriod.AverageAfterTaskTime.Should().Be.EqualTo(TimeSpan.FromSeconds(4));

				((IBelongsToBusinessUnit)skillDay).BusinessUnit.Should().Be.EqualTo(secondBusinessUnit);
			}
		}	
        
        [Test]
		public void ShouldImportForecastsToSkill()
		{
			var period = new DateOnlyPeriod(2011, 1, 1, 2011, 1, 1);
			var targetScenario = ScenarioFactory.CreateScenarioAggregate();
			var skillDay = SkillDayFactory.CreateSkillDay(_targetSkill, period.StartDate, targetScenario);
			skillDay.SkillDayCalculator = new SkillDayCalculator(_targetSkill, new[] { skillDay }, period);
			skillDay.SplitSkillDataPeriods(skillDay.SkillDataPeriodCollection.ToList());

			_targetSkill.RemoveWorkload(_targetSkill.WorkloadCollection.First());
			_targetSkill.AddWorkload(skillDay.WorkloadDayCollection[0].Workload);

			var secondBusinessUnit = _mocks.DynamicMock<IBusinessUnit>();
            var intervalStartTime = new DateTime(2011, 1, 1, 6, 15, 0, DateTimeKind.Utc);
            var intervalEndTime = new DateTime(2011, 1, 1, 6, 30, 0, DateTimeKind.Utc);
			var intervalPeriod = new DateTimePeriod(intervalStartTime, intervalEndTime);

            var forecasts = new List<IForecastsFileRow>
                                {
                                    new ForecastsFileRow
                                        {
                                            Tasks = 12,
                                            TaskTime = 110.02,
                                            AfterTaskTime = 121.30,
                                            Agents = 2,
                                            UtcDateTimeFrom = intervalStartTime,
                                            UtcDateTimeTo = intervalEndTime,
                                            LocalDateTimeFrom = new DateTime(2011, 1, 1, 8, 15, 0),
                                            LocalDateTimeTo = new DateTime(2011, 1, 1, 8, 30, 0),
                                            SkillName = "Insurance"
                                        }
                                };

			ReflectionHelper.SetBusinessUnit(_targetSkill, secondBusinessUnit);
			ReflectionHelper.SetBusinessUnit(skillDay, secondBusinessUnit);

			using (_mocks.Record())
			{
				Expect.Call(_scenarioProvider.DefaultScenario(secondBusinessUnit)).Return(targetScenario);
				Expect.Call(_skillDayLoadHelper.LoadSchedulerSkillDays(period, new[] { _targetSkill }, targetScenario)).Return(
					new Dictionary<ISkill, IList<ISkillDay>> { { _targetSkill, new[] { skillDay } } });
				Expect.Call(_skillDayRepository.GetAllSkillDays(period, new[] { skillDay }, _targetSkill, targetScenario, false)).
					Return(new[] { skillDay });
			}
			using (_mocks.Playback())
			{
                _target.Execute(new DateOnly(2011, 1, 1), _targetSkill, forecasts, ImportForecastsMode.ImportWorkload);
			    skillDay.SkillDataPeriodCollection[0].ManualAgents.Should().Be.EqualTo(null);
				var taskPeriod = skillDay.WorkloadDayCollection[0].TaskPeriodList.FirstOrDefault(p => p.Period.StartDateTime == intervalPeriod.StartDateTime);
			    taskPeriod.Tasks.Should().Be.EqualTo(12);
			    taskPeriod.AverageTaskTime.Should().Be.EqualTo(TimeSpan.FromSeconds(110.02));
			    taskPeriod.AverageAfterTaskTime.Should().Be.EqualTo(TimeSpan.FromSeconds(121.30));
			}
		}
	}
}