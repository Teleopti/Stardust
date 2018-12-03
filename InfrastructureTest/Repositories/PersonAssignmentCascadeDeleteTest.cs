using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class PersonAssignmentCascadeDeleteTest
	{
		/// PersonAssignment
		///   0: PersonalLayer
		///   1: MainLayer
		///   2: OvertimeLayer

		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;

		[Test]
		public void DeleteOvertimeShouldGenerateCorrectNumberOfStatements()
		{
			var target = createAssigment();
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var sessionFactory = uow.FetchSession().SessionFactory;
				using (sessionFactory.WithStats())
				{
					initAss(uow, target);
					target.RemoveActivity(target.OvertimeActivities().First());
					uow.Merge(target);
					uow.PersistAll();
					sessionFactory.Statistics.PrepareStatementCount
						.Should().Be.EqualTo(2); //delete overtime layer + update personassignment  (no index changes)
					sessionFactory.Statistics.EntityDeleteCount
						.Should().Be.EqualTo(1); //delete overtimeshift layer
				}
			}
		}

		[Test]
		public void DeleteMainShouldGenerateCorrectNumberOfStatements()
		{
			var target = createAssigment();
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var sessionFactory = uow.FetchSession().SessionFactory;
				using (sessionFactory.WithStats())
				{
					initAss(uow, target);
					target.ClearMainActivities();
					uow.Merge(target);
					uow.PersistAll();
					sessionFactory.Statistics.PrepareStatementCount
						.Should().Be
						.EqualTo(3); //delete mainshift layer + update personassignment + update overtime index
					sessionFactory.Statistics.EntityDeleteCount
						.Should().Be.EqualTo(1); //delete mainshiftlayer
				}
			}
		}

		[Test]
		public void DeletePersonalShouldGenerateCorrectNumberOfStatements()
		{
			var target = createAssigment();
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var sessionFactory = uow.FetchSession().SessionFactory;
				using (sessionFactory.WithStats())
				{
					initAss(uow, target);
					target.RemoveActivity(target.PersonalActivities().First());
					uow.Merge(target);
					uow.PersistAll();
					sessionFactory.Statistics.PrepareStatementCount
						.Should().Be
						.EqualTo(4); //delete overtime layer + update personassignment + update main index + update overtime index
					sessionFactory.Statistics.EntityDeleteCount
						.Should().Be.EqualTo(1); //delete personalshiftlayer	
				}
			}
		}

		[Test]
		public void DeleteAggregateShouldGenerateOneStatementForEachNonCascadeLeaf()
		{
			var target = createAssigment();
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var sessionFactory = uow.FetchSession().SessionFactory;
				using (sessionFactory.WithStats())
				{
					initAss(uow, target);
					PersonAssignmentRepository.Remove(target);
					uow.PersistAll();
					sessionFactory.Statistics.PrepareStatementCount
						.Should().Be.EqualTo(1); //delete pers assignment, (no layers - cascading)
					sessionFactory.Statistics.EntityDeleteCount
						.Should().Be.EqualTo(4); //delete pers assignment, otlayer, mslayer, pslayer
				}
			}
		}

		private static void initAss(IUnitOfWork uow, IPersonAssignment personAssignment)
		{
			uow.Reassociate(personAssignment);
			personAssignment.ShiftLayers.ToArray();
		}

		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public IScenarioRepository ScenarioRepository;
		public IMultiplicatorDefinitionSetRepository MultiplicatorDefinitionSetRepository;
		public ISiteRepository SiteRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		
		private IPersonAssignment createAssigment()
		{
			using (var uow = CurrentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var dummyCat = ShiftCategoryFactory.CreateShiftCategory("dummyCat");
				var dummyActivity = new Activity("dummy") {DisplayColor = Color.DodgerBlue};
				ActivityRepository.Add(dummyActivity);
				var dummyAgent = PersonFactory.CreatePerson("m");
				var dummyScenario = ScenarioFactory.CreateScenarioAggregate("Default", false);
				var definitionSet = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.Overtime);
				ScenarioRepository.Add(dummyScenario);
				ShiftCategoryRepository.Add(dummyCat);
				MultiplicatorDefinitionSetRepository.Add(definitionSet);
				PersonFactory.AddDefinitionSetToPerson(dummyAgent, definitionSet);
				var per = dummyAgent.Period(new DateOnly(2000, 1, 1));
				var site =new Site("_");
				per.Team.Site = site;
				SiteRepository.Add(site);
				TeamRepository.Add(per.Team);
				PartTimePercentageRepository.Add(per.PersonContract.PartTimePercentage);
				ContractRepository.Add(per.PersonContract.Contract);
				ContractScheduleRepository.Add(per.PersonContract.ContractSchedule);
				PersonRepository.Add(dummyAgent);
				var ass = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(dummyAgent,
					dummyScenario, dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), dummyCat);
				ass.AddOvertimeActivity(dummyActivity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), definitionSet);
				PersonAssignmentRepository.Add(ass);
				uow.PersistAll();
				return ass;
			}
		}
	}
}
