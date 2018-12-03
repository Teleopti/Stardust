using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	public class RequestsShiftTradeBulletinBoardControllerTest : IIsolateSystem
	{
		public RequestsShiftTradeBulletinBoardController Target;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public IScheduleStorage ScheduleStorage;
		public FakePeopleForShiftTradeFinder PeopleForShiftTradeFinder;
		public FakePersonScheduleDayReadModelFinder PersonScheduleDayReadModelFinder;
		public ITeamRepository TeamRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePeopleForShiftTradeFinder>().For<IPeopleForShiftTradeFinder>();
			isolate.UseTestDouble<FakeCommonAgentNameProvider>().For<ICommonAgentNameProvider>();
		}

		[Test]
		public void ShouldGetAllBulletinSchedules()
		{
			var person = PersonFactory.CreatePersonWithId();

			PersonRepository.Has(person);
			PeopleForShiftTradeFinder.Has(new PersonAuthorization { PersonId = person.Id.Value });

			var dayOffAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, new Scenario("scenario"), new DateOnly(2007, 1, 1), new DayOffTemplate(new Description("for", "test")));
			ScheduleStorage.Add(dayOffAssignment);

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var currentShift = ScheduleDayFactory.Create(new DateOnly(2007, 1, 1), person);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.SetId(new Guid("CB3396E0-5B4D-47CD-9D93-ED4A10725A53"));
			currentShift.AddMainShift(EditableShiftFactory.CreateEditorShift(activity,
				period.ChangeEndTime(TimeSpan.FromHours(3)),
				ShiftCategoryFactory.CreateShiftCategory("Early")));
			var shiftTradeOffer =
				new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending)
				{
					Criteria = new ShiftExchangeCriteria(DateOnly.Today.AddDays(1),
						new ScheduleDayFilterCriteria { DayType = ShiftExchangeLookingForDay.DayOffOrEmptyDay }),
					ShiftExchangeOfferId = Guid.NewGuid().ToString()
				}.WithId();

			var personRequest = new PersonRequest(person, shiftTradeOffer);
			PersonRequestRepository.Add(personRequest);

			var otherPerson = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(otherPerson);
			PeopleForShiftTradeFinder.Has(new PersonAuthorization { PersonId = otherPerson.Id.Value });

			var result = Target.BulletinSchedules(new DateOnly(2007, 1, 1), Guid.NewGuid().ToString(), new Paging
			{
				Skip = 0,
				Take = 10
			});

			var model = result.Data as ShiftTradeScheduleViewModel;

			model.PossibleTradeSchedules.Count().Should().Be(1);
		}

		[Test]
		public void ShouldGetAllBulletinSchedulesWithFilters()
		{
			var site = SiteFactory.CreateSimpleSite("site");
			var team = TeamFactory.CreateTeamWithId("team");
			team.Site = site;
			TeamRepository.Add(team);

			var person = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(person);
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(2000, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PeopleForShiftTradeFinder.Has(new PersonAuthorization { PersonId = person.Id.Value, TeamId = team.Id.Value });

			var dayOffAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(person, new Scenario("scenario"), new DateOnly(2007, 1, 1), new DayOffTemplate(new Description("for", "test")));
			ScheduleStorage.Add(dayOffAssignment);

			var period = new DateTimePeriod(new DateTime(2007, 1, 1, 3, 0, 0, DateTimeKind.Utc),
				new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc));
			var currentShift = ScheduleDayFactory.Create(new DateOnly(2007, 1, 1), person);
			var activity = ActivityFactory.CreateActivity("Phone");
			activity.SetId(new Guid("CB3396E0-5B4D-47CD-9D93-ED4A10725A53"));
			currentShift.AddMainShift(EditableShiftFactory.CreateEditorShift(activity,
				period.ChangeEndTime(TimeSpan.FromHours(3)),
				ShiftCategoryFactory.CreateShiftCategory("Early")));
			var shiftTradeOffer =
				new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending)
				{
					Criteria = new ShiftExchangeCriteria(DateOnly.Today.AddDays(1),
						new ScheduleDayFilterCriteria {DayType = ShiftExchangeLookingForDay.DayOffOrEmptyDay}),
					ShiftExchangeOfferId = Guid.NewGuid().ToString()
				}.WithId();

			var personRequest = new PersonRequest(person, shiftTradeOffer);
			PersonRequestRepository.Add(personRequest);

			var otherPerson = PersonFactory.CreatePersonWithId();
			PersonRepository.Has(otherPerson);
			otherPerson.AddPersonPeriod(new PersonPeriod(new DateOnly(2000, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			PeopleForShiftTradeFinder.Has(new PersonAuthorization {PersonId = otherPerson.Id.Value, TeamId = team.Id.Value});

			var result = Target.BulletinSchedulesWithTimeFilter(new DateOnly(2007, 1, 1), new ScheduleFilter { TeamIds = team.Id.Value.ToString() }, new Paging
			{
				Skip = 0,
				Take = 10
			});

			var model = result.Data as ShiftTradeScheduleViewModel;

			model.PossibleTradeSchedules.Count().Should().Be(1);
		}
	}
}
