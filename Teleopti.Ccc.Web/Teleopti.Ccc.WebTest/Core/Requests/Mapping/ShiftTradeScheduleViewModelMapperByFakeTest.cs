using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Ccc.WebTest.Core.Requests.ViewModelFactory;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture, MyTimeWebTest]
	public class ShiftTradeScheduleViewModelMapperByFakeTest : IIsolateSystem
	{
		public IShiftTradeScheduleViewModelMapper Mapper;
		public IPersonRepository PersonRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ITeamRepository TeamRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePeopleForShiftTradeFinder PeopleForShiftTradeFinder;
		public FakeAbsenceRepository AbsenceRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakePeopleForShiftTradeFinder>().For<IPeopleForShiftTradeFinder>();
			isolate.UseTestDouble<FakeAbsenceRepository>().For<IAbsenceRepository>();
		}

		protected void setUpData()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "2");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublished", "3");
			var person4 = PersonFactory.CreatePersonWithGuid("NoShiftTrade", "4");
			var site = SiteFactory.CreateSimpleSite("s");
			ITeam team = TeamFactory.CreateTeamWithId("team1");
			team.Site = site;
			TeamRepository.Add(team);

			person1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person1.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			person1.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			person2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person2.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			person2.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			person3.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person3.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			person3.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);
			person4.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person4.WorkflowControlSet = new WorkflowControlSet("valid workflow");
			person4.WorkflowControlSet.ShiftTradeOpenPeriodDaysForward = new MinMax<int>(0, 99);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2091, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2091, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2091, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person4.AddPersonPeriod(new PersonPeriod(new DateOnly(2091, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			addPerson(person1, team);
			addPerson(person2, team);
			addPerson(person3, team);
			addPerson(person4, team);

			var currentUser = LoggedOnUser.CurrentUser();
			currentUser.AddPersonPeriod(new PersonPeriod(new DateOnly(2091, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			addPerson(currentUser, team);


			var person1Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2095, 5, 19));
			person1Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 19, 8, 2095, 5, 19, 14));
			var person1Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2095, 5, 21));
			person1Assignment_2.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 21, 10, 2095, 5, 21, 16));


			var person2Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2095, 5, 19));
			person2Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 19, 11, 2095, 5, 19, 20));
			var person2Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2095, 5, 23));
			person2Assignment_2.SetDayOff(new DayOffTemplate(new Description("Day Off", "DO")));

			var person3Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person3, new DateOnly(2095, 5, 19));
			person3Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 19, 12, 2095, 5, 19, 15));
			
			var person4Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person4, new DateOnly(2095, 5, 19));
			person4Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 19, 12, 2095, 5, 19, 15));

			var currentUserAssignment1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(currentUser, new DateOnly(2095, 5, 19));
			currentUserAssignment1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 19, 8, 2095, 5, 19, 14));

			PersonAssignmentRepository.Add(person1Assignment_1);
			PersonAssignmentRepository.Add(person1Assignment_2);
			PersonAssignmentRepository.Add(person2Assignment_1);
			PersonAssignmentRepository.Add(person2Assignment_2);
			PersonAssignmentRepository.Add(person3Assignment);
			PersonAssignmentRepository.Add(currentUserAssignment1);
		}

		[Test]
		public void MapperShouldNotBeNull()
		{
			Mapper.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotSeeNoShiftTradeAgentsAndUnpublishedSchedules()
		{
			setUpData();
			var data = new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 10,},
				ShiftTradeDate = new DateOnly(2095, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(t => t.Id.Value).ToList(),
				SearchNameText = "",
			};

			var result = Mapper.Map(data);

			result.PossibleTradeSchedules.Count().Should().Be(2);
			result.PossibleTradeSchedules.First().Name.Should().Be("person 1");
		}

		
		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithNameSearch()
		{
			setUpData();

			var result = Mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2095, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = "1"
			});

			result.PossibleTradeSchedules.Single().Name.Should().Be("person 1");
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithDate()
		{
			setUpData();

			var currentUserAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(LoggedOnUser.CurrentUser(), new DateOnly(2095, 5, 21));
			currentUserAssignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 21, 10, 2095, 5, 21, 16));
			PersonAssignmentRepository.Add(currentUserAssignment);

			var person1 = PersonRepository.LoadAll().First(p => p.Name.LastName == "1");

			var result = Mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2095, 5, 21),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.PossibleTradeSchedules.Single(s => s.PersonId == person1.Id.Value);
			agentSchedule.MinStart.Should().Be.EqualTo(new DateTime(2095, 5, 21, 10, 0, 0));
		}

		[Test]
		public void TeamScheduleControllerShouldReturnCorrectTimeLine()
		{
			setUpData();

			var result = Mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2095, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.TimeLineHours.Max(t => t.EndTime).Should().Be.EqualTo(new DateTime(2095, 5, 19, 20, 15, 0));
			result.TimeLineHours.Min(t => t.StartTime).Should().Be.EqualTo(new DateTime(2095, 5, 19, 7, 45, 0));
		}



		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithTimeFilter()
		{
			setUpData();
			var result = Mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2095, 5, 19),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod> { new DateTimePeriod(2095, 5, 19, 7, 2095, 5, 19, 9) },
					EndTimes = new List<DateTimePeriod> { new DateTimePeriod(2095, 5, 19, 13, 2095, 5, 19, 15) },
					IsDayOff = false,
					IsWorkingDay = true,
					IsEmptyDay = false
				},
				SearchNameText = ""
			});

			result.PossibleTradeSchedules.Should().Have.Count.EqualTo(1);
		}


		[Test]
		public void ShouldSeeDayOffAgentScheduleWhenDayOffFilterEnabled()
		{
			setUpData();

			var currentUserAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(LoggedOnUser.CurrentUser(), new DateOnly(2095, 5, 23));
			currentUserAssignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 23, 10, 2095, 5, 23, 16));
			PersonAssignmentRepository.Add(currentUserAssignment);

			var result = Mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2095, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod>(),
					EndTimes = new List<DateTimePeriod>(),
					IsDayOff = true,
					IsWorkingDay = false,
					IsEmptyDay = false
				},
				SearchNameText = ""
			});

			result.PossibleTradeSchedules.Should().Have.Count.EqualTo(1);			
		}


		[Test]
		public void ShouldSeeCorrectAgentSchedulesWhenBothDayOffAndEmptyDayFilterEnabled()
		{
			setUpData();

			var currentUserAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(LoggedOnUser.CurrentUser(), new DateOnly(2095, 5, 23));
			currentUserAssignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2095, 5, 23, 10, 2095, 5, 23, 16));
			PersonAssignmentRepository.Add(currentUserAssignment);

			var result = Mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2095, 5, 23),
				TeamIdList = TeamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod>(),
					EndTimes = new List<DateTimePeriod>(),
					IsDayOff = true,
					IsWorkingDay = false,
					IsEmptyDay = true
				},
				SearchNameText = ""
			});

			result.PossibleTradeSchedules.Should().Have.Count.EqualTo(2);
		}

		private void addPerson(IPerson personWithAbsenceOnContractDayOff, ITeam team)
		{
			PersonRepository.Add(personWithAbsenceOnContractDayOff);
			PeopleForShiftTradeFinder.Has(new PersonAuthorization
			{
				PersonId = personWithAbsenceOnContractDayOff.Id.Value,
				TeamId = team.Id.Value
			});
		}
	}
}
