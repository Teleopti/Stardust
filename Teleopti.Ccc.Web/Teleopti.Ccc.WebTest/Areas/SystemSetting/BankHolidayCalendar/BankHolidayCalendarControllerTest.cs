﻿using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSettingWeb;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Controller;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;

namespace Teleopti.Ccc.WebTest.Areas.SystemSetting.BankHolidayCalendars
{
	[TestFixture]
	[DomainTest]
	[WebTest]
	public class BankHolidayCalendarControllerTest : IIsolateSystem
	{
		public BankHolidayCalendarController Target;
		public FakeBankHolidayCalendarRepository BankHolidayCalendarRepository;
		public FakeBankHolidayDateRepository BankHolidayDateRepository;

		private DateTime _nationalDay = new DateTime(2018, 10, 1);
		private DateTime _springFestival = new DateTime(2019, 2, 3);
		private DateTime _newYear = new DateTime(2019, 1, 1);

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeBankHolidayCalendarRepository>().For<IBankHolidayCalendarRepository>();
			isolate.UseTestDouble<FakeBankHolidayDateRepository>().For<IBankHolidayDateRepository>();
		}

		private BankHolidayCalendar PrepareData()
		{

			var Id = Guid.Parse("8cd8899a-4001-4b58-8f1d-dd39eba5fb94");

			var calendar = new BankHolidayCalendar("ChinaBankHoliday");
			calendar.SetId(Id);
			calendar.AddDate(new BankHolidayDate { Date = _nationalDay, Description = "National Day" });
			calendar.AddDate(new BankHolidayDate { Date = _springFestival, Description = "Spring Festival" });
			calendar.AddDate(new BankHolidayDate { Date = _newYear, Description = "New Year" });

			BankHolidayCalendarRepository.Add(calendar);
			calendar.Dates.ToList().ForEach(d => BankHolidayDateRepository.Add(d));

			return calendar;
		}

		[Test]
		public void ShouldCreateANewBankHolidayCalendar()
		{
			var input = new BankHolidayCalendarForm
			{
				Name = "ChinaBankHoliday",
				Years = new List<BankHolidayYearForm>{
					new BankHolidayYearForm {
						Dates = new List<BankHolidayDateForm>
						{
							new BankHolidayDateForm { Date=_nationalDay,Description="National Day" },
							new BankHolidayDateForm{ Date=_newYear,Description="New Year" }
						}
					}
				}
			};

			var result = Target.SaveBankHolidayCalendar(input);

			result.Name.Should().Be.EqualTo(input.Name);
			result.Years.Count().Should().Be.EqualTo(2);
			result.Years.First().Dates.First().Date.Should().Be.EqualTo(_nationalDay);
			result.Years.First().Dates.First().Description.Should().Be.EqualTo("National Day");
			result.Years.Last().Dates.First().Date.Should().Be.EqualTo(_newYear);
			result.Years.Last().Dates.First().Description.Should().Be.EqualTo("New Year");
		}

		[Test]
		public void ShouldSaveBankHolidayCalendarWithUpdatingCalendarName()
		{
			var calendar = PrepareData();

			var input = new BankHolidayCalendarForm
			{
				Id = calendar.Id.Value,
				Name = "New Calendar Name"
			};


			var result = Target.SaveBankHolidayCalendar(input);
			result.Name.Should().Be.EqualTo("New Calendar Name");
		}

		[Test]
		public void ShouldSaveBankHolidayCalendarWithAddingCalendarDates()
		{
			var calendar = PrepareData();

			var input = new BankHolidayCalendarForm
			{
				Id = calendar.Id.Value,
				Years = new List<BankHolidayYearForm>{
					new BankHolidayYearForm {
				Dates = new List<BankHolidayDateForm> {
					new BankHolidayDateForm { Date=new DateTime(2020,3,8),Description="Women Day" }
				} } }
			};


			var result = Target.SaveBankHolidayCalendar(input);

			result.Years.Count().Should().Be.EqualTo(3);
			result.Years.Last().Dates.First().Date.Should().Be.EqualTo(new DateTime(2020, 3, 8));
		}

		[Test]
		public void ShouldSaveBankHolidayCalendarWithUpdatingCalendarDates()
		{
			var calendar = PrepareData();

			var dateId = calendar.Dates.First().Id.Value;

			var input = new BankHolidayCalendarForm
			{
				Id = calendar.Id.Value,
				Years = new List<BankHolidayYearForm>{
					new BankHolidayYearForm {
				Dates = new List<BankHolidayDateForm> {
					new BankHolidayDateForm { Id=dateId, Date=_nationalDay.AddDays(1),Description="National Day2" }
				} } }
			};


			var result = Target.SaveBankHolidayCalendar(input);
			result.Years.Count().Should().Be.EqualTo(2);
			result.Years.First().Dates.First().Id.Should().Be.EqualTo(dateId);
			result.Years.First().Dates.First().Date.Should().Be.EqualTo(_nationalDay.AddDays(1));
			result.Years.First().Dates.First().Description.Should().Be.EqualTo("National Day2");
		}

		[Test]
		public void ShouldSaveBankHolidayCalendarWithDeletingCalendarDates()
		{
			var calendar = PrepareData();

			var dateId = calendar.Dates.First().Id.Value;

			var input = new BankHolidayCalendarForm
			{
				Id = calendar.Id.Value,
				Years = new List<BankHolidayYearForm>{
					new BankHolidayYearForm {
				Dates = new List<BankHolidayDateForm> {
					new BankHolidayDateForm { Id=dateId, IsDeleted=true }
				} } }
			};


			var result = Target.SaveBankHolidayCalendar(input);
			result.Years.Count().Should().Be.EqualTo(1);
			result.Years.First().Dates.First().Date.Should().Be.EqualTo(_newYear);
		}

