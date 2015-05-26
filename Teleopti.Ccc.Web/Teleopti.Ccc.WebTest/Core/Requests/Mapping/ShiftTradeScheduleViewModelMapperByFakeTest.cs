using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture, MyTimeWebTest]
	public class ShiftTradeScheduleViewModelMapperByFakeTest
	{
		private IShiftTradeScheduleViewModelMapper _mapper;

		private readonly IPersonRepository _personRepository;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonAssignmentRepository _personAssignmentRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		protected void SetUp()
		{
			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			_businessUnitRepository.Add(businessUnit);

			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("Unpublished", "2");
			var person3 = PersonFactory.CreatePersonWithGuid("NoShiftTrade", "3");

			ITeam team = TeamFactory.CreateTeamWithId("team1");
			_teamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			_personRepository.Add(person1);
			_personRepository.Add(person2);
			_personRepository.Add(person3);
			_personRepository.Add(_loggedOnUser.CurrentUser());


			var person1Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 19));
			person1Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"),
				new DateTimePeriod(2015, 5, 19, 8, 2015, 5, 19, 14));
			var person1Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 21));
			person1Assignment_2.AddActivity(ActivityFactory.CreateActivity("Phone"),
				new DateTimePeriod(2015, 5, 21, 10, 2015, 5, 21, 16));


			var person2Assignment_1 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2015, 5, 19));
			person2Assignment_1.AddActivity(ActivityFactory.CreateActivity("Phone"),
				new DateTimePeriod(2015, 5, 19, 11, 2015, 5, 19, 20));

			var person2Assignment_2 = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2015, 5, 23));
			person2Assignment_2.SetDayOff(new DayOffTemplate(new Description("Day Off", "DO")));

			var person3Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person3, new DateOnly(2015, 5, 19));
			person3Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"),
				new DateTimePeriod(2015, 5, 19, 12, 2015, 5, 19, 15));

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
				Paging = new Paging
				{
					Skip = 0,
					Take = 10,
				},
				ShiftTradeDate = new DateOnly(2015, 5, 19),
				TeamIdList = _teamRepository.LoadAll().Select(t => t.Id.Value).ToList(),
				SearchNameText = "",
			};

			var result = _mapper.Map(data);

			result.PossibleTradeSchedules.Single().Name.Should().Be("1 person");
		}
	}
}
