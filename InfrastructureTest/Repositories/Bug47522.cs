using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
		public IScheduleDifferenceSaver Saver;
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

		[Test, Ignore("fail with the right reason")]
		public void ShouldNotThrownExceptionWhenCheckingShiftTradeStatusEvenThoughOtherThreadMakingChangeToSamePersonAssignment()
		{
			var activity = new Activity(".");
			var person = PersonFactory.CreatePerson("test");
			var personTo = PersonFactory.CreatePerson("to");
			var scenario = ScenarioFactory.CreateScenario("testScenario", true, false);
			using (var uowSetup1 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				ActivityRepository.Add(activity);
				PersonRepository.Add(person);
				PersonRepository.Add(personTo);
				PersonAssignmentRepository.Add(new PersonAssignment(person, scenario, new DateOnly(2018, 1, 31)));
				uowSetup1.PersistAll();
			}
			using (var uowSetup2 = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var scheduleDictionary = ScheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] {person, personTo},
					new ScheduleDictionaryLoadOptions(false, false),
					new DateOnlyPeriod(new DateOnly(2018, 1, 31), new DateOnly(2018, 1, 31)), scenario);
				var personRequest = createPersonShiftTradeRequest(scheduleDictionary, person, personTo, new DateOnly(2018, 1, 31));
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
						var dic = ScheduleStorage.FindSchedulesForPersons(scenario, new[] {person},
							new ScheduleDictionaryLoadOptions(false, false),
							new DateOnlyPeriod(new DateOnly(2018, 1, 31), new DateOnly(2018, 1, 31)).ToDateTimePeriod(
								person.PermissionInformation.DefaultTimeZone()), new[] {person}, false);
						var scheduleRange = dic[person];
						var scheduleDay = scheduleRange.ScheduledDay(new DateOnly(2018, 1, 31));
						var personAss = scheduleDay.PersonAssignment();
						//personAss = PersonAssignmentRepository.Get(personAss.Id.Value); don't understand why this makes test green?
						personAss.AddActivity(activity, new DateTimePeriod(2018, 1, 31, 11, 2018, 1, 31, 17));
						dic.Modify(scheduleDay, NewBusinessRuleCollection.Minimum());
						Saver.SaveChanges(
							scheduleRange.DifferenceSinceSnapshot(new DifferenceEntityCollectionService<IPersistableScheduleData>()),
							(ScheduleRange) scheduleRange);
						uow2.PersistAll();
					}
				});
				other.Start();
				other.Join();

				(personRequestClient1.Request as IShiftTradeRequest).GetShiftTradeStatus(ShiftTradeRequestStatusChecker);


				Assert.DoesNotThrow(() => { uow.PersistAll(); }); //Here an optimistic lock is thrown
			}
		}
	}
}