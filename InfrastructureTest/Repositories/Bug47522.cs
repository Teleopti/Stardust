using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[DatabaseTest]
	public class Bug47522
	{
		public WithUnitOfWork WithUnitOfWork;
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonRepository PersonRepository;
		public IScenarioRepository ScenarioRepository;
		public IActivityRepository ActivityRepository;

		[Test, Ignore("for Hongli")]
		public void TryWritingTestReflectingHongliOpinion()
		{
			var activity = new Activity(".");
			var person = PersonFactory.CreatePerson("test");
			var scenario = ScenarioFactory.CreateScenario("testScenario", true, false);
			var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2018, 1, 31));
			
			WithUnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				PersonRepository.Add(person);
			});
			WithUnitOfWork.Do(() =>
			{
				PersonAssignmentRepository.Add(personAssignment);
			});

			//client 1
			var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			var personAssRepo = new PersonAssignmentRepository(uow);
			personAssRepo.Find(new[] { person }, new DateOnlyPeriod(new DateOnly(2018, 1, 31), new DateOnly(2018, 1, 31)),
				scenario);

			var other = new Thread(() =>
			{
				//client 2
				var uow2 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
				var personAss = new PersonAssignmentRepository(new ThisUnitOfWork(uow2)).Load(personAssignment.Id.GetValueOrDefault());

				personAss.AddActivity(activity, new DateTimePeriod(2018, 1, 31, 11, 2018, 1, 31, 17));
				uow2.PersistAll();
				uow2.Dispose();
			});
			other.Start();
			other.Join();

			Assert.DoesNotThrow(() => { uow.PersistAll(); }); //Here an optimistic lock is thrown
			uow.Dispose();
		}
	}
}