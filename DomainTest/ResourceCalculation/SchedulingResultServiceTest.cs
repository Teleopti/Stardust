﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
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

			_resources = new ResourceCalculationDataContainer(_personSkillProvider, 15);
			var layers = _personAssignmentListContainer.TestVisualLayerCollection();
			foreach (var layer in layers)
			{
				foreach (var resourceLayer in layer.ToResourceLayers(15))
				{
					_resources.AddResources(layer.Person, new DateOnly(2008,1,1), resourceLayer);
				}
			}

			_target = new SchedulingResultService(_skillStaffPeriods,
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
			                                                     _inPeriod), new PersistableScheduleDataPermissionChecker());
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

			ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult(_inPeriod);
			Assert.AreEqual(0.83,
							outDic[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
								s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value.Payload.CalculatedResource,
							0.01);
		}

		[Test]
		public void VerifySchedulingResultWithoutPeriodAndNoSchedules()
		{
			_target = new SchedulingResultService(_skillStaffPeriods,
												  _personAssignmentListContainer.AllSkills,
												  new ResourceCalculationDataContainer(_personSkillProvider, 15), 
												  _personSkillProvider);
			Assert.IsNotNull(_target);

			ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult(_inPeriod);
			Assert.AreEqual(0.0 / _inPeriod.ElapsedTime().TotalMinutes,
							outDic[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
								s => s.Key.StartDateTime == _inPeriod.StartDateTime).Value.Payload.CalculatedResource,
							0.001);
		}

		[Test]
		public void VerifySchedulingPeriodDoNotIntersectSkillStaffPeriod()
		{
			DateTime skillDayDate = new DateTime(2009, 1, 2, 10, 00, 00, DateTimeKind.Utc);
			_skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills, skillDayDate);
			_target = new SchedulingResultService(_skillStaffPeriods,
				_personAssignmentListContainer.AllSkills,
				_resources,
				_personSkillProvider);

			ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult(_inPeriod);
			Assert.AreEqual(outDic, _skillStaffPeriods);
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
