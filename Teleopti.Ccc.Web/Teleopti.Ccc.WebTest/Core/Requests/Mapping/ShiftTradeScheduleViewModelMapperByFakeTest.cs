using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture, MyTimeWebTest]
	public class ShiftTradeScheduleViewModelMapperByFakeTest
	{
		private readonly IShiftTradeScheduleViewModelMapper _mapper;
		private readonly IPersonRepository _personRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public ShiftTradeScheduleViewModelMapperByFakeTest()
		{
		}

		public ShiftTradeScheduleViewModelMapperByFakeTest(IShiftTradeScheduleViewModelMapper mapper, IPersonRepository personRepository, IBusinessUnitRepository businessUnitRepository, ITeamRepository teamRepository, IPersonAssignmentRepository personAssignmentRepository, ILoggedOnUser loggedOnUser)
		{
			_mapper = mapper;
			_personRepository = personRepository;
			_businessUnitRepository = businessUnitRepository;
			_teamRepository = teamRepository;
			_personAssignmentRepository = personAssignmentRepository;
			_loggedOnUser = loggedOnUser;
		}

		protected void SetUp()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			_businessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "2");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublished", "3");
			var person4 = PersonFactory.CreatePersonWithGuid("NoShiftTrade", "4");

			ITeam team = TeamFactory.CreateTeamWithId("team1");
			_teamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person4.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			_personRepository.Add(person1);
			_personRepository.Add(person2);
			_personRepository.Add(person3);
			_personRepository.Add(person4);
			_personRepository.Add(_loggedOnUser.CurrentUser());


			var person1Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 19));
			person1Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 8, 2015, 5, 19, 14));
			var person1Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 21));
			person1Assignment_2.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 21, 10, 2015, 5, 21, 16));


			var person2Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2015, 5, 19));
			person2Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 11, 2015, 5, 19, 20));
			var person2Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2015, 5, 23));
			person2Assignment_2.SetDayOff(new DayOffTemplate(new Description("Day Off", "DO")));

			var person3Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person3, new DateOnly(2015, 5, 19));
			person3Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 12, 2015, 5, 19, 15));
			
			var person4Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person4, new DateOnly(2015, 5, 19));
			person4Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 12, 2015, 5, 19, 15));

			_personAssignmentRepository.Add(person1Assignment_1);
			_personAssignmentRepository.Add(person1Assignment_2);
			_personAssignmentRepository.Add(person2Assignment_1);
			_personAssignmentRepository.Add(person2Assignment_2);
			_personAssignmentRepository.Add(person3Assignment);

		}

		[Test]
		public void MapperShouldNotBeNull()
		{
			_mapper.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotSeeNoShiftTradeAgentsAndUnpublishedSchedules()
		{
			SetUp();
			var data = new ShiftTradeScheduleViewModelData
			{
				Paging = new Paging { Skip = 0, Take = 10,},
				ShiftTradeDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(t => t.Id.Value).ToList(),
				SearchNameText = "",
			};

			var result = _mapper.Map(data);

			result.PossibleTradeSchedules.Count().Should().Be(2);
			result.PossibleTradeSchedules.First().Name.Should().Be("1 person");
		}

		
		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithNameSearch()
		{
			SetUp();

			var result = _mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = "1"
			});

			result.PossibleTradeSchedules.Single().Name.Should().Be("1 person");
		}

		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithDate()
		{
			SetUp();

			var person1 = _personRepository.LoadAll().First(p => p.Name.LastName == "1");

			var result = _mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2015, 5, 21),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			var agentSchedule = result.PossibleTradeSchedules.Single(s => s.PersonId == person1.Id.Value);
			agentSchedule.MinStart.Should().Be.EqualTo(new DateTime(2015, 5, 21, 10, 0, 0));
		}

		[Test]
		public void TeamScheduleControllerShouldReturnCorrectTimeLine()
		{
			SetUp();

			var result = _mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				SearchNameText = ""
			});

			result.TimeLineHours.Max(t => t.EndTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 20, 15, 0));
			result.TimeLineHours.Min(t => t.StartTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 7, 45, 0));
		}



		[Test]
		public void ShouldReturnCorrectAgentSchedulesWithTimeFilter()
		{
			SetUp();
			var result = _mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
				Paging = new Paging { Take = 20, Skip = 0 },
				TimeFilter = new TimeFilterInfo
				{
					StartTimes = new List<DateTimePeriod> { new DateTimePeriod(2015, 5, 19, 7, 2015, 5, 19, 9) },
					EndTimes = new List<DateTimePeriod> { new DateTimePeriod(2015, 5, 19, 13, 2015, 5, 19, 15) },
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
			SetUp();

			var result = _mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2015, 5, 23),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
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
			SetUp();

			var result = _mapper.Map(new ShiftTradeScheduleViewModelData
			{
				ShiftTradeDate = new DateOnly(2015, 5, 23),
				TeamIdList = _teamRepository.LoadAll().Select(x => x.Id.Value).ToList(),
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


	}
}
