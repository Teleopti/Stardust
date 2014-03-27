using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class PersonAssignmentCascadeDeleteTest : DatabaseTest
	{
		/// PersonAssignment
		///   0: PersonalLayer
		///   1: MainLayer
		///   2: OvertimeLayer

		private IPersonAssignment target;

		[Test]
		public void DeleteOvertimeShouldGenerateCorrectNumberOfStatements()
		{
			target.RemoveActivity(target.OvertimeActivities().First());
			PersistAndRemoveFromUnitOfWork(target);
			Session.SessionFactory.Statistics.PrepareStatementCount
				.Should().Be.EqualTo(2); //delete overtime layer + update personassignment  (no index changes)
			Session.SessionFactory.Statistics.EntityDeleteCount
				.Should().Be.EqualTo(1); //delete overtimeshift layer
		}

		[Test]
		public void DeleteMainShouldGenerateCorrectNumberOfStatements()
		{
			target.ClearMainActivities();
			PersistAndRemoveFromUnitOfWork(target);
			Session.SessionFactory.Statistics.PrepareStatementCount
				.Should().Be.EqualTo(3); //delete mainshift layer + update personassignment + update overtime index
			Session.SessionFactory.Statistics.EntityDeleteCount
				.Should().Be.EqualTo(1); //delete mainshiftlayer
		}

		[Test]
		public void DeletePersonalShouldGenerateCorrectNumberOfStatements()
		{
			target.RemoveActivity(target.PersonalActivities().First());
			PersistAndRemoveFromUnitOfWork(target);
			Session.SessionFactory.Statistics.PrepareStatementCount
				.Should().Be.EqualTo(4); //delete overtime layer + update personassignment + update main index + update overtime index
			Session.SessionFactory.Statistics.EntityDeleteCount
				.Should().Be.EqualTo(1); //delete personalshiftlayer
		}

		[Test]
		public void DeleteAggregateShouldGenerateOneStatementForEachNonCascadeLeaf()
		{
			Session.Delete(target);
			Session.Flush();
			Session.SessionFactory.Statistics.PrepareStatementCount
				.Should().Be.EqualTo(1); //delete pers assignment, (no layers - cascading)
			Session.SessionFactory.Statistics.EntityDeleteCount
				.Should().Be.EqualTo(4); //delete pers assignment, otlayer, mslayer, pslayer
		}



		//puhh - vi har såinih***ete med beroenden...
		protected override void SetupForRepositoryTest()
		{
			var dummyCat = ShiftCategoryFactory.CreateShiftCategory("dummyCat");
			var dummyActivity = new Activity("dummy") { DisplayColor = Color.DodgerBlue };
			PersistAndRemoveFromUnitOfWork(dummyActivity);
			var dummyAgent = PersonFactory.CreatePerson("m");
			var dummyScenario = ScenarioFactory.CreateScenarioAggregate("Default", false);
			var dummyCategory = ShiftCategoryFactory.CreateShiftCategory("Morning");
			var definitionSet = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime);

			PersistAndRemoveFromUnitOfWork(dummyCategory);
			PersistAndRemoveFromUnitOfWork(dummyScenario);
			PersistAndRemoveFromUnitOfWork(dummyCat);
			PersistAndRemoveFromUnitOfWork(definitionSet);

			PersonFactory.AddDefinitionSetToPerson(dummyAgent, definitionSet);
			var per = dummyAgent.Period(new DateOnly(2000, 1, 1));
			var site = SiteFactory.CreateSimpleSite("df");
			PersistAndRemoveFromUnitOfWork(site);
			per.Team.Site = site;
			PersistAndRemoveFromUnitOfWork(per.PersonContract.PartTimePercentage);
			PersistAndRemoveFromUnitOfWork(per.PersonContract.ContractSchedule);
			PersistAndRemoveFromUnitOfWork(per.PersonContract.Contract);
			PersistAndRemoveFromUnitOfWork(per.Team);
			PersistAndRemoveFromUnitOfWork(dummyAgent);

			target = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(dummyActivity,
										dummyAgent,
										new DateTimePeriod(2000, 1, 1, 2000, 1, 2),
										dummyCat,
										dummyScenario);
			target.AddOvertimeActivity(dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), definitionSet);
			Session.Save(target);
			Session.Flush();
			Session.SessionFactory.Statistics.Clear();
		}

	}
}