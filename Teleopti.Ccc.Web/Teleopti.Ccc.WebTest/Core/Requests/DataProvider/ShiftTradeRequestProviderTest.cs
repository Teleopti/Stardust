using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		public void ShouldRetrieveWorkflowControlSetForUser()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = new Person { WorkflowControlSet = new WorkflowControlSet() };

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var target = new ShiftTradeRequestProvider(loggedOnUser, null);

			var result = target.RetrieveShiftTradePreparationData(null);

			result.WorkflowControlSet.Should().Be.SameInstanceAs(person.WorkflowControlSet);
		}

		[Test]
		public void ShouldRetrieveScheduleForUser()
		{
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var person = new Person { WorkflowControlSet = new WorkflowControlSet() };
			var date = DateTime.UtcNow;
			IScheduleDictionary scheduleDictionary = new ScheduleDictionary(new Scenario("scenario"),
			                                                                new ScheduleDateTimePeriod(new DateTimePeriod(date,
			                                                                                                              date)));
			IScheduleDay scheduleDay = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, person, new DateOnly(date));

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			scheduleProvider.Stub(x => x.GetScheduleForPersons(new DateOnly(date), new List<IPerson> {person})).Return(new[]
			                                                                                                           	{
			                                                                                                           		scheduleDay
			                                                                                                           	});
			var target = new ShiftTradeRequestProvider(loggedOnUser, scheduleProvider);

			var result = target.RetrieveShiftTradePreparationData(new DateOnly(date));

			result.WorkflowControlSet.Should().Be.SameInstanceAs(person.WorkflowControlSet);
			result.MyScheduleDay.Should().Be.SameInstanceAs(scheduleDay);
		}
	}
}
