using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	public class ScheduleDayIntraIntervalIssueExtractorTest
	{
		private ScheduleDayIntraIntervalIssueExtractor _target;
		private IScheduleDictionary _scheduleDictionary;
		private MockRepository _mock;
		private ISkillStaffPeriod _skillStaffPeriod;
		private IList<ISkillStaffPeriod> _issues;
		private DateOnly _dateOnly;
		private IScenario _scenario;
		private IPersonAssignment _personAssignment;
		private IActivity _mainActivity;
		private IPerson _person;
		private IShiftCategory _shiftCategory;
		private DateTimePeriod _intervalPeriod;
		private DateTimePeriod _mainShiftPeriod;

		private void setup()
		{
			_mock = new MockRepository();
			_scenario = new Scenario("scenario");
			_dateOnly = new DateOnly(2014, 1, 1);
			_target = new ScheduleDayIntraIntervalIssueExtractor();
			_skillStaffPeriod = _mock.StrictMock<ISkillStaffPeriod>();
			_issues = new List<ISkillStaffPeriod>{_skillStaffPeriod};
			_mainActivity = ActivityFactory.CreateActivity("mainActivity");
			_person = PersonFactory.CreatePerson("person");
			var start = DateTime.SpecifyKind(_dateOnly.Date.AddHours(10), DateTimeKind.Utc);
			var end = start.AddMinutes(5);
			_mainShiftPeriod = new DateTimePeriod(start, end);
			_intervalPeriod = new DateTimePeriod(start, end.AddMinutes(25));
			_shiftCategory = ShiftCategoryFactory.CreateShiftCategory("shiftCategory");
			_personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person, _scenario, _mainActivity, _mainShiftPeriod, _shiftCategory);
			_scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(_scenario, _dateOnly.Date, _personAssignment);
		}

		[Test]
		public void ShouldExtract()
		{
			setup();
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod.Period).Return(_intervalPeriod).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_scheduleDictionary, _dateOnly, _issues);	
				Assert.AreEqual(1, result.Count);
			}
		}

		[Test]
		public void ShouldNotExtractWhenNoMainShift()
		{
			setup();
			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(_person, _scenario, _mainShiftPeriod);
			_scheduleDictionary = ScheduleDictionaryForTest.WithPersonAbsence(_scenario, _mainShiftPeriod, personAbsence);
			var result = _target.Extract(_scheduleDictionary, _dateOnly, _issues);
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		public void ShouldNotExtractWhenPersonAssingmentDontIntersectIntervalPeriod()
		{
			setup();
			using (_mock.Record())
			{
				Expect.Call(_skillStaffPeriod.Period).Return(_intervalPeriod.MovePeriod(TimeSpan.FromHours(4))).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.Extract(_scheduleDictionary, _dateOnly, _issues);
				Assert.AreEqual(0, result.Count);
			}	
		}
	}
}
