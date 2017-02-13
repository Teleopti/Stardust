using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MonthSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MonthSchedule;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	[TestFixture]
	public class ScheduleViewModelFactoryTest
	{
		[Test]
		public void ShoudCreateWeekViewModelByCallingProviderAndMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var weekScheduleDomainDataProvider = MockRepository.GenerateMock<IWeekScheduleDomainDataProvider>();
			var monthScheduleDomainDataProvider = MockRepository.GenerateMock<IMonthScheduleDomainDataProvider>();
			var target = new ScheduleViewModelFactory(mapper, weekScheduleDomainDataProvider, monthScheduleDomainDataProvider, new FakeLoggedOnUser(),null);
			var domainData = new WeekScheduleDomainData();
			var viewModel = new WeekScheduleViewModel();

			weekScheduleDomainDataProvider.Stub(x => x.Get(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<WeekScheduleDomainData, WeekScheduleViewModel>(domainData)).Return(viewModel);

			var result = target.CreateWeekViewModel(DateOnly.Today, StaffingPossiblity.None);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShoudCreateMonthViewModelByTwoStepMapping()
		{
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var weekScheduleDomainDataProvider = MockRepository.GenerateMock<IWeekScheduleDomainDataProvider>();
			var monthScheduleDomainDataProvider = MockRepository.GenerateMock<IMonthScheduleDomainDataProvider>();
			var target = new ScheduleViewModelFactory(mapper, weekScheduleDomainDataProvider, monthScheduleDomainDataProvider,
				null, null);
			var domainData = new MonthScheduleDomainData();
			var viewModel = new MonthScheduleViewModel();

			monthScheduleDomainDataProvider.Stub(x => x.Get(DateOnly.Today)).Return(domainData);
			mapper.Stub(x => x.Map<MonthScheduleDomainData, MonthScheduleViewModel>(domainData)).Return(viewModel);

			var result = target.CreateMonthViewModel(DateOnly.Today);

			result.Should().Be.SameInstanceAs(viewModel);
		}

		[Test]
		public void ShouldAdjustTimelineForOverTimeWhenSiteOpenHourPeriodContainsSchedulePeriod()
		{
			var user = createUserWithSiteOpenHours(8, 17);
			var result = createWeekViewModel(user, new TimePeriod(9, 10), StaffingPossiblity.Overtime);
			assertTimeLine(result, "08:00", "17:00");
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenSchedulePeriodContainsSiteOpenHourPeriod()
		{
			var user = createUserWithSiteOpenHours(8, 17);
			var result = createWeekViewModel(user, new TimePeriod(7, 18), StaffingPossiblity.Overtime);
			assertTimeLine(result, "07:00", "18:00");
		}

		[Test]
		public void ShouldNotAdjustTimelineBySiteOpenHourWhenAskForAbsence()
		{
			var user = createUserWithSiteOpenHours(8, 17);
			var result = createWeekViewModel(user, new TimePeriod(9, 10), StaffingPossiblity.Absence);
			assertTimeLine(result, "09:00", "10:00");
		}

		[Test]
		public void ShouldNotAdjustTimelineForOverTimeWhenNoSiteOpenHourAvailable()
		{
			var user = new FakeLoggedOnUser();
			user.SetFakeLoggedOnUser(createPerson());
			var result = createWeekViewModel(user, new TimePeriod(9, 10), StaffingPossiblity.Overtime);
			assertTimeLine(result, "09:00", "10:00");
		}

		[Test]
		public void ShouldNotGetPossibilitiesWhenNotShowingIntradaySchedule()
		{
			var user = new FakeLoggedOnUser();
			user.SetFakeLoggedOnUser(createPerson());
			var requestDate = DateOnly.Today.AddDays(10);
			var weekScheduleDomainData = new WeekScheduleDomainData
			{
				MinMaxTime = new TimePeriod(9, 10),
				Date = requestDate
			};
			var target = setupTarget(weekScheduleDomainData, user, requestDate);
			var result = target.CreateWeekViewModel(requestDate, StaffingPossiblity.Absence);
			Assert.AreEqual(0, result.Possibilities.Count());
		}

		private WeekScheduleViewModel createWeekViewModel(ILoggedOnUser loggedOnUser, TimePeriod scheduleMinMaxTime, StaffingPossiblity staffingPossiblity)
		{
			var weekScheduleDomainData = new WeekScheduleDomainData
			{
				MinMaxTime = scheduleMinMaxTime,
				Date = DateOnly.Today
			};
			var target = setupTarget(weekScheduleDomainData, loggedOnUser);
			return target.CreateWeekViewModel(DateOnly.Today, staffingPossiblity);
		}

		private static void assertTimeLine(WeekScheduleViewModel result, string expectFirstTimeLine, string expectLastTimeLine)
		{
			var firstTimeLine = result.TimeLine.First();
			var lastTimeLine = result.TimeLine.Last();
			Assert.AreEqual(expectFirstTimeLine, firstTimeLine.TimeLineDisplay);
			Assert.AreEqual(expectLastTimeLine, lastTimeLine.TimeLineDisplay);
		}

		private static ScheduleViewModelFactory setupTarget(WeekScheduleDomainData weekScheduleDomainData, ILoggedOnUser user, DateOnly? requestDate = null)
		{
			var mapper = createMapper(user);
			var weekScheduleDomainDataProvider = MockRepository.GenerateMock<IWeekScheduleDomainDataProvider>();
			var monthScheduleDomainDataProvider = MockRepository.GenerateMock<IMonthScheduleDomainDataProvider>();
			var staffingPossibilityViewModelFactory = MockRepository.GenerateMock<IStaffingPossibilityViewModelFactory>();
			weekScheduleDomainDataProvider.Stub(x => x.Get(requestDate ?? DateOnly.Today)).Return(weekScheduleDomainData);
			return new ScheduleViewModelFactory(mapper, weekScheduleDomainDataProvider, monthScheduleDomainDataProvider,
				user, staffingPossibilityViewModelFactory);
		}

		private static ILoggedOnUser createUserWithSiteOpenHours(int startHour, int endHour)
		{
			var user = new FakeLoggedOnUser();
			var person = createPersonWithSiteOpenHours(startHour, endHour);
			user.SetFakeLoggedOnUser(person);
			return user;
		}

		private static IMappingEngine createMapper(ILoggedOnUser user)
		{
			Mapper.Reset();
			Mapper.Initialize(c =>
			{
				c.AddProfile(new WeekScheduleViewModelMappingProfile(
					() => Mapper.Engine,
					() => new PeriodSelectionViewModelFactory(),
					() => null,
					() => null,
					() => null,
					() => user,
					() => new Now(),
					() => new SwedishCulture()
					));
			});

			return Mapper.Engine;
		}

		private static IPerson createPersonWithSiteOpenHours(int startHour, int endHour, bool isOpenHoursClosed = false)
		{
			var person = createPerson();
			var team = person.MyTeam(DateOnly.Today);
			var siteOpenHour = new SiteOpenHour
			{
				Parent = team.Site,
				TimePeriod = new TimePeriod(startHour, 0, endHour, 0),
				WeekDay = DateOnly.Today.DayOfWeek,
				IsClosed = isOpenHoursClosed
			};
			team.Site.AddOpenHour(siteOpenHour);
			return person;
		}

		private static IPerson createPerson()
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today, team);
			return person;
		}
	}
}