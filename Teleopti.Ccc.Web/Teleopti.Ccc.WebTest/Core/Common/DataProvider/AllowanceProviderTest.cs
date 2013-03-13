using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Common.DataProvider
{
	[TestFixture]
	public class AllowanceProviderTest
	{

		[Test]
		public void ShouldGetAllowance()
		{
			var person = MockRepository.GenerateMock<IPerson>();
			var budgetDayRepository = MockRepository.GenerateMock<IBudgetDayRepository>();
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today);
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var target = new AllowanceProvider(budgetDayRepository, userTimeZone, loggedOnUser, scenarioRepository);

			var result = target.GetAllowanceForPeriod(period);

			result.First().Date.Should().Be.EqualTo(period.StartDate.Date);

		}
	}
}