		[Test]
		public void ShouldSaveBankHolidayCalendarWithAddingUpdatingDeletingCalendarDates()
		{
			var calendar = PrepareData();

			var dateDeletedId = calendar.Dates.First().Id.Value;
			var dateUpdatedId = calendar.Dates.Last().Id.Value;

			var input = new BankHolidayCalendarForm
			{
				Id = calendar.Id.Value,
				Years = new List<BankHolidayYearForm>{
					new BankHolidayYearForm {
				Dates = new List<BankHolidayDateForm> {
					new BankHolidayDateForm { Id=dateDeletedId, IsDeleted=true },
					new BankHolidayDateForm{ Date=new DateTime(2020,3,8),Description="Women Day" },
					new BankHolidayDateForm{ Id=dateUpdatedId,Description="Chinese New Year",Date=_springFestival }
				} } }
			};


			var result = Target.SaveBankHolidayCalendar(input);
			result.Years.Count().Should().Be.EqualTo(2);
			result.Years.Last().Dates.First().Date.Should().Be.EqualTo(new DateTime(2020, 3, 8));
			result.Years.Last().Dates.First().Description.Should().Be.EqualTo("Women Day");

			
			result.Years.First().Dates.Last().Date.Should().Be.EqualTo(_springFestival);
			result.Years.First().Dates.Last().Description.Should().Be.EqualTo("Chinese New Year");
		}

		[Test]
		public void ShouldLoadBankHolidayCalendarById()
		{

			var calendar = PrepareData();

			var result = Target.LoadBankHolidayCalendarById(calendar.Id.Value);

			result.Name.Should().Be.EqualTo("ChinaBankHoliday");
			result.Years.Count().Should().Be.EqualTo(2);
			result.Years.First().Year.Should().Be.EqualTo(2018);
			result.Years.First().Dates.First().Date.Should().Be.EqualTo(_nationalDay);
			result.Years.First().Dates.First().Description.Should().Be.EqualTo("National Day");
		}

		[Test]
		public void ShouldLoadBankHolidayCalendarWithSorterdByYear()
		{
			var calendar = PrepareData();

			var result = Target.LoadBankHolidayCalendarById(calendar.Id.Value);

			result.Years.Count().Should().Be.EqualTo(2);
			result.Years.First().Year.Should().Be.EqualTo(2018);
			result.Years.Last().Year.Should().Be.EqualTo(2019);
		}

		[Test]
		public void ShouldLoadBankHolidayCalendarWithSorterdByDate()
		{
			var calendar = PrepareData();

			var result = Target.LoadBankHolidayCalendarById(calendar.Id.Value);

			result.Years.Count().Should().Be.EqualTo(2);
			result.Years.Last().Year.Should().Be.EqualTo(2019);
			result.Years.Last().Dates.Count().Should().Be.EqualTo(2);
			result.Years.Last().Dates.First().Date.Should().Be.EqualTo(_newYear);
			result.Years.Last().Dates.Last().Date.Should().Be.EqualTo(_springFestival);

		}

		[Test]
		public void ShouldLoadAllBankHolidayCalendarsWithSorterdByName()
		{
			PrepareData();

			var Id = Guid.Parse("8cd8899a-4001-4b58-8f1d-dd39eba5fb95");

			var calendar = new BankHolidayCalendar("SwdenBankHoliday");
			calendar.SetId(Id);
			calendar.AddDate(new BankHolidayDate { Date = _nationalDay.AddDays(1), Description = "Swden National Day" });

			BankHolidayCalendarRepository.Add(calendar);

			var result = Target.LoadBankHolidayCalendars();

			result.First().Name.Should().Be.EqualTo("ChinaBankHoliday");
			result.Last().Name.Should().Be.EqualTo("SwdenBankHoliday");
		}

		[Test]
		public void ShouldLoadAllBankHolidayCalendars()
		{
			PrepareData();

			var Id = Guid.Parse("8cd8899a-4001-4b58-8f1d-dd39eba5fb95");

			var calendar = new BankHolidayCalendar("SwdenBankHoliday");
			calendar.SetId(Id);
			calendar.AddDate(new BankHolidayDate { Date = _nationalDay.AddDays(1), Description = "Swden National Day" });

			BankHolidayCalendarRepository.Add(calendar);

			var result = Target.LoadBankHolidayCalendars();

			result.Count().Should().Be.EqualTo(2);
			result.First().Name.Should().Be.EqualTo("ChinaBankHoliday");
			result.Last().Name.Should().Be.EqualTo("SwdenBankHoliday");
		}

		[Test]
		public void ShouldDeleteBankHolidayDatesWhenDeletingBankHolidayCalendar()
		{
			var calendar = PrepareData();

			var Id = calendar.Id.Value;

			var result = Target.DeleteBankHolidayCalendarById(Id);

			result.Should().Be.EqualTo(true);

			var calendars = BankHolidayCalendarRepository.LoadAll().Where(c => c.Id.Value == Id);
			calendars.Count().Should().Be.EqualTo(0);

			var dates = BankHolidayDateRepository.LoadAll().Where(d => d.Calendar.Id.Value == Id);
			dates.Count().Should().Be.EqualTo(0);
		}
	}
}
