using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class PersonAccountViewModelMappingTest
	{
		private IPerson _person;
		private Absence _absVacation;
		private Absence _absIllness;
		private TimeZoneInfo _cccTimeZone;

		private AccountDay _accountDay;
		private AccountTime _accountTime;
		private PersonAccountViewModelMapper target;

		[SetUp]
		public void Setup()
		{
			var timeZone = MockRepository.GenerateMock<IUserTimeZone>();

			target = new PersonAccountViewModelMapper(timeZone);

			_cccTimeZone = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"); // +8:00
			timeZone.Stub(x => x.TimeZone()).Return(_cccTimeZone);

			_person = PersonFactory.CreatePerson();

			var startDate = new DateOnly(2014, 01, 01);
			var periodLength = startDate.Date.AddYears(1) - startDate.Date;
			var accrued = TimeSpan.FromDays(12);
			var used = TimeSpan.FromDays(3);
			_absVacation = new Absence { Description = new Description("Vacation") };
			_accountDay = AbsenceAccountFactory.CreateAbsenceAccountDays(_person, _absVacation, startDate, periodLength,
			                                                             TimeSpan.Zero,
			                                                             TimeSpan.Zero, accrued, TimeSpan.Zero, used);

			accrued = TimeSpan.FromHours(96);
			used = TimeSpan.FromMinutes(15 * 60 + 45); // 15:45
			_absIllness = new Absence { Description = new Description("Illness") };
			_accountTime = AbsenceAccountFactory.CreateAbsenceAccountHours(_person, _absIllness, startDate, periodLength,
			                                                               TimeSpan.Zero,
			                                                               TimeSpan.Zero, accrued, TimeSpan.Zero, used);
		}

		[Test]
		public void ShouldMapAllValues()
		{
			var startDate = new DateTime(2014, 01, 01).AddHours(8);
			var endDate = startDate.AddYears(1).AddDays(-1);

			var vmDay = target.Map(_accountDay);
			vmDay.AbsenceName.Should().Be("Vacation");
			vmDay.TrackerType.Should().Be("Days");
			vmDay.PeriodStart.Should().Be(startDate);
			vmDay.PeriodEnd.Should().Be(endDate);
			vmDay.Used.Should().Be("3");
			vmDay.Remaining.Should().Be("9");

			var vmHour = target.Map(_accountTime);
			vmHour.AbsenceName.Should().Be("Illness");
			vmHour.TrackerType.Should().Be("Hours");
			vmHour.PeriodStart.Should().Be(startDate);
			vmHour.PeriodEnd.Should().Be(endDate);
			vmHour.Used.Should().Be("15:45");
			vmHour.Remaining.Should().Be("80:15");
		}
	}
}