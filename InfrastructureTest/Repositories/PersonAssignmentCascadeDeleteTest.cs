using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class PersonAssignmentCascadeDeleteTest : DatabaseTest
	{
		private IPersonAssignment target;

		[Test]
		public void DeleteOvertimeShouldGenerateOneStatement()
		{
			target.RemoveOvertimeShift(target.OvertimeShiftCollection[0]);
			PersistAndRemoveFromUnitOfWork(target);
			Session.SessionFactory.Statistics.PrepareStatementCount
				.Should().Be.EqualTo(2); //delete overtime (no layer) + update personassignment
			Session.SessionFactory.Statistics.EntityDeleteCount
				.Should().Be.EqualTo(2); //delete overtimeshift + overtimeshiftlayer
		}

		[Test]
		public void DeleteMainShiftShouldGenerateOneStatement()
		{
			target.ClearMainShiftLayers();
			PersistAndRemoveFromUnitOfWork(target);
			Session.SessionFactory.Statistics.PrepareStatementCount
				.Should().Be.EqualTo(2); //delete layer + update personassignment
			Session.SessionFactory.Statistics.EntityDeleteCount
				.Should().Be.EqualTo(1); //delete mainshiftlayer
		}

		[Test]
		public void DeletePersonalShiftShouldGenerateOneStatement()
		{
			target.RemoveLayer(target.PersonalLayers.First());
			PersistAndRemoveFromUnitOfWork(target);
			Session.SessionFactory.Statistics.PrepareStatementCount
				.Should().Be.EqualTo(2); //delete overtime (no layer) + update personassignment
			Session.SessionFactory.Statistics.EntityDeleteCount
				.Should().Be.EqualTo(1); //delete personalshiftlayer
		}

		[Test]
		public void DeleteAggregateShouldGenerateOneStatementForEachNonCascadeLeaf()
		{
			Session.Delete(target);
			Session.Flush();
			Session.SessionFactory.Statistics.PrepareStatementCount
				.Should().Be.EqualTo(3); //delete pers assignment, ot, ps (no layers)
			Session.SessionFactory.Statistics.EntityDeleteCount
				.Should().Be.EqualTo(6); //delete pers assignment, ot, ps, otlayer, mslayer, pslayer
		}



		//puhh - vi har såinih***ete med beroenden...
		protected override void SetupForRepositoryTest()
		{
			var dummyCat = ShiftCategoryFactory.CreateShiftCategory("dummyCat");
			var groupAct = new GroupingActivity("f");
			PersistAndRemoveFromUnitOfWork(groupAct);
			var dummyActivity = ActivityFactory.CreateActivity("dummy", Color.DodgerBlue);
			dummyActivity.GroupingActivity = groupAct;
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
			var otShift = new OvertimeShift();
			target.AddOvertimeShift(otShift);
			otShift.LayerCollection.Add(new OvertimeShiftActivityLayer(dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), definitionSet));
			Session.Save(target);
			Session.Flush();
			Session.SessionFactory.Statistics.Clear();
		}

	}
}