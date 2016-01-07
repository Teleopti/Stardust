using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.Mapping
{
	[TestFixture]
	[MyTimeWebTest]
	public class ScheduleViewModelFactoryTest2
	{
		public IScheduleViewModelFactory Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public FakeScheduleDataReadScheduleRepository ScheduleData;
		public MutableNow Now;
		public FakeUserCulture Culture;
		public FakeUserTimeZone TimeZone;

		[Test]
		public void ShouldMap()
		{
			var viewModel = Target.CreateWeekViewModel(new DateOnly(Now.UtcDateTime()));

			viewModel.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldMapTimeLineEdges()
		{
			Culture.IsSwedish();
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 08:00".Utc(), "2015-03-29 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("07:45");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("17:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnDayBeforeDst()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-28 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-28 07:45".Utc(), "2015-03-28 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("08:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("18:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDay()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, new DateTimePeriod("2015-03-29 07:45".Utc(), "2015-03-29 17:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("09:30");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("19:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnFirstDstDayAndNightShift()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 28));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-03-28 00:00".Utc(), "2015-03-28 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 3, 29));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-03-29 00:00".Utc(), "2015-03-29 04:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment1, personAssignment2 });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("00:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("01:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(5).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.ElementAt(6).TimeLineDisplay.Should().Be("06:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("06:15");
		}

		[Test]
		public void ShouldMapTimeLineCorrectlyOnEndDstDayAndNightShift()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-10-25 10:00");
			var date = new DateOnly(Now.UtcDateTime());
			var phone = new Activity("p");
			var personAssignment1 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 24));
			personAssignment1.AddActivity(phone, new DateTimePeriod("2015-10-24 00:00".Utc(), "2015-10-24 04:00".Utc()));
			var personAssignment2 = new PersonAssignment(User.CurrentUser(), Scenario.Current(), new DateOnly(2015, 10, 25));
			personAssignment2.AddActivity(phone, new DateTimePeriod("2015-10-25 00:00".Utc(), "2015-10-25 04:00".Utc()));
			ScheduleData.Set(new IScheduleData[] { personAssignment1, personAssignment2 });

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.TimeLine.First().TimeLineDisplay.Should().Be("01:45");
			viewModel.TimeLine.ElementAt(1).TimeLineDisplay.Should().Be("02:00");
			viewModel.TimeLine.ElementAt(2).TimeLineDisplay.Should().Be("03:00");
			viewModel.TimeLine.ElementAt(3).TimeLineDisplay.Should().Be("04:00");
			viewModel.TimeLine.ElementAt(4).TimeLineDisplay.Should().Be("05:00");
			viewModel.TimeLine.ElementAt(5).TimeLineDisplay.Should().Be("06:00");
			viewModel.TimeLine.Last().TimeLineDisplay.Should().Be("06:15");
		}

		[Test]
		public void ShouldMapBaseUtcOffset()
		{
			Culture.IsSwedish();
			TimeZone.IsHawaii();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());

			var viewModel = Target.CreateWeekViewModel(date);
			viewModel.BaseUtcOffsetInMinutes.Should().Be(-10 * 60);
		}


		[Test]
		public void ShouldMapDaylightSavingTimeAdjustment()
		{

			Culture.IsSwedish();
			TimeZone.IsSweden();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());

			var viewModel = Target.CreateWeekViewModel(date);

			viewModel.DaylightSavingTimeAdjustment.Should().Not.Be.Null();
			viewModel.DaylightSavingTimeAdjustment.StartDateTime.Should().Be(new DateTime(2015, 3, 29, 1, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.EndDateTime.Should().Be(new DateTime(2015, 10, 25, 2, 0, 0, DateTimeKind.Utc));
			viewModel.DaylightSavingTimeAdjustment.AdjustmentOffsetInMinutes.Should().Be(60);
		}

		[Test]
		public void ShouldNotMapDaylightSavingTimeAdjustment()
		{
			Culture.IsSwedish();
			TimeZone.IsChina();
			User.CurrentUser().PermissionInformation.SetDefaultTimeZone(TimeZone.TimeZone());
			User.CurrentUser().PermissionInformation.SetCulture(Culture.GetCulture());
			Now.Is("2015-03-29 10:00");
			var date = new DateOnly(Now.UtcDateTime());

			var viewModel = Target.CreateWeekViewModel(date);
			Assert.IsNull(viewModel.DaylightSavingTimeAdjustment);

		}

		[Test]
		public void ShouldValidatePeriodSelectionStartDateAndEndDateFormatCorrectly()
		{
			Culture.IsSwedish();
			TimeZone.IsSweden();
			var date = new DateOnly(2015, 07, 06);
			var viewModel = Target.CreateWeekViewModel(date);


			Assert.AreEqual("2015-07-06", viewModel.CurrentWeekStartDate);
			Assert.AreEqual("2015-07-12", viewModel.CurrentWeekEndDate);
		}
	}

	public class FakeScheduleProjectionReadOnlyRepository : IScheduleProjectionReadOnlyRepository
	{
		public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public void ClearPeriodForPerson(DateOnlyPeriod period, Guid scenarioId, Guid personId)
		{
			throw new NotImplementedException();
		}

		public void AddProjectedLayer(DateOnly belongsToDate, Guid scenarioId, Guid personId, ProjectionChangedEventLayer layer)
		{
			throw new NotImplementedException();
		}

		public int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate)
		{
			throw new NotImplementedException();
		}

		public bool IsInitialized()
		{
			throw new NotImplementedException();
		}

		public DateTime? GetNextActivityStartTime(DateTime dateTime, Guid personId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ProjectionChangedEventLayer> ForPerson(DateOnly date, Guid personId, Guid scenarioId)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ProjectionChangedEventLayer> ForPerson(DateOnlyPeriod datePeriod, Guid personId, Guid scenarioId)
		{
			throw new NotImplementedException();
		}
	}

	public class FakeBudgetDayRepository : IBudgetDayRepository
	{
		public void Add(IBudgetDay entity)
		{
			throw new NotImplementedException();
		}

		public void Remove(IBudgetDay entity)
		{
			throw new NotImplementedException();
		}

		public IBudgetDay Get(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IBudgetDay> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IBudgetDay Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IBudgetDay> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public DateOnly FindLastDayWithStaffEmployed(IScenario scenario, IBudgetGroup budgetGroup, DateOnly lastDateToSearch)
		{
			throw new NotImplementedException();
		}

		public IList<IBudgetDay> Find(IScenario scenario, IBudgetGroup budgetGroup, DateOnlyPeriod dateOnlyPeriod)
		{
			throw new NotImplementedException();
		}
	}

	public class FakePersonRequestRepository : IPersonRequestRepository
	{
		private IList<IPersonRequest> _requestRepository = new List<IPersonRequest>();
		public void Add(IPersonRequest entity)
		{
			_requestRepository.Add(entity);
		}

		public void Remove(IPersonRequest entity)
		{
			throw new NotImplementedException();
		}

		public IPersonRequest Get(Guid id)
		{
			return _requestRepository.FirstOrDefault(x => x.Id == id);
		}

		public IList<IPersonRequest> LoadAll()
		{
			throw new NotImplementedException();
		}

		public IPersonRequest Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public long CountAllEntities()
		{
			throw new NotImplementedException();
		}

		public void AddRange(IEnumerable<IPersonRequest> entityCollection)
		{
			throw new NotImplementedException();
		}

		public IUnitOfWork UnitOfWork { get; private set; }
		public IList<IPersonRequest> Find(IPerson person, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, Paging paging)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgent(IPerson person, DateTimePeriod period)
		{
			return new List<PersonRequest>();
		}

		public IEnumerable<IPersonRequest> FindAllRequests(RequestFilter filter)
		{
			int count;
			return FindAllRequests(filter, out count, true);
		}


		public IEnumerable<IPersonRequest> FindAllRequests(RequestFilter filter, out int count, bool ignoreCount = false)
		{
			var requests = _requestRepository.Where(request => filter.Period.ContainsPart(request.Request.Period)).ToList();

			if (filter.RequestTypes != null)
			{
				requests = requests.Where(request => filter.RequestTypes.Contains(request.Request.RequestType)).ToList();
			}

			if (filter.Persons != null)
			{
				requests = requests.Where(request => filter.Persons.Contains(request.Person)).ToList();
			}
						
			count = requests.Count();
			return requests;			
		}

		public IEnumerable<IPersonRequest> FindAllRequestsForAgentByType(IPerson person, Paging paging, params RequestType[] requestTypes)
		{
			return _requestRepository.Where(request => requestTypes.Contains(request.Request.RequestType));		
		}

		public IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(IPerson person, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindPersonRequestUpdatedAfter(DateTime lastTime)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindAllRequestModifiedWithinPeriodOrPending(ICollection<IPerson> persons, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public IPersonRequest Find(Guid id)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindByStatus<T>(IPerson person, DateTime startDate, int status) where T : Request
		{
			throw new NotImplementedException();
		}

		public IPersonRequest FindPersonRequestByRequestId(Guid value)
		{
			throw new NotImplementedException();
		}

		public IList<IPersonRequest> FindAllRequestsExceptOffer(IPerson person, Paging paging)
		{
			return _requestRepository.Where(request => request.Request.RequestType != RequestType.ShiftExchangeOffer
													&& request.Person.Id.GetValueOrDefault() == person.Id.GetValueOrDefault())
									 .ToList();
		}

		public IList<IShiftExchangeOffer> FindOfferByStatus(IPerson person, DateOnly date, ShiftExchangeOfferStatus status)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IShiftExchangeOffer> FindShiftExchangeOffersForBulletin(IEnumerable<IPerson> personList, DateOnly shiftTradeDate)
		{
			throw new NotImplementedException();
		}
	}
}
