using System;
using System.Collections.Generic;
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

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_inPeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 15, 00, DateTimeKind.Utc));
			_personAssignmentListContainer = PersonAssignmentFactory.CreatePersonAssignmentListForActivityDividerTest();
			_skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills);
			_target = new SchedulingResultService(_skillStaffPeriods,
				_personAssignmentListContainer.AllSkills,
				_personAssignmentListContainer.TestVisualLayerCollection(),
				new SingleSkillCalculator(),
				false,
				new SingleSkillDictionary());
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

			ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult();
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
												  new List<IVisualLayerCollection>(),
												  new SingleSkillCalculator(),
												  false,
												  new SingleSkillDictionary());
			Assert.IsNotNull(_target);

			ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult();
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
				_personAssignmentListContainer.TestVisualLayerCollection(),
				new SingleSkillCalculator(),
				false,
				new SingleSkillDictionary());

			ISkillSkillStaffPeriodExtendedDictionary outDic = _target.SchedulingResult();
			Assert.AreEqual(outDic, _skillStaffPeriods);
		}

		[Test]
		public void ShouldNotUseSingleSkillCalculationOnNoInputDays()
		{
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			var toRemove = new List<IVisualLayerCollection>();
			var toAdd = new List<IVisualLayerCollection>();
			_target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), new SingleSkillCalculator(), false, singleSkillDictionary);

			var res = _target.UseSingleSkillCalculations(toRemove, toAdd);
			Assert.IsFalse(res);
		}

		[Test]
		public void ShouldNotUseSingleSkillCalculationsWhenToRemoveIsNotSingleSkilled()
		{
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			_target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), new SingleSkillCalculator(), false, singleSkillDictionary);
			var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
			var toRemove = new List<IVisualLayerCollection> { visualLayerCollection };
			var toAdd = new List<IVisualLayerCollection>();
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("logon", "password");
			var dateTimePeriod = new DateTimePeriod(2011, 1, 1, 2011, 1, 1);

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Person).Return(person);
				Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod);
				Expect.Call(singleSkillDictionary.IsSingleSkill(person, new DateOnly(2011, 1, 1))).Return(false);
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
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			_target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), new SingleSkillCalculator(), false, singleSkillDictionary);
			var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
			var toAdd = new List<IVisualLayerCollection> { visualLayerCollection };
			var toRemove = new List<IVisualLayerCollection>();
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("logon", "password");
			var dateTimePeriod = new DateTimePeriod(2011, 1, 1, 2011, 1, 1);

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Person).Return(person);
				Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod);
				Expect.Call(singleSkillDictionary.IsSingleSkill(person, new DateOnly(2011, 1, 1))).Return(false);
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
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			var visualLayerCollection1 = _mocks.StrictMock<IVisualLayerCollection>();
			var visualLayerCollection2 = _mocks.StrictMock<IVisualLayerCollection>();
			_target = new SchedulingResultService(_skillStaffPeriods, _personAssignmentListContainer.AllSkills, _personAssignmentListContainer.TestVisualLayerCollection(), new SingleSkillCalculator(), false, singleSkillDictionary);
			var toAdd = new List<IVisualLayerCollection> { visualLayerCollection1 };
			var toRemove = new List<IVisualLayerCollection> { visualLayerCollection2 };
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("logon", "password");
			var dateTimePeriod = new DateTimePeriod(2011, 1, 1, 2011, 1, 1);

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection1.Person).Return(person);
				Expect.Call(visualLayerCollection1.Period()).Return(dateTimePeriod);
				Expect.Call(visualLayerCollection2.Person).Return(person);
				Expect.Call(visualLayerCollection2.Period()).Return(dateTimePeriod);

				Expect.Call(singleSkillDictionary.IsSingleSkill(person, new DateOnly(2011, 1, 1))).Return(true).Repeat.Twice();
			}

			using (_mocks.Playback())
			{
				var res = _target.UseSingleSkillCalculations(toRemove, toAdd);
				Assert.IsTrue(res);
			}
		}
	}
}
