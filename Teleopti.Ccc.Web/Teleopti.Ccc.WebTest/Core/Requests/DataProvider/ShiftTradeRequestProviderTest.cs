using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

			var target = new ShiftTradeRequestProvider(loggedOnUser, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>());

			var result = target.RetrieveUserWorkflowControlSet();

			result.Should().Be.SameInstanceAs(person.WorkflowControlSet);
		}

		[Test]
		public void ShouldGetMyScheduleForADay()
		{
			var scheduleDayReadModelFinder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();

			DateOnly date = DateOnly.Today;
			var scheduleReadModel = new PersonScheduleDayReadModel();
			var person = new Person();
			person.SetId(Guid.NewGuid());

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			scheduleDayReadModelFinder.Stub(x => x.ForPerson(date, person.Id.Value)).Return(scheduleReadModel);

			var target = new ShiftTradeRequestProvider(loggedOnUser, scheduleDayReadModelFinder);

			target.RetrieveMySchedule(date).Should().Be.SameInstanceAs(scheduleReadModel);
		}

		[Test]
		public void ShouldGetScheduleForPossibleTradePersons()
		{
			var scheduleDayReadModelFinder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			DateOnly date = DateOnly.Today;
			var person1 = new Person();
			var person2 = new Person();
			person1.SetId(Guid.NewGuid());
			person2.SetId(Guid.NewGuid());
			var scheduleReadModels = new[] {new PersonScheduleDayReadModel(), new PersonScheduleDayReadModel()};

			scheduleDayReadModelFinder.Stub(x => x.ForPeople(new DateTimePeriod(date, date), new[] { person1.Id.Value, person2.Id.Value })).Return(scheduleReadModels);

			var target = new ShiftTradeRequestProvider(MockRepository.GenerateMock<ILoggedOnUser>(), scheduleDayReadModelFinder);

			var result = target.RetrievePossibleTradeSchedules(date, new[] { person1, person2 });

			result.Should().Be.SameInstanceAs(scheduleReadModels);
		}
	}
}
