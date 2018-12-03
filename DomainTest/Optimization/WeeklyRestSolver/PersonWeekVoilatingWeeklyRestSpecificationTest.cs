using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
	public class PersonWeekVoilatingWeeklyRestSpecificationTest
	{
		private IPersonWeekViolatingWeeklyRestSpecification _target;
		private IScheduleRange _currentSchedules;
		private PersonWeek _personWeek;
		private MockRepository _mock;
		private IExtractDayOffFromGivenWeek _extractDayOffFromGivenWeek;
		private IVerifyWeeklyRestAroundDayOffSpecification _verifyWeeklyRestAroundDayOffSpecification;
		private IEnsureWeeklyRestRule _ensureWeeklyRestRule;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private TimeSpan _weeklyRest;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_extractDayOffFromGivenWeek = _mock.StrictMock<IExtractDayOffFromGivenWeek>();
			_ensureWeeklyRestRule = _mock.StrictMock<IEnsureWeeklyRestRule>();
			_verifyWeeklyRestAroundDayOffSpecification = _mock.StrictMock<IVerifyWeeklyRestAroundDayOffSpecification>();
			_target = new PersonWeekViolatingWeeklyRestSpecification(_extractDayOffFromGivenWeek ,_verifyWeeklyRestAroundDayOffSpecification,_ensureWeeklyRestRule  );
			_currentSchedules = _mock.StrictMock<IScheduleRange>();
			
			_personWeek = new PersonWeek(PersonFactory.CreatePerson( "test1"),new DateOnlyPeriod(2014,06,15,2014,06,18));
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_weeklyRest = TimeSpan.FromHours(40);
		}

		[Test]
		public void ReturnTrueIfNoDayOffFoundAndWeeklyRestAchieved()
		{
			IEnumerable<IScheduleDay> listOfScheduleDays = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};
			using (_mock.Record())
			{
				Expect.Call(_currentSchedules.Person).Return(_personWeek.Person);
				Expect.Call(_currentSchedules.ScheduledDayCollection(_personWeek.Week)).Return(listOfScheduleDays);
				Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(listOfScheduleDays)).Return(new List<DateOnly>());
				Expect.Call(_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(new List<DateOnly>(), _currentSchedules))
					.Return(true);
				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _currentSchedules, _weeklyRest)).Return(true);
			}
			using (_mock.Playback())
			{
				Assert.IsTrue(_target.IsSatisfyBy(_currentSchedules, _personWeek.Week, _weeklyRest));
			}
			
		}

		[Test]
		public void ReturnFalseIfNoDayOffFoundAndWeeklyRestNotAchieved()
		{
			IEnumerable<IScheduleDay> listOfScheduleDays = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };
			using (_mock.Record())
			{
				Expect.Call(_currentSchedules.Person).Return(_personWeek.Person);
				Expect.Call(_currentSchedules.ScheduledDayCollection(_personWeek.Week)).Return(listOfScheduleDays);
				Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(listOfScheduleDays)).Return(new List<DateOnly>());
				Expect.Call(_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(new List<DateOnly>(), _currentSchedules))
					.Return(true);
				Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(_personWeek, _currentSchedules, _weeklyRest)).Return(false);
			}
			using (_mock.Playback())
			{
				Assert.IsFalse(_target.IsSatisfyBy(_currentSchedules, _personWeek.Week, _weeklyRest));
			}

		}

		[Test]
		public void ReturnTrueIfDayOffFoundDayOffSpecIsFalse()
		{
			IEnumerable<IScheduleDay> listOfScheduleDays = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };
			using (_mock.Record())
			{
				Expect.Call(_currentSchedules.Person).Return(_personWeek.Person);
				Expect.Call(_currentSchedules.ScheduledDayCollection(_personWeek.Week)).Return(listOfScheduleDays);
				Expect.Call(_extractDayOffFromGivenWeek.GetDaysOff(listOfScheduleDays)).Return(new List<DateOnly>());
				Expect.Call(_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(new List<DateOnly>(), _currentSchedules))
					.Return(false);
			}
			using (_mock.Playback())
			{
				Assert.IsTrue( _target.IsSatisfyBy(_currentSchedules, _personWeek.Week, _weeklyRest));
			}

		}
	}

	
}
