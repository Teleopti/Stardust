using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[NoDefaultData]
	public class MultiAbsenceRequestsHandlerBug45331Test : IIsolateSystem
	{
		public IPersonRequestRepository PersonRequestRepository;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public MultiAbsenceRequestsHandlerRobustToggleOff Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepositoryThrowsLockException PersonAssignmentRepository;
		public MutableNow Now;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeSkillDayRepository SkillDayRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<MultiAbsenceRequestsHandlerRobustToggleOff>().For<IHandleEvent<NewMultiAbsenceRequestsCreatedEvent>>();
			isolate.UseTestDouble<FakeCommandDispatcher>().For<ICommandDispatcher>();
			isolate.UseTestDouble<FakePersonAssignmentRepositoryThrowsLockException>().For<IPersonAssignmentRepository>();
			isolate.UseTestDouble<FakeASMScheduleChangeTimeRepository>().For<IASMScheduleChangeTimeRepository>();
		}

		[Test]
		public void KeepTheRequestPendingIfThereIsDeadlockWhileLoading()
		{
			var dateTimeNow = new DateTime(2016, 12, 01, 8, 0, 0, 2);
			var truncatedSent = new DateTime(2016, 12, 01, 8, 0, 0, 0);
			Now.Is(dateTimeNow);
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scenarioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			wfcs.AbsenceRequestWaitlistEnabled = true;
			WorkflowControlSetRepository.Add(wfcs);
			var firstDay = new DateOnly(2016, 12, 01);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0));

			var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			person.WorkflowControlSet = wfcs;
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			addToQueue(personRequest, RequestValidatorsFlag.None);

			QueuedAbsenceRequestRepository.Add(
				new QueuedAbsenceRequest
				{
					PersonRequest = Guid.Empty,
					Sent = truncatedSent,  //truncated by DB
					StartDateTime = personRequest.Request.Period.StartDateTime,
					EndDateTime = personRequest.Request.Period.EndDateTime
				});

			var requestList = new List<Guid> { personRequest.Id.GetValueOrDefault() };
			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent { PersonRequestIds = requestList, Sent = Now.UtcDateTime() });
			Assert.IsTrue(personRequest.IsPending);
		}

		[Test]
		public void ResetSentColumnIfThereIsDeadlockWhileLoading()
		{
			var dateTimeNow = new DateTime(2016, 12, 01, 8, 0, 0, 2);
			var truncatedSent = new DateTime(2016, 12, 01, 8, 0, 0, 0);
			Now.Is(dateTimeNow);
			var bu = new Domain.Common.BusinessUnit("bu").WithId();
			BusinessUnitRepository.Has(bu);
			var scenario = new Scenario("scenarioName").WithId();
			scenario.DefaultScenario = true;
			scenario.SetBusinessUnit(bu);
			ScenarioRepository.Has(scenario);

			var absence = AbsenceFactory.CreateAbsence("Holiday").WithId();

			var wfcs = new WorkflowControlSet().WithId();
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new AbsenceRequestNoneValidator(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				Period = new DateOnlyPeriod(2016, 11, 1, 2016, 12, 30),
				OpenForRequestsPeriod = new DateOnlyPeriod(2016, 11, 1, 2059, 12, 30),
				AbsenceRequestProcess = new GrantAbsenceRequest()
			});
			wfcs.AbsenceRequestWaitlistEnabled = true;
			WorkflowControlSetRepository.Add(wfcs);
			var firstDay = new DateOnly(2016, 12, 01);
			var activity = ActivityRepository.Has("activityName");
			var skill = SkillRepository.Has("skillName", activity);

			SkillDayRepository.Has(skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 0));

			var person = PersonRepository.Has(new Contract("contract"), new ContractSchedule("_"), new PartTimePercentage("_"), new Team { Site = new Site("site") }, new SchedulePeriod(firstDay, SchedulePeriodType.Week, 1), skill);
			person.WorkflowControlSet = wfcs;
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, new DateTimePeriod(2016, 12, 1, 10, 2016, 12, 2, 20));
			PersonAssignmentRepository.Has(assignment);

			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, new DateTimePeriod(2016, 12, 1, 12, 2016, 12, 1, 13))).WithId();
			personRequest.Pending();
			PersonRequestRepository.Add(personRequest);

			QueuedAbsenceRequestRepository.Add(
				new QueuedAbsenceRequest
				{
					PersonRequest = Guid.Empty,
					Sent = truncatedSent,  //truncated by DB
					StartDateTime = personRequest.Request.Period.StartDateTime,
					EndDateTime = personRequest.Request.Period.EndDateTime
				});

			var requestList = new List<Guid> { personRequest.Id.GetValueOrDefault() };
			Target.Handle(new NewMultiAbsenceRequestsCreatedEvent { PersonRequestIds = requestList, Sent = truncatedSent });
			
			QueuedAbsenceRequestRepository.LoadAll().First().Sent.HasValue.Should().Be.False();
		}


		private void addToQueue(IPersonRequest personRequest, RequestValidatorsFlag requestValidatorsFlag)
		{
			QueuedAbsenceRequestRepository.Add(new QueuedAbsenceRequest
			{
				PersonRequest = personRequest.Id.GetValueOrDefault(),
				MandatoryValidators = requestValidatorsFlag,
				StartDateTime = personRequest.Request.Period.StartDateTime,
				EndDateTime = personRequest.Request.Period.EndDateTime,
				Sent = Now.UtcDateTime()
			});
		}
	}
	public class FakePersonAssignmentRepositoryThrowsLockException : IPersonAssignmentRepository
	{
		private readonly IFakeStorage _storage;

		public FakePersonAssignmentRepositoryThrowsLockException(IFakeStorage storage)
		{
			_storage = storage;
		}

		public void Has(IPersonAssignment personAssignment)
		{
			Add(personAssignment);
		}

		public void Has(IPerson agent, IScenario scenario, IActivity activity, IShiftCategory shiftCategory, DateOnlyPeriod period, TimePeriod timePeriod, string source = null)
		{
			foreach (var date in period.DayCollection())
			{
				var ass = new PersonAssignment(agent, scenario, date);
				ass.AddActivity(activity, timePeriod);
				ass.SetShiftCategory(shiftCategory);
				ass.Source = source;
				Add(ass);
			}
		}

		public void Has(IPerson agent, IScenario scenario, IActivity activity, IShiftCategory shiftCategory, DateOnly date, TimePeriod timePeriod, string source = null)
		{
			Has(agent, scenario, activity, shiftCategory, new DateOnlyPeriod(date, date), timePeriod, source);
		}

		public void Has(IPerson agent, IScenario scenario, IDayOffTemplate dayOffTemplate, DateOnly date)
		{
			var ass = new PersonAssignment(agent, scenario, date);
			ass.SetDayOff(dayOffTemplate);
			Add(ass);
		}

		public void Add(IPersonAssignment entity)
		{
			_storage.Add(entity);
		}

		public void Remove(IPersonAssignment entity)
		{
			_storage.Remove(entity);
		}

		public IPersonAssignment Get(Guid id)
		{
			return _storage.Get<IPersonAssignment>(id);
		}

		public IEnumerable<IPersonAssignment> LoadAll()
		{
			return _storage.LoadAll<IPersonAssignment>().ToList();
		}

		public IPersonAssignment Load(Guid id)
		{
			throw new NotImplementedException();
		}

		public IPersonAssignment LoadAggregate(Guid id)
		{
			return _storage.LoadAll<IPersonAssignment>().First(x => x.Id == id);
		}

		public IEnumerable<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			throw SqlExceptionConstructor.CreateSqlException("Deadlock exception", 1205);
		}

		public IEnumerable<DateScenarioPersonId> FetchDatabaseVersions(DateOnlyPeriod period, IScenario scenario, IPerson person)
		{
			throw new NotImplementedException();
		}

		public DateTime GetScheduleLoadedTime()
		{
			return DateTime.UtcNow;
		}

		public IEnumerable<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario, string source)
		{
			return Find(persons, period, scenario).Where(s => s.Source == source).ToList();
		}

		public bool IsThereScheduledAgents(Guid businessUnitId, DateOnlyPeriod period)
		{
			return _storage.LoadAll<IPersonAssignment>().Any(pa => pa.Person.PersonPeriodCollection.First().Team.Site.GetOrFillWithBusinessUnit_DONTUSE().Id == businessUnitId &&
				period.Contains(pa.Date));
		}
	}
}