using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;
using Teleopti.Ccc.Web.Areas.Requests.Core.FormData;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Core.ViewModelFactory
{
	[TestFixture, RequestsTest]
	public class ShiftTradeRequestViewModelFactoryTest : ISetup
	{
		public ICurrentScenario Scenario;
		public IShiftTradeRequestViewModelFactory ShiftTradeRequestViewModelFactory;
		public IPersonRequestRepository PersonRequestRepository;
		public IPermissionProvider PermissionProvider;
		public IPeopleSearchProvider PeopleSearchProvider;
		public IPersonRequestCheckAuthorization PersonRequestCheckAuthorization;
		public IScheduleStorage ScheduleStorage;
		public IShiftTradeRequestStatusChecker ShiftTradeRequestStatusChecker;
		public IUserCulture UserCulture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			setupStateHolderProxy();
			system.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
		}
		
		[Test]
		public void ShouldGetNothingWhenSelectNoTeam()
		{
			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2016, 3, 1),
				EndDate = new DateOnly(2016, 3, 1),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new List<Guid>().ToArray()
			};

			var requestListViewModel = ShiftTradeRequestViewModelFactory.CreateRequestListViewModel(input);
			requestListViewModel.TotalCount.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetShiftTradeRequest()
		{
			runSingleShiftTradeRequestTest();
		}

		[Test]
		public void ShouldOnlyGetShiftTradeRequest()
		{
			createTextRequest(new DateTimePeriod(2016, 03, 01, 2016, 03, 02));
			runSingleShiftTradeRequestTest();
		}

		[Test]
		public void ShouldGetMutipleShiftTradeRequests()
		{
			createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 1),
					PersonFactory.CreatePerson("Person", "From"), PersonFactory.CreatePerson("Person", "To"));

			createShiftTradeRequest(new DateOnly(2016, 3, 5), new DateOnly(2016, 3, 5),
				PersonFactory.CreatePerson("Person2", "From2"), PersonFactory.CreatePerson("Person2", "To2"));

			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 5));
			requestListViewModel.Requests.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnMinimumAndMaximumDateOfInputWhenAllRequestsAreInsidePeriod()
		{
			createShiftTradeRequest(new DateOnly(2016, 3, 2), new DateOnly(2016, 3, 2),
					PersonFactory.CreatePerson("Person", "From"), PersonFactory.CreatePerson("Person", "To"));

			createShiftTradeRequest(new DateOnly(2016, 3, 5), new DateOnly(2016, 3, 8),
				PersonFactory.CreatePerson("Person2", "From2"), PersonFactory.CreatePerson("Person2", "To2"));

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2016, 3, 1),
				EndDate = new DateOnly(2016, 3, 10),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new[] { Guid.NewGuid() }
			};

			var requestListViewModel = ShiftTradeRequestViewModelFactory.CreateRequestListViewModel(input);

			requestListViewModel.MinimumDateTime.Should().Be.EqualTo(input.StartDate.Date);
			requestListViewModel.MaximumDateTime.Should().Be.EqualTo(input.EndDate.Date);
		}
		


		[Test]
		public void ShouldReturnMaximumDateOfRequestWhenOutsideOfInputPeriod()
		{
			createShiftTradeRequest(new DateOnly(2016, 3, 2), new DateOnly(2016, 3, 2),
					PersonFactory.CreatePerson("Person", "From"), PersonFactory.CreatePerson("Person", "To"));

			createShiftTradeRequest(new DateOnly(2016, 3, 5), new DateOnly(2016, 3, 8),
				PersonFactory.CreatePerson("Person2", "From2"), PersonFactory.CreatePerson("Person2", "To2"));

			var shiftTradeWithLaterEndDate  = createShiftTradeRequest(new DateOnly(2016, 3, 9), new DateOnly(2016, 3, 11),
				PersonFactory.CreatePerson("Person2", "From2"), PersonFactory.CreatePerson("Person2", "To2"));

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2016, 3, 1),
				EndDate = new DateOnly(2016, 3, 10),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new[] { Guid.NewGuid() }
			};

			var requestListViewModel = ShiftTradeRequestViewModelFactory.CreateRequestListViewModel(input);

			requestListViewModel.MinimumDateTime.Should().Be.EqualTo(input.StartDate.Date);
			requestListViewModel.MaximumDateTime.Should().Be.EqualTo(shiftTradeWithLaterEndDate.Request.Period.EndDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone));
		}

		[Test]
		public void ShouldOnlyRequestStartingInsidePeriod()
		{
			createShiftTradeRequest(new DateOnly(2016, 2, 27), new DateOnly(2016, 3, 2),
					PersonFactory.CreatePerson("Person", "From"), PersonFactory.CreatePerson("Person", "To"));

			createShiftTradeRequest(new DateOnly(2016, 3, 5), new DateOnly(2016, 3, 8),
				PersonFactory.CreatePerson("Person2", "From2"), PersonFactory.CreatePerson("Person2", "To2"));
			
			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2016, 3, 1),
				EndDate = new DateOnly(2016, 3, 10),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new []{Guid.NewGuid()}
			};

			var requestListViewModel = ShiftTradeRequestViewModelFactory.CreateRequestListViewModel(input);

			requestListViewModel.Requests.Count().Should().Be.EqualTo (1);
			requestListViewModel.MinimumDateTime.Should().Be.EqualTo(input.StartDate.Date);
		}

		[Test]
		public void ShouldGetShiftTradeRequestPersonToDetails()
		{
			var personToTeam = TeamFactory.CreateTeam("Team", "SiteName");
			var timeZone = TimeZoneInfoFactory.ChinaTimeZoneInfo();

			var personTo = PersonFactory.CreatePersonWithPersonPeriodFromTeam(new DateOnly(2016, 01, 01), personToTeam);
			personTo.SetId(Guid.NewGuid());
			personTo.PermissionInformation.SetDefaultTimeZone(timeZone);

			createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 1),
					PersonFactory.CreatePerson("Person", "From"), personTo);

			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 5));
			var requestViewModel = (ShiftTradeRequestViewModel)requestListViewModel.Requests.First();

			requestViewModel.PersonTo.Should().Not.Be.NullOrEmpty();
			requestViewModel.PersonIdTo.Should().Be.EqualTo(personTo.Id);
			requestViewModel.PersonToTeam.Should().Be(personToTeam.SiteAndTeam);
			requestViewModel.PersonToTimeZone.Should().Be(new IanaTimeZoneProvider().WindowsToIana(timeZone.Id));
		}

		[Test]
		public void ShouldGetShiftTradeDay()
		{
			var personTo = PersonFactory.CreatePerson("Person", "To");
			var personFrom = PersonFactory.CreatePerson("Person", "From");

			var personrequest = createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 1), personFrom, personTo);
			var shiftTradeRequest = (ShiftTradeRequest)personrequest.Request;

			var personToAssignment = addPersonAssignment(personTo, "sdfTo", "shiftCategory", Color.PaleVioletRed, new DateOnly(2016, 3, 1));
			var personFromAssignment = addPersonAssignment(personFrom, "sdfFrom", "shiftCategoryFrom", Color.AliceBlue, new DateOnly(2016, 3, 1));

			var schedule = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(2016, 03, 01, 2016, 03, 02), Scenario.Current());

			setShiftTradeSwapDetailsToAndFrom(shiftTradeRequest, schedule, personTo, personFrom);

			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 1));
			var requestViewModel = (ShiftTradeRequestViewModel)requestListViewModel.Requests.First();
			var shiftTradeDay = requestViewModel.ShiftTradeDays.Single();

			shiftTradeDay.Date.Should().Be(new DateOnly(requestViewModel.PeriodStartTime));
			shiftTradeDay.ToScheduleDayDetail.Name.Should().Be(personToAssignment.ShiftCategory.Description.Name);
			shiftTradeDay.ToScheduleDayDetail.ShortName.Should().Be(personToAssignment.ShiftCategory.Description.ShortName);
			shiftTradeDay.FromScheduleDayDetail.Name.Should().Be(personFromAssignment.ShiftCategory.Description.Name);
			shiftTradeDay.FromScheduleDayDetail.ShortName.Should().Be(personFromAssignment.ShiftCategory.Description.ShortName);
			shiftTradeDay.ToScheduleDayDetail.Color.Should().Be(personToAssignment.ShiftCategory.DisplayColor.ToHtml());
			shiftTradeDay.ToScheduleDayDetail.Type.Should().Be(ShiftObjectType.PersonAssignment);
			shiftTradeDay.FromScheduleDayDetail.Color.Should().Be(personFromAssignment.ShiftCategory.DisplayColor.ToHtml());
		}

		[Test]
		public void ShouldGetShiftTradeDayWithAbsence()
		{
			var personTo = PersonFactory.CreatePerson("Person", "To").WithId();
			var personFrom = PersonFactory.CreatePerson("Person", "From").WithId();

			var personrequest = createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 1), personFrom, personTo);
			var shiftTradeRequest = (ShiftTradeRequest)personrequest.Request;

			var personToAssignment = addPersonAssignment(personTo, "sdfTo", "shiftCategory", Color.PaleVioletRed, new DateOnly(2016, 3, 1));
			var personFromAbsence = addPersonAbsence (personFrom, "Holiday", "HO", Color.Aqua, new DateOnly (2016, 3, 1));

			var schedule = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom }, new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(2016, 03, 01, 2016, 03, 02), Scenario.Current());

			setShiftTradeSwapDetailsToAndFrom(shiftTradeRequest, schedule, personTo, personFrom);

			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 1));
			var requestViewModel = (ShiftTradeRequestViewModel)requestListViewModel.Requests.First();
			var shiftTradeDay = requestViewModel.ShiftTradeDays.Single();

			shiftTradeDay.Date.Should().Be(new DateOnly(requestViewModel.PeriodStartTime));
			shiftTradeDay.ToScheduleDayDetail.Name.Should().Be(personToAssignment.ShiftCategory.Description.Name);
			shiftTradeDay.ToScheduleDayDetail.ShortName.Should().Be(personToAssignment.ShiftCategory.Description.ShortName);
			shiftTradeDay.FromScheduleDayDetail.Name.Should().Be(personFromAbsence.Layer.Payload.Description.Name);
			shiftTradeDay.FromScheduleDayDetail.ShortName.Should().Be(personFromAbsence.Layer.Payload.Description.ShortName);
			shiftTradeDay.ToScheduleDayDetail.Color.Should().Be(personToAssignment.ShiftCategory.DisplayColor.ToHtml());
			shiftTradeDay.ToScheduleDayDetail.Type.Should().Be(ShiftObjectType.PersonAssignment);
			shiftTradeDay.FromScheduleDayDetail.Type.Should().Be(ShiftObjectType.FullDayAbsence);
			shiftTradeDay.FromScheduleDayDetail.Color.Should().Be(Color.White.ToHtml());
		}

		[Test]
		public void ShouldGetMultiDayShiftTradeWithDayOff()
		{
			var personTo = PersonFactory.CreatePerson("Person", "To");
			var personFrom = PersonFactory.CreatePerson("Person", "From");

			var personrequest = createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3), personFrom, personTo);
			var shiftTradeRequest = (ShiftTradeRequest)personrequest.Request;

			addPersonAssignment(personTo, "sdfTo", "shiftCategoryTo", Color.PaleVioletRed, new DateOnly(2016, 3, 1));
			addPersonAssignment(personFrom, "sdfFrom", "shiftCategoryFrom", Color.AliceBlue, new DateOnly(2016, 3, 1));
			addDayOff(personTo, "DayOff", "DO", Color.Gray, new DateOnly(2016, 3, 2));
			addPersonAssignment(personFrom, "sdfFrom", "shiftCategoryFrom", Color.AliceBlue, new DateOnly(2016, 3, 2));

			var schedule = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { personTo, personFrom },
				new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(2016, 03, 01, 2016, 03, 03), Scenario.Current());

			setShiftTradeSwapDetailsToAndFrom(shiftTradeRequest, schedule, personTo, personFrom);

			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3));
			var requestViewModel = (ShiftTradeRequestViewModel)requestListViewModel.Requests.First();

			var shiftTradeDay = requestViewModel.ShiftTradeDays.Single(sftTradeDay => sftTradeDay.Date == new DateOnly(2016, 3, 2));

			requestViewModel.ShiftTradeDays.Count().Should().Be(3);
			shiftTradeDay.ToScheduleDayDetail.Name.Should().Be("DayOff");
			shiftTradeDay.ToScheduleDayDetail.Type.Should().Be(ShiftObjectType.DayOff);
			shiftTradeDay.ToScheduleDayDetail.ShortName.Should().Be("DO");
			shiftTradeDay.ToScheduleDayDetail.Color.Should().Be(Color.Gray.ToHtml());
		}


		[Test]
		public void ShouldUpdateShiftTradeStatusIfScheduleHasBeenUpdated()
		{
			var theDate = new DateOnly (2016, 3, 1);
			// shift trade request was created and then someone has updated the schedule, changing the status in
			// this scenario can happen lazily when we fetch the requests 

			var personTo = PersonFactory.CreatePerson ("Person", "To");
			var personFrom = PersonFactory.CreatePerson ("Person", "From");

			var personRequest = createShiftTradeRequest (theDate, theDate, personFrom, personTo);
			var shiftTradeRequest = (ShiftTradeRequest) personRequest.Request;
			personRequest.Pending();

			var personAssignmentTo = createShiftTradeDetails (personTo, personFrom, shiftTradeRequest, theDate)[personTo];

			//change schedule so that the request will be (hopefully) set to referred
			personAssignmentTo.MoveActivityAndKeepOriginalPriority (personAssignmentTo.ShiftLayers.First(),
				new DateTime (theDate.Year, theDate.Month, theDate.Day, 2, 0, 0, DateTimeKind.Utc), new TrackedCommandInfo());

			createRequestListViewModel (theDate, theDate);

			var status = shiftTradeRequest.GetShiftTradeStatus (new ShiftTradeRequestStatusCheckerForTestDoesNothing());
			status.Should().Be (ShiftTradeStatus.Referred);

		}

		[Test]
		public void ShouldFilterReferredRequestWhenStatusUpdateChangesRequestToReferred()
		{
			var theDate = new DateOnly (2016, 3, 1);
			// shift trade request was created and then someone has updated the schedule, as the 'referred' status is 
			// set lazily when we fetch the requests, this will mean we need to filter it seperately.

			var personTo = PersonFactory.CreatePerson ("Person", "To");
			var personFrom = PersonFactory.CreatePerson ("Person", "From");

			var personRequest = createShiftTradeRequest (theDate, theDate, personFrom, personTo);
			var shiftTradeRequest = (ShiftTradeRequest) personRequest.Request;
			var shiftTradeRequest1PersonAssignmentTo =
				createShiftTradeDetails (personTo, personFrom, shiftTradeRequest, theDate)[personTo];
			personRequest.Pending();

			shiftTradeRequest1PersonAssignmentTo.MoveActivityAndKeepOriginalPriority (
				shiftTradeRequest1PersonAssignmentTo.ShiftLayers.First(),
				new DateTime (theDate.Year, theDate.Month, theDate.Day, 2, 0, 0, DateTimeKind.Utc), new TrackedCommandInfo());

			var personRequest2 = createShiftTradeRequest (theDate, theDate, personFrom, personTo);
			var shiftTradeRequest2 = (ShiftTradeRequest) personRequest2.Request;
			createShiftTradeDetails (personTo, personFrom, shiftTradeRequest2, theDate);
			personRequest2.Pending();

			var requestListViewModel = createRequestListViewModel (theDate, theDate);

			Assert.AreEqual (1, requestListViewModel.Requests.Count());
			Assert.AreEqual (personRequest2.Id, requestListViewModel.Requests.First().Id);

		}


		[Test]
		public void ShouldGetNoRequestsWithUnmatchedDate()
		{
			var personTo = PersonFactory.CreatePerson("Person", "To");
			var personFrom = PersonFactory.CreatePerson("Person", "From");

			createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3), personFrom, personTo);

			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 2, 1), new DateOnly(2016, 2, 3));
			requestListViewModel.Requests.Count().Should().Be(0);
		}

		[Test]
		public void ShouldGetNoRequestsWithNullInput()
		{
			var personTo = PersonFactory.CreatePerson("Person", "To");
			var personFrom = PersonFactory.CreatePerson("Person", "From");

			createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3), personFrom, personTo);

			var requestListViewModel = ShiftTradeRequestViewModelFactory.CreateRequestListViewModel(null);
			requestListViewModel.Requests.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotReturnRequestsInOkByMeStatus()
		{
			var personTo = PersonFactory.CreatePerson("Person", "To");
			var personFrom = PersonFactory.CreatePerson("Person", "From");

			var personrequest = createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3), personFrom, personTo);
			((ShiftTradeRequest)personrequest.Request).SetShiftTradeStatus(ShiftTradeStatus.OkByMe, new PersonRequestAuthorizationCheckerConfigurable());

			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3));
			requestListViewModel.Requests.Count().Should().Be(0);
		}

		[Test]
		public void ShouldNotReturnRequestsInReferredStatus()
		{
			var personTo = PersonFactory.CreatePerson("Person", "To");
			var personFrom = PersonFactory.CreatePerson("Person", "From");

			var personrequest = createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3), personFrom, personTo);
			((ShiftTradeRequest)personrequest.Request).SetShiftTradeStatus(ShiftTradeStatus.Referred, new PersonRequestAuthorizationCheckerConfigurable());

			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3));
			requestListViewModel.Requests.Count().Should().Be(0);
		}

		[Test]
		public void ShouldGetBrokenRules()
		{
			var personTo = PersonFactory.CreatePerson ("Person", "To");
			var personFrom = PersonFactory.CreatePerson ("Person", "From");

			var personrequest = createShiftTradeRequest (new DateOnly (2016, 3, 1), new DateOnly (2016, 3, 3), personFrom,
				personTo);
			((ShiftTradeRequest) personrequest.Request).SetShiftTradeStatus (ShiftTradeStatus.OkByBothParts,
				new PersonRequestAuthorizationCheckerConfigurable());
			var schedule = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod (new[] {personTo, personFrom},
				new ScheduleDictionaryLoadOptions (false, false),
				new DateOnlyPeriod (2016, 03, 01, 2016, 03, 03), Scenario.Current());
			setShiftTradeSwapDetailsToAndFrom ((IShiftTradeRequest) personrequest.Request, schedule, personTo, personFrom);
			personrequest.Pending();

			personrequest.TrySetBrokenBusinessRule (BusinessRuleFlags.DataPartOfAgentDay | BusinessRuleFlags.MinWeeklyRestRule);

			var requestListViewModel = createRequestListViewModel (new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3));
			var brokenRules = ((ShiftTradeRequestViewModel) requestListViewModel.Requests.FirstOrDefault()).BrokenRules;
			brokenRules.Count().Should().Be (2);
			brokenRules.Contains ("NotAllowedChange").Should().Be (true);
			brokenRules.Contains ("WeeklyRestTime").Should().Be (true);
		}


		[Test]
		public void ShouldGetISO8601FirstDayOfWeek()
		{
			var requestListViewModel = createRequestListViewModel(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 3));
			requestListViewModel.FirstDayOfWeek.Should().Be (7);
		}


		private static void setShiftTradeSwapDetailsToAndFrom(IShiftTradeRequest shiftTradeRequest, IScheduleDictionary schedule, IPerson personTo, IPerson personFrom)
		{
			foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
			{
				var scheduleDayTo = schedule[personTo].ScheduledDay(shiftTradeSwapDetail.DateTo);
				var scheduleDayFrom = schedule[personFrom].ScheduledDay(shiftTradeSwapDetail.DateFrom);
				shiftTradeSwapDetail.SchedulePartFrom = scheduleDayFrom;
				shiftTradeSwapDetail.SchedulePartTo = scheduleDayTo;
				shiftTradeSwapDetail.ChecksumFrom = new ShiftTradeChecksumCalculator (scheduleDayFrom).CalculateChecksum();
				shiftTradeSwapDetail.ChecksumTo = new ShiftTradeChecksumCalculator(scheduleDayTo).CalculateChecksum();
			}
		}

		private PersonAssignment addPersonAssignment(IPerson person, string activityName, string categoryName, Color displayColor, DateOnly date)
		{
			var personAssignment = new PersonAssignment(person, Scenario.Current(), date);
			//personAssignment.AddActivity(new Activity(activityName), date.ToDateTimePeriod(TimeZoneInfo.Utc));
			personAssignment.AddActivity(new Activity(activityName), new DateTimePeriod(date.Date.Utc(), date.Date.AddHours (8).Utc()));
			personAssignment.SetShiftCategory(new ShiftCategory(categoryName)
			{
				DisplayColor = displayColor
			});

			ScheduleStorage.Add(personAssignment);
			return personAssignment;
		}

		private PersonAbsence addPersonAbsence(IPerson person, string name, string shortName, Color displayColor, DateOnly date)
		{
			var personfromAssignment = addPersonAssignment(person, "sdfTo", "shiftCategory", Color.PaleVioletRed, date);
			ScheduleStorage.Add(personfromAssignment);

			var personAbsence = new PersonAbsence(person, Scenario.Current(), new AbsenceLayer (AbsenceFactory.CreateAbsence (name, shortName, displayColor), date.ToDateTimePeriod (TimeZoneInfo.Utc)));
			ScheduleStorage.Add(personAbsence);

			return personAbsence;
		}

		private PersonAssignment addDayOff(IPerson personTo, string name, string shortName, Color displayColor, DateOnly date)
		{
			var personAssignment = new PersonAssignment(personTo, Scenario.Current(), date);
			var dayOff = new DayOffTemplate(new Description(name, shortName)) { DisplayColor = displayColor };

			personAssignment.SetDayOff(dayOff);

			ScheduleStorage.Add(personAssignment);
			return personAssignment;
		}

		private void runSingleShiftTradeRequestTest()
		{
			getSimpleRequestListViewModel().Requests.Count().Should().Be.EqualTo(1);
		}

		private RequestListViewModel getSimpleRequestListViewModel()
		{
			createShiftTradeRequest(new DateOnly(2016, 3, 1), new DateOnly(2016, 3, 1),
				PersonFactory.CreatePerson("Person", "From"), PersonFactory.CreatePerson("Person", "To"));

			var input = new AllRequestsFormData
			{
				StartDate = new DateOnly(2016, 3, 1),
				EndDate = new DateOnly(2016, 3, 1),
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new [] {Guid.NewGuid()}
			};

			var requestListViewModel = ShiftTradeRequestViewModelFactory.CreateRequestListViewModel(input);
			return requestListViewModel;
		}

		private PersonRequest createShiftTradeRequest(DateOnly dateFrom, DateOnly dateTo, IPerson personFrom, IPerson personTo)
		{
			var shiftTradeSwapDetailList = new List<IShiftTradeSwapDetail>();

			var dateOnlyPeriod = new DateOnlyPeriod(dateFrom, dateTo);

			foreach (var day in dateOnlyPeriod.DayCollection())
			{
				shiftTradeSwapDetailList.Add(new ShiftTradeSwapDetail(personFrom, personTo, day, day));
			}

			var shiftTradeRequest = new ShiftTradeRequest(shiftTradeSwapDetailList);

			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts,
				new PersonRequestAuthorizationCheckerConfigurable());

			var personRequest = new PersonRequest (personFrom, shiftTradeRequest).WithId();

			((FakePersonRequestRepository)PersonRequestRepository).Add(personRequest);

			return personRequest;
		}

		private void createTextRequest(DateTimePeriod dateTimePeriod)
		{
			var textRequest = new TextRequest(dateTimePeriod);
			var personRequest = new PersonRequest(PersonFactory.CreatePerson("test1"), textRequest).WithId();
			((FakePersonRequestRepository)PersonRequestRepository).Add(personRequest);
		}

		private static void setupStateHolderProxy()
		{
			var stateMock = new FakeState();
			var dataSource = new DataSource(UnitOfWorkFactoryFactory.CreateUnitOfWorkFactory("for test"), null, null);
			var loggedOnPerson = StateHolderProxyHelper.CreateLoggedOnPerson();
			StateHolderProxyHelper.CreateSessionData(loggedOnPerson, dataSource, BusinessUnitFactory.BusinessUnitUsedInTest);
			StateHolderProxyHelper.ClearAndSetStateHolder(stateMock);
		}

		private IDictionary<IPerson, PersonAssignment> createShiftTradeDetails (IPerson personTo, IPerson personFrom,
			ShiftTradeRequest shiftTradeRequest, DateOnly dateOnly)
		{
			var personToAssignment = addPersonAssignment (personTo, "sdfTo", "shiftCategory", Color.PaleVioletRed, dateOnly);
			var personFromAssignment = addPersonAssignment (personFrom, "sdfFrom", "shiftCategoryFrom", Color.AliceBlue, dateOnly);

			var schedule = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod (new[] {personTo, personFrom},
				new ScheduleDictionaryLoadOptions (false, false),
				new DateOnlyPeriod (dateOnly, dateOnly.AddDays (1)), Scenario.Current());

			setShiftTradeSwapDetailsToAndFrom (shiftTradeRequest, schedule, personTo, personFrom);

			return new Dictionary<IPerson, PersonAssignment>
			{
				{personTo, personToAssignment},
				{personFrom, personFromAssignment},
			};

		}

		private ShiftTradeRequestListViewModel createRequestListViewModel(DateOnly startDate, DateOnly endDate)
		{
			var input = new AllRequestsFormData
			{
				StartDate = startDate,
				EndDate = endDate,
				AgentSearchTerm = new Dictionary<PersonFinderField, string>(),
				SelectedTeamIds = new []{Guid.NewGuid()}
			};
			return ShiftTradeRequestViewModelFactory.CreateRequestListViewModel (input);
		}

	}
}