using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[DatabaseTest]
	public class Bug47522
	{
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonRepository PersonRepository;
		public IScenarioRepository ScenarioRepository;
		public IActivityRepository ActivityRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IShiftTradeRequestStatusChecker ShiftTradeRequestStatusChecker;
		public IScheduleStorage ScheduleStorage;

		private static IPersonRequest createPersonShiftTradeRequest(IScheduleDictionary scheduleDictionary, IPerson personFrom, IPerson personTo, DateOnly requestDate)
		{
			var request = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, requestDate);
			var shiftTradeRequest = request.Request as IShiftTradeRequest;
			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, null);
			foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
			{
				shiftTradeSwapDetail.SchedulePartFrom = scheduleDictionary[personFrom].ScheduledDay(requestDate);
				shiftTradeSwapDetail.SchedulePartTo = scheduleDictionary[personTo].ScheduledDay(requestDate);
				shiftTradeSwapDetail.ChecksumFrom = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartFrom).CalculateChecksum();
				shiftTradeSwapDetail.ChecksumTo = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartTo).CalculateChecksum();
			}
			return request;
		}

		[Test]
		public void ShouldNotThrownExceptionWhenCheckingShiftTradeStatusEvenThoughOtherThreadMakingChangeToSamePersonAssignment()
		{
			var activity = new Activity(".");
			var person = PersonFactory.CreatePerson("test");
			var personTo = PersonFactory.CreatePerson("to");
			var scenario = ScenarioFactory.CreateScenario("testScenario", true, false);
			var date = new DateOnly(2018, 1, 31);
			using (var uowSetup1 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				PersonRepository.Add(person);
				PersonRepository.Add(personTo);
				PersonAssignmentRepository.Add(new PersonAssignment(person, scenario, date));
				uowSetup1.PersistAll();
			}
			using (var uowSetup2 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] {person, personTo},
					new ScheduleDictionaryLoadOptions(false, false),
					date.ToDateOnlyPeriod(), scenario);
				var personRequest = createPersonShiftTradeRequest(scheduleDictionary, person, personTo, date);
				PersonRequestRepository.Add(personRequest);
				uowSetup2.PersistAll();
			}

			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var personRequestClient1 = PersonRequestRepository.LoadAll().Single();
				(personRequestClient1.Request as IShiftTradeRequest).GetShiftTradeStatus(ShiftTradeRequestStatusChecker);
				var other = new Thread(() =>
				{
					//client 2
					using (var uow2 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						var personAss = PersonAssignmentRepository.Find(new[]{person}, date.ToDateOnlyPeriod(), scenario).Single();
						personAss.AddActivity(activity, new DateTimePeriod(2018, 1, 31, 11, 2018, 1, 31, 17));
						uow2.PersistAll();
					}
				});
				other.Start();
				other.Join();

				(personRequestClient1.Request as IShiftTradeRequest).GetShiftTradeStatus(ShiftTradeRequestStatusChecker);


				Assert.DoesNotThrow(() => { uow.PersistAll(); }); //Here an optimistic lock is thrown
			}
		}
		
		[Test]
		[Ignore("This also fails")]
		public void SameProblemNotUsingPersonRequestAtAll()
		{
			var activity = new Activity(".");
			var person = PersonFactory.CreatePerson("test");
			var scenario = ScenarioFactory.CreateScenario("testScenario", true, false);
			var date = new DateOnly(2018, 1, 31);
			using (var uowSetup = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				PersonRepository.Add(person);
				PersonAssignmentRepository.Add(new PersonAssignment(person, scenario, date));
				uowSetup.PersistAll();
			}
			
			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				PersonAssignmentRepository.Find(new[]{person}, date.ToDateOnlyPeriod(), scenario);
				var other = new Thread(() =>
				{
					//client 2
					using (var uow2 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						var personAss = PersonAssignmentRepository.Find(new[]{person}, date.ToDateOnlyPeriod(), scenario).Single();
						personAss.AddActivity(activity, new DateTimePeriod(2018, 1, 31, 11, 2018, 1, 31, 17));
						uow2.PersistAll();
					}
				});
				other.Start();
				other.Join();

				PersonAssignmentRepository.Find(new[]{person}, date.ToDateOnlyPeriod(), scenario);

				Assert.DoesNotThrow(() => { uow.PersistAll(); });
			}
		}
	}
}