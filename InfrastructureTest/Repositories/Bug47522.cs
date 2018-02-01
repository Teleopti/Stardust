using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
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
		public IPersonRequestRepository PersonRequestRepository;
		public IScheduleDifferenceSaver Saver;

		private IScheduleDictionary _scheduleDictionary;

		private IPersonRequest createPersonShiftTradeRequest(IPerson personFrom, IPerson personTo, DateOnly requestDate)
		{
			var request = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, requestDate);
			var shiftTradeRequest = request.Request as IShiftTradeRequest;
			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, null);
			foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
			{
				shiftTradeSwapDetail.SchedulePartFrom = _scheduleDictionary[personFrom].ScheduledDay(requestDate);
				shiftTradeSwapDetail.SchedulePartTo = _scheduleDictionary[personTo].ScheduledDay(requestDate);
				shiftTradeSwapDetail.ChecksumFrom = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartFrom).CalculateChecksum();
				shiftTradeSwapDetail.ChecksumTo = new ShiftTradeChecksumCalculator(shiftTradeSwapDetail.SchedulePartTo).CalculateChecksum();
			}
			return request;
		}

		[Test, Ignore("for hongli...")]
		public void TryWritingTestReflectingHongliOpinion()
		{
			var activity = new Activity(".");
			var person = PersonFactory.CreatePerson("test");
			var personTo = PersonFactory.CreatePerson("to");
			var scenario = ScenarioFactory.CreateScenario("testScenario", true, false);
			var personAssignment = new PersonAssignment(person, scenario, new DateOnly(2018, 1, 31));
			WithUnitOfWork.Do(() =>
			{
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				PersonRepository.Add(person);
				PersonRepository.Add(personTo);
				PersonAssignmentRepository.Add(personAssignment);
			});
			using (IUnitOfWork uowSetup = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var repositoryFactorySetup = new RepositoryFactory();
				var scheduleRepSetup = new ScheduleStorage(new ThisUnitOfWork(uowSetup), repositoryFactorySetup, new PersistableScheduleDataPermissionChecker(), new ScheduleStorageRepositoryWrapper(repositoryFactorySetup, new ThisUnitOfWork(uowSetup)));
				_scheduleDictionary = scheduleRepSetup.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person, personTo }, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(new DateOnly(2018, 1, 31), new DateOnly(2018, 1, 31)), scenario);
				var personRequest = createPersonShiftTradeRequest(person, personTo, new DateOnly(2018, 1, 31));
				PersonRequestRepository.Add(personRequest);
				uowSetup.PersistAll();
			}


			var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
			

			var repositoryFactoryClient1 = new RepositoryFactory();
			var scheduleRepClient1 = new ScheduleStorage(new ThisUnitOfWork(uow), repositoryFactoryClient1, new PersistableScheduleDataPermissionChecker(), new ScheduleStorageRepositoryWrapper(repositoryFactoryClient1, new ThisUnitOfWork(uow)));
			var dicClient1 = scheduleRepClient1.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { person }, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(new DateOnly(2018, 1, 31), new DateOnly(2018, 1, 31)), scenario);

			var personRequestClient1 = new PersonRequestRepository(new ThisUnitOfWork(uow)).LoadAll().Single();

			var checker = new ShiftTradeRequestStatusCheckerWithSchedule(dicClient1, new PersonRequestCheckAuthorization());

			var checkResult = (personRequestClient1.Request as IShiftTradeRequest).GetShiftTradeStatus(checker);

			var other = new Thread(() =>
			{
				//client 2
				var uow2 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork();
				var repositoryFactory = new RepositoryFactory();
				var scheduleRep = new ScheduleStorage(new ThisUnitOfWork(uow2), repositoryFactory, new PersistableScheduleDataPermissionChecker(), new ScheduleStorageRepositoryWrapper(repositoryFactory, new ThisUnitOfWork(uow2)));
				var dic = scheduleRep.FindSchedulesForPersons(scenario, new[] { person }, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(new DateOnly(2018, 1, 31), new DateOnly(2018, 1, 31)).ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone()), new[] { person }, false);
				var scheduleRange = dic[person];
				var scheduleDay = scheduleRange.ScheduledDay(new DateOnly(2018, 1, 31));
				var personAss = scheduleDay.PersonAssignment();

				personAss.AddActivity(activity, new DateTimePeriod(2018, 1, 31, 11, 2018, 1, 31, 17));
				dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
				Saver.SaveChanges(scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()), (ScheduleRange)scheduleRange);
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