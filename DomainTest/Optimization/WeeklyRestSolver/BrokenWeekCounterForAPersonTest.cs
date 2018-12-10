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
	public class BrokenWeekCounterForAPersonTest
	{
		private BrokenWeekCounterForAPerson _target;
		private IWeeksFromScheduleDaysExtractor _weeksFromScheduleDaysExtractor;
		private IEnsureWeeklyRestRule _ensureWeeklyRestRule;
		private IContractWeeklyRestForPersonWeek _contractWeeklyRestForPersonWeek;
		private MockRepository _mock;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private PersonWeek _personWeek1;
		private PersonWeek _personWeek2;
		private PersonWeek _personWeek3;
		private TimeSpan _weeklyRest;
		private IScheduleRange _currentSchedules;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_weeksFromScheduleDaysExtractor = _mock.StrictMock<IWeeksFromScheduleDaysExtractor>();
			_ensureWeeklyRestRule = _mock.StrictMock<IEnsureWeeklyRestRule>();
			_contractWeeklyRestForPersonWeek = _mock.StrictMock<IContractWeeklyRestForPersonWeek>();
			_target = new BrokenWeekCounterForAPerson(_weeksFromScheduleDaysExtractor, _ensureWeeklyRestRule , _contractWeeklyRestForPersonWeek );
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_personWeek1 = new PersonWeek(PersonFactory.CreatePerson( "1"),new DateOnlyPeriod(2014,06,16,2014,06,22));
			_personWeek2 = new PersonWeek(PersonFactory.CreatePerson( "1"),new DateOnlyPeriod(2014,06,23,2014,06,29));
			_personWeek3 = new PersonWeek(PersonFactory.CreatePerson( "1"),new DateOnlyPeriod(2014,06,30,2014,07,6));
			_weeklyRest = TimeSpan.FromHours(36);
			_currentSchedules = _mock.StrictMock<IScheduleRange>();
		}

		[Test]
		public void ReturnZeroIfNoWeekIsBroken()
		{
			IEnumerable<IScheduleDay> scheduleDays = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};
			using (_mock.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays, true)).Return(new List< PersonWeek >{_personWeek1,_personWeek2,_personWeek3});
				expectCallInLoopOfEachWeek(_personWeek1, _weeklyRest, true);
				expectCallInLoopOfEachWeek(_personWeek2, _weeklyRest, true);
				expectCallInLoopOfEachWeek(_personWeek3, _weeklyRest, true);
			}
			using (_mock.Playback())
			{
				Assert.AreEqual(0, _target.CountBrokenWeek(scheduleDays,_currentSchedules ));
			}
		}

		[Test]
		public void ReturnOneWhenWeekIsBroken()
		{
			IEnumerable<IScheduleDay> scheduleDays = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };
			using (_mock.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays, true)).Return(new List<PersonWeek> { _personWeek1, _personWeek2, _personWeek3 });
				expectCallInLoopOfEachWeek(_personWeek1, _weeklyRest, true);
				expectCallInLoopOfEachWeek(_personWeek2, _weeklyRest, false);
				expectCallInLoopOfEachWeek(_personWeek3, _weeklyRest, true);
			}
			using (_mock.Playback())
			{
				Assert.AreEqual(1, _target.CountBrokenWeek(scheduleDays, _currentSchedules));
			}
		}

		[Test]
		public void ReturnThreeWhenAllWeeksBroken()
		{
			IEnumerable<IScheduleDay> scheduleDays = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2 };
			using (_mock.Record())
			{
				Expect.Call(_weeksFromScheduleDaysExtractor.CreateWeeksFromScheduleDaysExtractor(scheduleDays, true)).Return(new List<PersonWeek> { _personWeek1, _personWeek2, _personWeek3 });
				expectCallInLoopOfEachWeek(_personWeek1, _weeklyRest, false);
				expectCallInLoopOfEachWeek(_personWeek2, _weeklyRest, false);
				expectCallInLoopOfEachWeek(_personWeek3, _weeklyRest, false);
			}
			using (_mock.Playback())
			{
				Assert.AreEqual(3, _target.CountBrokenWeek(scheduleDays, _currentSchedules));
			}
		}

		private void expectCallInLoopOfEachWeek(PersonWeek personWeek, TimeSpan weeklyRest, bool result)
		{
			Expect.Call(_contractWeeklyRestForPersonWeek.GetWeeklyRestFromContract(personWeek)).Return(weeklyRest);
			Expect.Call(_ensureWeeklyRestRule.HasMinWeeklyRest(personWeek, _currentSchedules, weeklyRest)).Return(result);
		}

	}

	
}
