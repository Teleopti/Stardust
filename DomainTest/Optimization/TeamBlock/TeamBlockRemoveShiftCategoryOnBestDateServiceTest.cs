using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockRemoveShiftCategoryOnBestDateServiceTest
	{
		private TeamBlockRemoveShiftCategoryOnBestDateService _target;
		private MockRepository _mock;
		private IShiftCategory _shiftCategory;
		private ISchedulingOptions _schedulingOptions;
		private IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculator;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDayService _scheduleDayService;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDay _scheduleDay;
		private IPersonAssignment _personAssignment;
		private DateOnly _dateOnly;
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private IPersonSkill _personSkill;
		private ISkill _skill;
		private IList<ISkill> _skills;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_shiftCategory = new ShiftCategory("shiftCategory");
			_shiftCategory.SetId(Guid.NewGuid());
			_schedulingOptions = new SchedulingOptions();
			_scheduleMatrixValueCalculator = _mock.StrictMock<IScheduleMatrixValueCalculatorPro>();
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayService = _mock.StrictMock<IScheduleDayService>();
			_dateOnly = new DateOnly(2015, 1, 1);
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_personAssignment = _mock.StrictMock<IPersonAssignment>();
			_person = _mock.StrictMock<IPerson>();
			_personPeriod = _mock.StrictMock<IPersonPeriod>();
			_personSkill = _mock.StrictMock<IPersonSkill>();
			_skill = _mock.StrictMock<ISkill>();
			_skills = new List<ISkill>{_skill};
			_target = new TeamBlockRemoveShiftCategoryOnBestDateService(_scheduleDayService);	
		}

		[Test]
		public void ShouldExecute()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleMatrixPro.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro}));
				Expect.Call(_scheduleDayPro.Day).Return(_dateOnly).Repeat.AtLeastOnce();

				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory);

				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill> {_personSkill});
				Expect.Call(_personSkill.Skill).Return(_skill);
				Expect.Call(_scheduleMatrixValueCalculator.DayValueForSkills(_dateOnly, _skills)).Return(10d);

				Expect.Call(_scheduleDayService.DeleteMainShift(new List<IScheduleDay> { _scheduleDay }, _schedulingOptions));
			}

			using (_mock.Playback())
			{
				var result = _target.Execute(_shiftCategory, _schedulingOptions, _scheduleMatrixValueCalculator, _scheduleMatrixPro, _dateOnlyPeriod);
				Assert.AreEqual(_scheduleDayPro, result);
			}
		}

		[Test]
		public void ShouldFindShiftCategoryOnScheduleDayPro()
		{
			using (_mock.Record())
			{
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_scheduleDay.PersonAssignment()).Return(_personAssignment);
				Expect.Call(_personAssignment.ShiftCategory).Return(_shiftCategory);
			}

			using (_mock.Playback())
			{
				var result = _target.IsThisDayCorrectShiftCategory(_scheduleDayPro, _shiftCategory);
				Assert.IsTrue(result);
			}	
		}
	}
}
