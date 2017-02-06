using System;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	[TestFixture, IoCTest]
	public class StaffingPossibilityViewModelFactoryTest : ISetup
	{
		public IStaffingPossibilityViewModelFactory StaffingPossibilityViewModelFactory;
		public FakeLoggedOnUser LoggedOnUser;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<StaffingPossibilityViewModelFactory>().For<IStaffingPossibilityViewModelFactory>();
		}

		[Test]
		public void ShouldReturnFullIntradaySiteOpenHourPeriod()
		{
			setLoggedOnUser(createPerson());
			var possibilityViewModel = StaffingPossibilityViewModelFactory.CreateAbsencePossibilityViewModel();
			possibilityViewModel.SiteOpenHourPeriod.Should()
				.Be(new TimePeriod(TimeSpan.Zero, TimeSpan.FromHours(24).Subtract(TimeSpan.FromSeconds(1))));
		}

		[Test]
		public void ShouldReturnSpecificIntradaySiteOpenHourPeriod()
		{
			setLoggedOnUser(createPersonWithSiteOpenHours(8, 17));
			var possibilityViewModel = StaffingPossibilityViewModelFactory.CreateAbsencePossibilityViewModel();
			possibilityViewModel.SiteOpenHourPeriod.Should()
				.Be(new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(17)));
		}

		[Test]
		public void ShouldReturnEmptyIntradaySiteOpenHourPeriod()
		{
			setLoggedOnUser(createPersonWithSiteOpenHours(8, 17, true));
			var possibilityViewModel = StaffingPossibilityViewModelFactory.CreateAbsencePossibilityViewModel();
			possibilityViewModel.SiteOpenHourPeriod.Should()
				.Be(new TimePeriod());
		}

		private void setLoggedOnUser(IPerson person)
		{
			LoggedOnUser.SetFakeLoggedOnUser(person);
		}

		private IPerson createPersonWithSiteOpenHours(int startHour, int endHour, bool isOpenHoursClosed = false)
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

		private IPerson createPerson()
		{
			var team = TeamFactory.CreateTeam("team", "site");
			var person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(DateOnly.Today, team);
			return person;
		}
	}
}
