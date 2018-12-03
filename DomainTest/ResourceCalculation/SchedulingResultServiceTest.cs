using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	public class SchedulingResultServiceTest
	{
		private SchedulingResultService _target;
		private PersonAssignmentListContainer _personAssignmentListContainer;
		private ISkillSkillStaffPeriodExtendedDictionary _skillStaffPeriods;
		private DateTimePeriod _inPeriod;
		private PersonSkillProvider _personSkillProvider;
		private IResourceCalculationDataContainerWithSingleOperation _resources;

		[SetUp]
		public void Setup()
		{
			_inPeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 15, 00, DateTimeKind.Utc));
			_personAssignmentListContainer = PersonAssignmentFactory.CreatePersonAssignmentListForActivityDividerTest();
			_skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills);
			_personSkillProvider = new PersonSkillProvider();

			_resources = new ResourceCalculationDataContainer(Enumerable.Empty<ExternalStaff>(), _personSkillProvider, 15, false, new ActivityDivider());
			var layers = _personAssignmentListContainer.TestVisualLayerCollection();
			foreach (var layer in layers)
			{
				foreach (var resourceLayer in layer.Item1.ToResourceLayers(15, TimeZoneInfo.Utc))
				{
					_resources.AddResources(layer.Item2, new DateOnly(2008,1,1), resourceLayer);
				}
			}

			_target = new SchedulingResultService(new SkillResourceCalculationPeriodWrapper(_skillStaffPeriods), 
				_personAssignmentListContainer.AllSkills,
				_resources,
				_personSkillProvider);
		}

		[Test]
		public void ShouldBeAbleToCreateInstanceFromScheduleDictionary()
		{
			var scheduleDictionary = new ScheduleDictionaryForTest(_personAssignmentListContainer.Scenario,
			                                                       new ScheduleDateTimePeriod(_inPeriod,
			                                                                                  _personAssignmentListContainer
				                                                                                  .ContainedPersons.Values),
			                                                       new Dictionary<IPerson, IScheduleRange>());
			var range = new ScheduleRange(scheduleDictionary,
			                              new ScheduleParameters(_personAssignmentListContainer.Scenario,
			                                                     _personAssignmentListContainer.ContainedPersons.First().Value,
			                                                     _inPeriod), new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());
			scheduleDictionary.AddTestItem(_personAssignmentListContainer.ContainedPersons.First().Value,range);
			range.Add(_personAssignmentListContainer.PersonAssignmentListForActivityDividerTest[0]);
			
			_target =
				new SchedulingResultService(
					new SchedulingResultStateHolder(_personAssignmentListContainer.ContainedPersons.Values, scheduleDictionary, new Dictionary<ISkill, IEnumerable<ISkillDay>>
						                                {
							                                {_personAssignmentListContainer.AllSkills[0], new List<ISkillDay>()}
						                                }),
					_personAssignmentListContainer.AllSkills, new PersonSkillProvider());
			Assert.IsNotNull(_target);
		}

		[Test]
		public void VerifySchedulingResultWithoutPeriod()
		{
			Assert.IsNotNull(_target);

			IResourceCalculationPeriodDictionary res;
			
			_target.SchedulingResult(_inPeriod, null, false).TryGetValue(_personAssignmentListContainer.ContainedSkills["PhoneA"], out res);
			
			var val = (SkillStaffPeriod)res.Items().First(
				s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value;
			
			Assert.AreEqual(0.99,val.CalculatedResource,
							0.01);
		}

		[Test]
		public void VerifySchedulingResultWithoutPeriodAndNoSchedules()
		{
			_target = new SchedulingResultService(new SkillResourceCalculationPeriodWrapper(_skillStaffPeriods), 
												  _personAssignmentListContainer.AllSkills,
												  new ResourceCalculationDataContainer(Enumerable.Empty<ExternalStaff>(), _personSkillProvider, 15, false, new ActivityDivider()),
												  _personSkillProvider);
			Assert.IsNotNull(_target);
			
			IResourceCalculationPeriodDictionary res;

			_target.SchedulingResult(_inPeriod, null, false).TryGetValue(_personAssignmentListContainer.ContainedSkills["PhoneA"], out res);
			var val = (SkillStaffPeriod)res.Items().First(
				s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value;

			Assert.AreEqual(0.0 / _inPeriod.ElapsedTime().TotalMinutes, val.CalculatedResource, 0.001);
		}

		[Test]
		public void VerifySchedulingPeriodDoNotIntersectSkillStaffPeriod()
		{
			var eminem = new SkillResourceCalculationPeriodWrapper(_skillStaffPeriods);
			DateTime skillDayDate = new DateTime(2009, 1, 2, 10, 00, 00, DateTimeKind.Utc);
			_skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills, skillDayDate);
			_target = new SchedulingResultService(eminem, 
				_personAssignmentListContainer.AllSkills,
				_resources,
				_personSkillProvider);

			var outDic = _target.SchedulingResult(_inPeriod, null, false);
			Assert.AreEqual(outDic, eminem);
		}

		[Test]
		public void ShouldNotCrashIfNoSkills()
		{
			var mock = new MockRepository();
			var stateHolder = mock.DynamicMock<ISchedulingResultStateHolder>();
			var holder = mock.DynamicMock<ISkillStaffPeriodHolder>();

			Expect.Call(stateHolder.SkillStaffPeriodHolder).Return(holder);
			Expect.Call(holder.SkillSkillStaffPeriodDictionary).Return(_skillStaffPeriods);
			mock.ReplayAll();
			_target = new SchedulingResultService(stateHolder, new List<ISkill>(), _personSkillProvider);
			Assert.IsNotNull(_target);
			mock.VerifyAll();
		}
	}
}
