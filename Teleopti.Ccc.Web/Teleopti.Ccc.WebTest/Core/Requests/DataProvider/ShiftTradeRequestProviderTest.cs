using System;
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

			var target = new ShiftTradeRequestProvider(loggedOnUser, MockRepository.GenerateMock<IScheduleProvider>());

			var result = target.RetrieveUserWorkflowControlSet();

			result.Should().Be.SameInstanceAs(person.WorkflowControlSet);
		}

		[Test]
		public void ShouldGetMyScheduleForADay()
		{
			
		}

		[Test]
		public void ShouldGetScheduleForPossibleTradePersons()
		{
			
		}
	}
}
