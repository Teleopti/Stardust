using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class RemoveShiftCategoryOnBestDateServiceTest
	{
		private IRemoveShiftCategoryOnBestDateService _interface;
		private RemoveShiftCategoryOnBestDateService _target;
		private IScheduleMatrixPro _scheduleMatrix;
		private IPerson _person;
		private DateOnlyPeriod _period;
		private MockRepository _mocks;
		private IScheduleMatrixValueCalculatorPro _scheduleMatrixValueCalculatorPro;
		private IShiftCategory _shiftCategory;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDay _schedulePart;
		private IScheduleDayService _scheduleDayService;
		private SchedulingOptions _schedulingOptions;

		[SetUp]
		public void Setup()
		{   
			_mocks = new MockRepository();
			_scheduleMatrixValueCalculatorPro = _mocks.StrictMock<IScheduleMatrixValueCalculatorPro>();
			_period = new DateOnlyPeriod(2010, 1, 1, 2010, 1, 3);
			_person = PersonFactory.CreatePerson("Test");
			_scheduleMatrix = ScheduleMatrixProFactory.Create(_period, _person);
			_shiftCategory = ShiftCategoryFactory.CreateShiftCategory("xx");
			_scheduleDayService = _mocks.StrictMock<IScheduleDayService>();
			_interface = new RemoveShiftCategoryOnBestDateService(_scheduleMatrix, _scheduleMatrixValueCalculatorPro, _scheduleDayService);
			_target = new RemoveShiftCategoryOnBestDateService(_scheduleMatrix, _scheduleMatrixValueCalculatorPro, _scheduleDayService);
			
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_schedulePart = _mocks.StrictMock<IScheduleDay>();
			_schedulingOptions = new SchedulingOptions();
		}

		[Test]
		public void VerifyConstructor()
		{
			Assert.IsNotNull(_interface);
			Assert.IsNotNull(_target);
		}

		[Test]
		public void VerifyExecute()
		{
			ISkill skill = SkillFactory.CreateSkill("skill");
			DateOnly firstDate = new DateOnly(2010, 1, 1);
			_person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(firstDate, skill));
			IScheduleMatrixPro scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_interface = new RemoveShiftCategoryOnBestDateService(scheduleMatrix, _scheduleMatrixValueCalculatorPro, _scheduleDayService);
			var unlockedDays = new [] { _scheduleDayPro};

			IScheduleDayPro result;

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrix.UnlockedDays).Return(unlockedDays);
				matchingShiftCategoryMock();
				Expect.Call(scheduleMatrix.Person).Return(_person).Repeat.Any();
				Expect.Call(_scheduleDayPro.Day).Return(firstDate).Repeat.Any();
				Expect.Call(_scheduleDayService.DeleteMainShift(new List<IScheduleDay> { _schedulePart }, _schedulingOptions)).Return(new List<IScheduleDay> { _schedulePart }).Repeat.Once();
				Expect.Call(_scheduleMatrixValueCalculatorPro.DayValueForSkills(firstDate, new List<ISkill> {skill}))
					.Repeat.Once()
					.Return(2);
				Expect.Call(scheduleMatrix.EffectivePeriodDays).Return(unlockedDays).Repeat.Once();
			}

			using (_mocks.Playback())
			{
				result = _interface.ExecuteOne(_shiftCategory, _schedulingOptions);
			}

			Assert.AreSame(_scheduleDayPro, result);
		}


		[Test]
		public void VerifyExecuteWhenNotFindDayToRemove()
		{
		   ISkill skill = SkillFactory.CreateSkill("skill");
			DateOnly firstDate = new DateOnly(2010, 1, 1);
			_person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(firstDate, skill));
			IScheduleMatrixPro scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_interface = new RemoveShiftCategoryOnBestDateService(scheduleMatrix, _scheduleMatrixValueCalculatorPro, _scheduleDayService);
			var unlockedDays = new [] { _scheduleDayPro};

			IScheduleDayPro result;

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrix.UnlockedDays).Return(unlockedDays);
				matchingShiftCategoryMock();
				Expect.Call(scheduleMatrix.Person).Return(_person).Repeat.Any();
				Expect.Call(_scheduleDayPro.Day).Return(firstDate).Repeat.Any();
				Expect.Call(_scheduleMatrixValueCalculatorPro.DayValueForSkills(firstDate, new List<ISkill> {skill}))
					.Return(double.MaxValue)
					.Repeat.Once();
				Expect.Call(scheduleMatrix.EffectivePeriodDays)
					.Return(unlockedDays)
					.Repeat.Once();
			}

			using (_mocks.Playback())
			{
				result = _interface.ExecuteOne(_shiftCategory, _schedulingOptions);
			}

			Assert.IsNull(result);
		}


		[Test]
		public void VerifyIsDayToBeRemoved()
		{
			bool result;

			using(_mocks.Record())
			{
				matchingShiftCategoryMock();
			}

			using(_mocks.Playback())
			{
				result = _interface.IsThisDayCorrectShiftCategory(_scheduleDayPro, _shiftCategory);
			}
			
			Assert.IsTrue(result);
		}

		[Test]
		public void VerifyIsDayToBeRemovedWhenWrongShiftCategory()
		{
			IPersonAssignment assignment = _mocks.StrictMock<IPersonAssignment>();
			bool result;

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.Once();
				Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Once();
				Expect.Call(_schedulePart.PersonAssignment()).Return(assignment).Repeat.Once();
				Expect.Call(assignment.ShiftCategory).Return(ShiftCategoryFactory.CreateShiftCategory("yy")).Repeat.Once();
			}

			using (_mocks.Playback())
			{
				result = _interface.IsThisDayCorrectShiftCategory(_scheduleDayPro, _shiftCategory);
			}

			Assert.IsFalse(result);
		}

		[Test]
		public void VerifyIsDayToBeRemovedWhenNoMainShift()
		{

			bool result;

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.Once();
				Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Once();
			}

			using (_mocks.Playback())
			{
				result = _interface.IsThisDayCorrectShiftCategory(_scheduleDayPro, _shiftCategory);
			}

			Assert.IsFalse(result);
		}

		[Test]
		public void VerifyDaysToWorkWithUsesUnlockedDays()
		{
			var scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new RemoveShiftCategoryOnBestDateService(scheduleMatrix, _scheduleMatrixValueCalculatorPro, _scheduleDayService);
			var unlockedDays = new [] {_scheduleDayPro, _scheduleDayPro};

			IList<IScheduleDayPro> daysToWorkWith;

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrix.UnlockedDays).Return(unlockedDays);
				matchingShiftCategoryMock();
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 1)).Repeat.Once();
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 2)).Repeat.Once();
			}

			using (_mocks.Playback())
			{
				daysToWorkWith = _target.DaysToWorkWith(_shiftCategory, _period);
			}
			
			Assert.AreEqual(2, daysToWorkWith.Count);
		}

		[Test]
		public void VerifyDaysToWorkWithUsesPeriod()
		{
			IScheduleMatrixPro scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_target = new RemoveShiftCategoryOnBestDateService(scheduleMatrix, _scheduleMatrixValueCalculatorPro, _scheduleDayService);
			var unlockedDays = new [] { _scheduleDayPro, _scheduleDayPro };

			IList<IScheduleDayPro> daysToWorkWith;

			using (_mocks.Record())
			{
				Expect.Call(scheduleMatrix.UnlockedDays).Return(unlockedDays);
				matchingShiftCategoryMock();
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 1)).Repeat.Once();
				Expect.Call(_scheduleDayPro.Day).Return(new DateOnly(2010, 1, 2)).Repeat.Once();
			}

			using (_mocks.Playback())
			{
				daysToWorkWith = _target.DaysToWorkWith(_shiftCategory, new DateOnlyPeriod(2010, 1, 1, 2010, 1, 1));
			}

			Assert.AreEqual(1, daysToWorkWith.Count);
		}

		[Test]
		public void VerifyFindDayToRemove()
		{
			ISkill skill = SkillFactory.CreateSkill("skill");
			DateOnly firstDate = new DateOnly(2010, 1, 1);
			_person.AddPersonPeriod(PersonPeriodFactory.CreatePersonPeriodWithSkills(firstDate, skill));
			IList<IScheduleDayPro> list = new List<IScheduleDayPro>();
			list.Add(new ScheduleDayPro(firstDate, _scheduleMatrix));
			list.Add(new ScheduleDayPro(new DateOnly(2010, 1, 2), _scheduleMatrix));
			list.Add(new ScheduleDayPro(new DateOnly(2010, 1, 3), _scheduleMatrix));

			IScheduleDayPro dayToRemove;

			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixValueCalculatorPro.DayValueForSkills(firstDate, new List<ISkill> {skill}))
					.Repeat.Once()
					.Return(2);
				Expect.Call(_scheduleMatrixValueCalculatorPro.DayValueForSkills(firstDate.AddDays(1), new List<ISkill> { skill }))
					.Repeat.Once().
					Return(1);
				Expect.Call(_scheduleMatrixValueCalculatorPro.DayValueForSkills(firstDate.AddDays(2), new List<ISkill> { skill }))
					.Repeat.Once().
					Return(3);
			}

			using (_mocks.Playback())
			{
				dayToRemove = _target.FindDayToRemove(list);
			}
			
			Assert.AreEqual(new DateOnly(2010, 1, 2), dayToRemove.Day);

		}
	
		private void matchingShiftCategoryMock()
		{
			IPersonAssignment assignment = _mocks.StrictMock<IPersonAssignment>();

			Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_schedulePart).Repeat.AtLeastOnce();
			Expect.Call(_schedulePart.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
			Expect.Call(_schedulePart.PersonAssignment()).Return(assignment).Repeat.AtLeastOnce();
			Expect.Call(assignment.ShiftCategory).Return(_shiftCategory).Repeat.AtLeastOnce();
		}
	}
}