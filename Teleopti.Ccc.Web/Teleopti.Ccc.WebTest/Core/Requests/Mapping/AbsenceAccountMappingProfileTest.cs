using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

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

		[SetUp]
		public void Setup()
		{
			var timeZone = MockRepository.GenerateMock<IUserTimeZone>();

			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new PersonAccountViewModelMappingProfile(() => timeZone)));

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
			used = TimeSpan.FromHours(15);
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

			var result1 = Mapper.Map<IAccount, AbsenceAccountViewModel>(_accountDay);
			result1.AbsenceName.Should().Be("Vacation");
			result1.PeriodStartUtc.Should().Be(startDate);
			result1.PeriodEndUtc.Should().Be(endDate);
			result1.Used.Should().Be(TimeSpan.FromDays(3));
			result1.Remaining.Should().Be(TimeSpan.FromDays(9));


			var result2 = Mapper.Map<IAccount, AbsenceAccountViewModel>(_accountTime);
			result2.AbsenceName.Should().Be("Illness");
			result2.PeriodStartUtc.Should().Be(startDate);
			result2.PeriodEndUtc.Should().Be(endDate);
			result2.Used.Should().Be(TimeSpan.FromHours(15));
			result2.Remaining.Should().Be(TimeSpan.FromHours(81));
		}
	}
}