using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
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
		private MockRepository _mocks;
		private PersonSkillProvider _personSkillProvider;
		private IResourceCalculationDataContainer _resources;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_inPeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 15, 00, DateTimeKind.Utc));
			_personAssignmentListContainer = PersonAssignmentFactory.CreatePersonAssignmentListForActivityDividerTest();
			_skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills);
			_personSkillProvider = new PersonSkillProvider();

			_resources = new ResourceCalculationDataContainer(_personSkillProvider);
			var layers = _personAssignmentListContainer.TestVisualLayerCollection();
			foreach (var layer in layers)
			{
				foreach (var resourceLayer in layer.ToResourceLayers(15))
				{
					_resources.AddResources(resourceLayer.Period, resourceLayer.Activity, layer.Person, new DateOnly(2008,1,1), resourceLayer.Resource);
				}
			}

			_target = new SchedulingResultService(_skillStaffPeriods,
				_personAssignmentListContainer.AllSkills,
				_resources,
				new SingleSkillCalculator(),
				false,
				_personSkillProvider);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static IDictionary<ISkill, IList<ISkillDay>> CreateSkillDaysFromSkillStaffPeriodDictionary(MockRepository mocks, ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary)
		{
			IDictionary<ISkill, IList<ISkillDay>> skillDaysDic = new Dictionary<ISkill, IList<ISkillDay>>();
			foreach (var pair in skillStaffPeriodDictionary)
			{
				var skillDays = new List<ISkillDay>();
				foreach (var skillStaffPeriod in pair.Value)
				{
					ISkillDay skillDay = mocks.StrictMock<ISkillDay>();
					Expect.Call(skillDay.Skill).Return(pair.Key).Repeat.Any();
					Expect.Call(skillDay.CurrentDate).Return(new DateOnly(skillStaffPeriod.Key.StartDateTime.Date)).Repeat.Any();
					Expect.Call(skillDay.SkillStaffPeriodCollection).Return(
						new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod> { skillStaffPeriod.Value }))
						.Repeat.Any();
					skillDays.Add(skillDay);
				}

				skillDaysDic.Add(pair.Key, skillDays);
			}
			return skillDaysDic;
		}

		[Test]
		public void VerifyConstructorOverload2()
		{
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
												  new ResourceCalculationDataContainer(_personSkillProvider), 
												  new SingleSkillCalculator(),
												  false,
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
				new SingleSkillCalculator(),
				false,
				_personSkillProvider);

			ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult(_inPeriod);
			Assert.AreEqual(outDic, _skillStaffPeriods);
		}

		[Test]
		public void ShouldNotUseSingleSkillCalculationOnNoInputDays()
		{
			var toRemove = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			var toAdd = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			_target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _resources, new SingleSkillCalculator(), false, _personSkillProvider);

			var res = _target.UseSingleSkillCalculations(toRemove, toAdd);
			Assert.IsFalse(res);
		}

		[Test]
		public void ShouldNotUseSingleSkillCalculationsWhenToRemoveIsNotSingleSkilled()
		{
			_target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _resources, new SingleSkillCalculator(), false, _personSkillProvider);
			var toRemove = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			var toAdd = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			
			using (_mocks.Record())
			{
			}

			using (_mocks.Playback())
			{
				var res = _target.UseSingleSkillCalculations(toRemove, toAdd);
				Assert.IsFalse(res);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "AddIs"), Test]
		public void ShouldNotUseSingleSkillCalculationsWhenToAddIsNotSingleSkilled()
		{
			_target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _resources, new SingleSkillCalculator(), false, _personSkillProvider);
			var toAdd = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			var toRemove = _mocks.DynamicMock<IResourceCalculationDataContainer>();

			using (_mocks.Record())
			{
			}

			using (_mocks.Playback())
			{
				var res = _target.UseSingleSkillCalculations(toRemove, toAdd);
				Assert.IsFalse(res);
			}
		}

		[Test]
		public void ShouldUseSingleSkillCalculationsWhenToAddAndToRemoveIsSingleSkilled()
		{
			_target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _resources, new SingleSkillCalculator(), false, _personSkillProvider);
			var toAdd = _mocks.DynamicMock<IResourceCalculationDataContainer>();
			var toRemove = _mocks.DynamicMock<IResourceCalculationDataContainer>();

			using (_mocks.Record())
			{
				Expect.Call(toAdd.HasItems()).Return(true);
				Expect.Call(toAdd.AllIsSingleSkill()).Return(true);
				Expect.Call(toRemove.AllIsSingleSkill()).Return(true);
			}

			using (_mocks.Playback())
			{
				var res = _target.UseSingleSkillCalculations(toRemove, toAdd);
				Assert.IsTrue(res);
			}
		}
	}
}
