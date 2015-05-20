using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.WebTest.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture, MyTimeWebTest]
	public class TeamScheduleReworkedTest
	{
		public TeamScheduleController Target;
		
		public IPersonRepository PersonRepository;
		public IPersonForScheduleFinder PersonForScheduleFinder;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ITeamRepository TeamRepository;
		public IPersonScheduleDayReadModelFinder PersonScheduleDayReadModelFinder;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPermissionProvider PermissionProvider;

		protected void SetUp()
		{

			var businessUnit = BusinessUnitFactory.CreateWithId("Teleopti");
			BusinessUnitRepository.Add(businessUnit);


			var person1 = PersonFactory.CreatePersonWithGuid("person", "1");
			var person2 = PersonFactory.CreatePersonWithGuid("person", "2");
			var person3 = PersonFactory.CreatePersonWithGuid("Unpublish_person", "3");


			ITeam team = TeamFactory.CreateTeamWithId("team1");			
			TeamRepository.Add(team);

			person1.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person2.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));
			person3.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), PersonContractFactory.CreatePersonContract(), team));

			PersonRepository.Add(person1);
			PersonRepository.Add(person2);
			PersonRepository.Add(person3);


			var person1Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person1, new DateOnly(2015, 5, 19));
			person1Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 8, 2015, 5, 19, 18 ));
			
			var person2Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person2, new DateOnly(2015, 5, 19));
			person2Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 9, 2015, 5, 19, 19));

			var person3Assignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person3, new DateOnly(2015, 5, 19));
			person3Assignment.AddActivity(ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2015, 5, 19, 9, 2015, 5, 19, 19));
			PersonAssignmentRepository.Add(person1Assignment);
			PersonAssignmentRepository.Add(person2Assignment);
			PersonAssignmentRepository.Add(person3Assignment);
		}
	

		[Test]
		public void TargetControllerNotNull()
		{
			Target.Should().Not.Be.Null();
		}


		[Test]
		public void PermissionProviderShouldBeUsed()
		{
			PermissionProvider.Should().Not.Be.Null();
		}

		[Test]
		public void PersonForScheduleFinderShouldWork()
		{
			SetUp();

			var team = TeamRepository.LoadAll().First();			
			var persons = PersonForScheduleFinder.GetPersonFor(new DateOnly(2015, 5, 19), new[] {team.Id.Value}, "");

			persons.Should().Have.Count.EqualTo(3);
		}

		[Test]
		public void PersonScheduleDayReadModelFinderShouldWork()
		{
			SetUp();

			var personScheduleReadModels = PersonScheduleDayReadModelFinder.ForPersons(new DateOnly(2015, 5, 19),
				PersonRepository.LoadAll().Select( x => x.Id.Value), new Paging());

			personScheduleReadModels.Should().Have.Count.EqualTo(3);
			
		}

		[Test]
		public void TeamScheduleControllerShouldReturnCorrectTimeLine()
		{
			SetUp();

			var jsonResult = Target.TeamSchedule(
				new DateOnly(2015, 5, 19),
				new ScheduleFilter() {TeamIds = String.Join(",",TeamRepository.LoadAll().Select(x => x.Id.Value).ToList() ), SearchNameText = ""},
				new Paging() { Take = 20, Skip = 0}
				);

			var result = (TeamScheduleViewModelReworked)jsonResult.Data;
			result.TimeLine.Max(t => t.EndTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 19, 15, 0));
			result.TimeLine.Min(t => t.StartTime).Should().Be.EqualTo(new DateTime(2015, 5, 19, 7, 45, 0));
		}
	}
}
