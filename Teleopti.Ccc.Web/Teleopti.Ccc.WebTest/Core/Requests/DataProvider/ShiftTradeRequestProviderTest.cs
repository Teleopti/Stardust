using System;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class ShiftTradeRequestProviderTest
	{
		[Test]
		public void ShouldGetWorkflowControlSetForUser()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person { WorkflowControlSet = new WorkflowControlSet() };

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var target = new ShiftTradeRequestProvider(loggedOnUser, MockRepository.GenerateMock<IScheduleProvider>(), null);

			var result = target.RetrieveUserWorkflowControlSet();

			result.Should().Be.SameInstanceAs(person.WorkflowControlSet);
		}

		[Test]
		public void ShouldGetMyScheduleForADay()
		{
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var person = new Person();
			var date = DateOnly.Today;
			IScheduleDictionary scheduleDictionary = new ScheduleDictionary(new Scenario("scenario"),
																			new ScheduleDateTimePeriod(
																				new DateTimePeriod(
																					DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
																					DateTime.SpecifyKind(date.Date, DateTimeKind.Utc))));
			IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, person, date);

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(new DateOnlyPeriod(date, date))).Return(new[] { scheduleDay });

			var target = new ShiftTradeRequestProvider(MockRepository.GenerateMock<ILoggedOnUser>(), scheduleProvider, null);

			var result = target.RetrieveMyScheduledDay(date);

			result.Should().Be.SameInstanceAs(scheduleDay);
		}

		[Test]
		public void ShouldGetScheduleForPossibleTradePersons()
		{
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var possibleTradePersonProvider = MockRepository.GenerateMock<IPossibleShiftTradePersonsProvider>();
			var personList = new[] { new Person() };
			var date = DateOnly.Today;
			IScheduleDictionary scheduleDictionary = new ScheduleDictionary(new Scenario("scenario"),
																			new ScheduleDateTimePeriod(
																				new DateTimePeriod(
																					DateTime.SpecifyKind(date.Date, DateTimeKind.Utc),
																					DateTime.SpecifyKind(date.Date, DateTimeKind.Utc))));
			IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, personList[0], date);

			possibleTradePersonProvider.Stub(x => x.RetrievePersons(date)).Return(personList);
			scheduleProvider.Stub(x => x.GetScheduleForPersons(date, personList)).Return(new[] { scheduleDay });

			
			var target = new ShiftTradeRequestProvider(MockRepository.GenerateMock<ILoggedOnUser>(), scheduleProvider, possibleTradePersonProvider);

			var result = target.RetrievePossibleTradePersonsScheduleDay(date);

			result.FirstOrDefault().Should().Be.SameInstanceAs(scheduleDay);
		}
	}
}
