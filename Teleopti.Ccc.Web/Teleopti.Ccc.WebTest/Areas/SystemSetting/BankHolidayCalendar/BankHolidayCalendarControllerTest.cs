using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Controller;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;

namespace Teleopti.Ccc.WebTest.Areas.SystemSetting.BankHolidayCalendar
{
	[TestFixture]
	[DomainTest]
	[WebTest]
	public class BankHolidayCalendarControllerTest: IIsolateSystem
	{
		public BankHolidayCalendarController Target;
		public FakeBankHolidayCalendarRepository BankHolidayCalendarRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeBankHolidayCalendarRepository>().For<IBankHolidayCalendarRepository>();
		}

		[Test]
		public void ShouldCreateBankHolidayCalendar()
		{
			var input = new BankHolidayForm
			{
				Name = "ChinaBankHoliday",
				Dates = new List<DateTime> {DateTime.Today}
			};

			var vm = Target.CreateBankHolidayCalendar(input);

			var result = BankHolidayCalendarRepository.Get(vm.Id);
			result.Name.Should().Be.EqualTo(input.Name);
			result.Dates.First().Should().Be.EqualTo(input.Dates.First());
		}
	}
}
