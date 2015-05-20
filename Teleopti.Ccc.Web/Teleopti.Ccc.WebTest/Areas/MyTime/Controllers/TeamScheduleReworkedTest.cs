using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture, MyTimeWebTest]
	class TeamScheduleReworkedTest
	{
		private TeamScheduleController target;
	
		private IPersonRepository _personRepository;
		private IPersonForScheduleFinder _personForScheduleFinder;
		private IBusinessUnitRepository _businessUnitRepository;
		private ITeamRepository _teamRepository;
		private IPersonScheduleDayReadModelFinder _personScheduleDayReadModelFinder;
		private IPersonAssignmentRepository _personAssignmentRepository;
		private ITimeLineViewModelReworkedMapper _timeLineViewModelReworkedMapper;


		protected void SetUp()
		{

			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			_businessUnitRepository.Add(businessUnit);


			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "2");


			ITeam team = TeamFactory.CreateTeamWithId("team1");			
			_teamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			_personRepository.Add(person1);
			_personRepository.Add(person2);


			var person1Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 19));
			person1Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 8, 2015, 5, 19, 18 ));
			
			var person2Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 19));
			person2Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 9, 2015, 5, 19, 19));
			_personAssignmentRepository.Add(person1Assignment);
			_personAssignmentRepository.Add(person2Assignment);
		}
	

		[Test]
		public void TargetControllerNotNull()
		{
			target.Should().Not.Be.Null();
		}

		[Test]
		public void PersonForScheduleFinderShouldWork()
		{
			SetUp();

			var team = _teamRepository.LoadAll().First();			
			var persons = _personForScheduleFinder.GetPersonFor(new DateOnly(2015, 5, 19), new[] {team.Id.Value}, "");

			persons.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void PersonScheduleDayReadModelFinderShouldWork()
		{
			SetUp();

			var personScheduleReadModels = _personScheduleDayReadModelFinder.ForPersons(new DateOnly(2015, 5, 19),
				_personRepository.LoadAll().Select( x => x.Id.Value), new Paging());

			personScheduleReadModels.Should().Have.Count.EqualTo(2);
			
		}

		[Test]
		public void TeamScheduleControllerShouldReturnCorrectTimeLine()
		{
			SetUp();

			var jsonResult = target.TeamSchedule(
				new DateOnly(2015, 5, 19),
				new ScheduleFilter() {TeamIds = String.Join(",",_teamRepository.LoadAll().Select(x => x.Id.Value).ToList() ), SearchNameText = ""},
				new Paging() { Take = 20, Skip = 0}
				);

			var result = (TeamScheduleViewModelReworked)jsonResult.Data;
			result.TimeLine.Max(t => t.EndTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 19, 15, 0));
			result.TimeLine.Min(t => t.StartTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 7, 45, 0));
		}
		


	}
}
