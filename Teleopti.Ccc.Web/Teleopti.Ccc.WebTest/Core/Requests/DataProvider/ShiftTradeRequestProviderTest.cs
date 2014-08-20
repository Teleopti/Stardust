using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
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

			var target = new ShiftTradeRequestProvider(loggedOnUser, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IPermissionProvider>());

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

			var target = new ShiftTradeRequestProvider(loggedOnUser, scheduleDayReadModelFinder, MockRepository.GenerateMock<IPermissionProvider>());

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

			scheduleDayReadModelFinder.Stub(x => x.ForPersons(date, new[] { person1.Id.Value, person2.Id.Value }, new Paging())).Return(scheduleReadModels);

			var target = new ShiftTradeRequestProvider(MockRepository.GenerateMock<ILoggedOnUser>(), scheduleDayReadModelFinder, MockRepository.GenerateMock<IPermissionProvider>());

			var result = target.RetrievePossibleTradeSchedules(date, new[] { person1, person2 }, new Paging());

			result.Should().Be.SameInstanceAs(scheduleReadModels);
		}

		[Test]
		public void ShouldGetScheduleForPossibleTradePersonsWithTimeFilter()
		{
			var scheduleDayReadModelFinder = MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>();
			DateOnly date = DateOnly.Today;
			var person1 = new Person();
			var person2 = new Person();
			person1.SetId(Guid.NewGuid());
			person2.SetId(Guid.NewGuid());
			var scheduleReadModels = new[] { new PersonScheduleDayReadModel(), new PersonScheduleDayReadModel() };
			var filterInfo = new TimeFilterInfo
			{
				StartTimes = new List<DateTimePeriod>() {new DateTimePeriod()},
				EndTimes = new List<DateTimePeriod>() {new DateTimePeriod()},
				IsDayOff = true
			};

			scheduleDayReadModelFinder.Stub(x => x.ForPersonsByFilteredTimes(date, new[] { person1.Id.Value, person2.Id.Value }, new Paging(), filterInfo)).Return(scheduleReadModels);

			var target = new ShiftTradeRequestProvider(MockRepository.GenerateMock<ILoggedOnUser>(), scheduleDayReadModelFinder, MockRepository.GenerateMock<IPermissionProvider>());

			var result = target.RetrievePossibleTradeSchedulesWithFilteredTimes(date, new[] { person1, person2 }, new Paging(), filterInfo);

			result.Should().Be.SameInstanceAs(scheduleReadModels);
		}

		[Test]
		public void ShouldReturnMyTeamForGivenDate()
		{
			var theDate = DateOnly.Today;
			var myTeam = new Team();
			myTeam.SetId(Guid.NewGuid());
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.MyTeam(theDate)).Return(myTeam);
			permissionProvider.Stub(
				x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, theDate, myTeam)).Return(true);

			var target = new ShiftTradeRequestProvider(loggedOnUser, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), permissionProvider);

			var result = target.RetrieveMyTeamId(theDate);

			result.Should().Be.EqualTo(myTeam.Id);
		}

		[Test]
		public void ShouldReturnNullIfNoTeamForGivenDate()
		{
			var theDate = DateOnly.Today;
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.MyTeam(theDate)).Return(null);

			var target = new ShiftTradeRequestProvider(loggedOnUser, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), MockRepository.GenerateMock<IPermissionProvider>());

			var result = target.RetrieveMyTeamId(theDate);

			result.Should().Be.EqualTo(null);
		}

		[Test]
		public void ShouldReturnNullIfNoPermissionForMyTeam()
		{
			var theDate = DateOnly.Today;
			var myTeam = new Team();
			myTeam.SetId(Guid.NewGuid());
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var person = MockRepository.GenerateMock<IPerson>();
			var permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			person.Stub(x => x.MyTeam(theDate)).Return(myTeam);
			permissionProvider.Stub(
				x => x.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb, theDate, myTeam)).Return(false);

			var target = new ShiftTradeRequestProvider(loggedOnUser, MockRepository.GenerateMock<IPersonScheduleDayReadModelFinder>(), permissionProvider);

			var result = target.RetrieveMyTeamId(theDate);

			result.Should().Be.EqualTo(null);
		}
	}
}
