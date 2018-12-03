using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[TestFixture]
	public class WeeksFromScheduleDaysExtractorTest
	{
		private MockRepository _mocks;
		private WeeksFromScheduleDaysExtractor _target;
		private IPerson _person1;
		private IPerson _person2;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new WeeksFromScheduleDaysExtractor();
		}

		[Test]
		public void VerifyCreate()
		{
			Assert.IsNotNull(_target);
		}

		[Test]
		public void VerifyCanCreateCorrectListOfWeeks()
		{
			var dateOnly1 = new DateOnly(2010, 8, 5);
			var dateOnly2 = new DateOnly(2010, 8, 18);
			_person1 = PersonFactory.CreatePerson("", "");
			_person2 = PersonFactory.CreatePerson("", "");
			_person1.FirstDayOfWeek = DayOfWeek.Sunday;
			_person2.FirstDayOfWeek = DayOfWeek.Monday;

			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod1 = new DateOnlyAsDateTimePeriod(dateOnly1,
				TimeZoneInfo.Utc);
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod2 = new DateOnlyAsDateTimePeriod(dateOnly2,
				TimeZoneInfo.Utc);

			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			IList<IScheduleDay> days = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.Person).Return(_person2).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod2);
			}

			using (_mocks.Playback())
			{
				var ret = _target.CreateWeeksFromScheduleDaysExtractor(days).ToArray();
				Assert.AreEqual(2, ret.Length);
				Assert.AreSame(_person1, ret.First().Person);
				Assert.AreSame(_person2, ret.ElementAt(1).Person);
				Assert.AreEqual(new DateOnly(2010, 8, 1), ret.First().Week.StartDate);
				Assert.AreEqual(new DateOnly(2010, 8, 7), ret.First().Week.EndDate);
				Assert.AreEqual(new DateOnly(2010, 8, 16), ret.ElementAt(1).Week.StartDate);
				Assert.AreEqual(new DateOnly(2010, 8, 22), ret.ElementAt(1).Week.EndDate);
				Assert.AreEqual(7, ret.First().Week.DayCount());
			}
		}

		[Test]
		public void ShouldNotReturnWeekDuplicates()
		{
			var dateOnly1 = new DateOnly(2010, 8, 5);
			var dateOnly2 = new DateOnly(2010, 8, 7);
			_person1 = PersonFactory.CreatePerson("", "");
			_person1.FirstDayOfWeek = DayOfWeek.Monday;

			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod1 = new DateOnlyAsDateTimePeriod(dateOnly1,
				TimeZoneInfo.Utc);
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod2 = new DateOnlyAsDateTimePeriod(dateOnly2,
				TimeZoneInfo.Utc);

			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			IList<IScheduleDay> days = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay2.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod1);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod2);
			}

			using (_mocks.Playback())
			{
				var ret = _target.CreateWeeksFromScheduleDaysExtractor(days);
				Assert.AreEqual(1, ret.Count());
			}
		}

		[Test]
		public void VerifyCanCreateCorrectListOfWeeksWithAddWeeksBefore()
		{
			var dateOnly1 = new DateOnly(2010, 8, 23);
			_person1 = PersonFactory.CreatePerson("", "");
			_person1.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
			var personContract = _mocks.DynamicMock<IPersonContract>();
			_person1.AddPersonPeriod(new PersonPeriod(dateOnly1.AddDays(-100), personContract, new Team()));
			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod1 = new DateOnlyAsDateTimePeriod(dateOnly1,
				TimeZoneInfo.Utc);
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			IList<IScheduleDay> days = new List<IScheduleDay> {_scheduleDay1};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod1).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var ret = _target.CreateWeeksFromScheduleDaysExtractor(days, true).ToArray();
				Assert.AreEqual(2, ret.Length);
				Assert.AreSame(_person1, ret.First().Person);
				Assert.AreEqual(new DateOnly(2010, 8, 23), ret.First().Week.StartDate);
				Assert.AreEqual(new DateOnly(2010, 8, 16), ret.ElementAt(1).Week.StartDate);
				Assert.AreEqual(7, ret.First().Week.DayCount());
			}
		}

		[Test]
		public void VerifyCanCreateCorrectListOfWeeksWithAddWeeksAfter()
		{
			var dateOnly1 = new DateOnly(2010, 8, 29);
			_person1 = PersonFactory.CreatePerson("", "");
			_person1.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));
			var personContract = _mocks.DynamicMock<IPersonContract>();
			_person1.AddPersonPeriod(new PersonPeriod(dateOnly1.AddDays(-100), personContract, new Team()));

			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod1 = new DateOnlyAsDateTimePeriod(dateOnly1,
				TimeZoneInfo.Utc);
			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			IList<IScheduleDay> days = new List<IScheduleDay> {_scheduleDay1};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod1).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				var ret = _target.CreateWeeksFromScheduleDaysExtractor(days, true).ToArray();
				Assert.AreEqual(2, ret.Length);
				Assert.AreSame(_person1, ret.First().Person);
				Assert.AreEqual(new DateOnly(2010, 8, 23), ret.First().Week.StartDate);
				Assert.AreEqual(new DateOnly(2010, 8, 30), ret.ElementAt(1).Week.StartDate);
				Assert.AreEqual(new DateOnly(2010, 9, 5), ret.ElementAt(1).Week.EndDate);
				Assert.AreEqual(7, ret.First().Week.DayCount());
				Assert.AreEqual(7, ret.ElementAt(1).Week.DayCount());
			}
		}

		[Test]
		public void ShouldNotReturnWeekBeforeIfNoPersonPeriod()
		{
			var dateOnly1 = new DateOnly(2010, 12, 6);
			_person1 = PersonFactory.CreatePerson("", "");
			var personContract = _mocks.DynamicMock<IPersonContract>();
			_person1.AddPersonPeriod(new PersonPeriod(dateOnly1.AddDays(-3), personContract, new Team()));
			_person1.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));

			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod1 = new DateOnlyAsDateTimePeriod(dateOnly1, 
				TimeZoneInfo.Utc);

			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();

			IList<IScheduleDay> days = new List<IScheduleDay> {_scheduleDay1};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod1);
			}

			using (_mocks.Playback())
			{
				var ret = _target.CreateWeeksFromScheduleDaysExtractor(days, true).ToArray();
				Assert.AreEqual(1, ret.Length);
				Assert.AreSame(_person1, ret.First().Person);
				Assert.AreEqual(new DateOnly(2010, 12, 6), ret.First().Week.StartDate);
			}
		}

		[Test]
		public void ShouldNotReturnWeekAfterIfNoPersonPeriod()
		{
			var dateOnly1 = new DateOnly(2010, 12, 12);
			_person1 = PersonFactory.CreatePerson("", "");
			var personContract = _mocks.DynamicMock<IPersonContract>();
			_person1.AddPersonPeriod(new PersonPeriod(dateOnly1.AddDays(-100), personContract, new Team()));
			_person1.TerminatePerson(dateOnly1.AddDays(2), new PersonAccountUpdaterDummy());
			_person1.PermissionInformation.SetCulture(new CultureInfo("sv-SE"));

			IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod1 = new DateOnlyAsDateTimePeriod(dateOnly1, (
				TimeZoneInfo.Utc));

			_scheduleDay1 = _mocks.StrictMock<IScheduleDay>();

			IList<IScheduleDay> days = new List<IScheduleDay> {_scheduleDay1};

			using (_mocks.Record())
			{
				Expect.Call(_scheduleDay1.Person).Return(_person1).Repeat.AtLeastOnce();
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod1);
			}

			using (_mocks.Playback())
			{
				var ret = _target.CreateWeeksFromScheduleDaysExtractor(days, true).ToArray();
				Assert.AreEqual(1, ret.Length);
				Assert.AreSame(_person1, ret.First().Person);
				Assert.AreEqual(new DateOnly(2010, 12, 6), ret.First().Week.StartDate);
			}
		}
	}
}
