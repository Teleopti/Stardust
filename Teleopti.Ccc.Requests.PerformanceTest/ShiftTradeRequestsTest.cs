using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Requests.PerformanceTest
{
	[ShiftTradeRequestPerformanceTest]
	public class ShiftTradeRequestsTest
	{
		public AsSystem AsSystem;

		public IDataSourceScope DataSource;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public ShiftTradeRequestHandler ShiftTradeRequestHandler;
		public WithUnitOfWork WithUnitOfWork;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IGlobalSettingDataRepository GlobalSettingDataRepository;
		public IReadModelScheduleProjectionReadOnlyValidator ReadModelScheduleProjectionUpdater;
		public IReadModelFixer ReadModelFixer;
		public MutableNow Now;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;

		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IActivityRepository ActivityRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IRuleSetBagRepository RuleSetBagRepository;
		public IWorkloadRepository WorkloadRepository;
		public IWorkShiftRuleSetRepository WorkShiftRuleSetRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;

		private const string tenantName = "Teleopti WFM";
		private LicenseSchema schema;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;

		[Test]
		public void ShouldBePerformantWhenValidatingAndReferringShiftTradeRequests()
		{
			schema = LicenseDataFactory.CreateDefaultActiveLicenseSchemaForTest();
			LicenseSchema.SetActiveLicenseSchema(tenantName, schema);
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			using (DataSource.OnThisThreadUse(tenantName))
			{
				var businessId = WithUnitOfWork.Get(() => BusinessUnitRepository.LoadAll().First()).Id.Value;
				AsSystem.Logon(tenantName, businessId);
			}

			var personRequests = new List<IPersonRequest>();

			WithUnitOfWork.Do(setupPersons);

			WithUnitOfWork.Do(() =>
			{
				setupPersonRequests(personRequests);
			});

			WithUnitOfWork.Do(() =>
			{
				var sw = new Stopwatch();
				sw.Start();

				personRequests.ForEach(shiftTradeRequest =>
			   {
				   ShiftTradeRequestHandler.Handle(new AcceptShiftTradeEvent
				   {
					   PersonRequestId = shiftTradeRequest.Id.GetValueOrDefault()
				   });
			   });

				sw.Stop();

				Console.WriteLine("Processing and Validation of Shift Trade Requests took " + sw.Elapsed);
			});

			var expectedResults = new Dictionary<Guid, PersonRequestStatus>();
			var actualResults = new Dictionary<Guid, PersonRequestStatus>();

			expectedResults.Add(personRequests[0].Id.Value, PersonRequestStatus.Pending);
			expectedResults.Add(personRequests[1].Id.Value, PersonRequestStatus.Pending);
			expectedResults.Add(personRequests[2].Id.Value, PersonRequestStatus.Pending);
			expectedResults.Add(personRequests[3].Id.Value, PersonRequestStatus.Pending);
			expectedResults.Add(personRequests[4].Id.Value, PersonRequestStatus.Pending);
			expectedResults.Add(personRequests[5].Id.Value, PersonRequestStatus.Pending);
			expectedResults.Add(personRequests[6].Id.Value, PersonRequestStatus.Pending);

			WithUnitOfWork.Do(() =>
		   {
			   foreach (var req in personRequests)
			   {
				   var request = PersonRequestRepository.Get(req.Id.Value);
				   var requestStatus = PersonRequestStatus.New;

				   if (request.IsPending)
					   requestStatus = PersonRequestStatus.Pending;
				   else if (request.IsDenied)
					   requestStatus = PersonRequestStatus.Denied;
				   else if (request.IsApproved)
					   requestStatus = PersonRequestStatus.Approved;

				   actualResults.Add(request.Id.Value, requestStatus);
			   }
		   });

			CollectionAssert.AreEquivalent(expectedResults, actualResults);
		}

		private void setupPersons()
		{
			var dateOnly = Now.UtcDateTime().ToDateOnly();

			var team = new Team { Site = new Site("_") };
			team.SetDescription(new Description("_"));

			SiteRepository.Add(team.Site);
			TeamRepository.Add(team);

			var contract = new Contract("_");
			ContractRepository.Add(contract);
			var partTimePercentage = new PartTimePercentage("part");
			PartTimePercentageRepository.Add(partTimePercentage);

			var contractSchedule = new ContractSchedule("_");
			ContractScheduleRepository.Add(contractSchedule);

			var workflowcontrolset = new WorkflowControlSet("desc");
			WorkflowControlSetRepository.Add(workflowcontrolset);

			var activity = new Activity("_");
			ActivityRepository.Add(activity);

			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(1, 1, 1, 1, 1),
				new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCategory))
			{
				Description = new Description("_")
			};

			ShiftCategoryRepository.Add(shiftCategory);

			var skill = new Skill().IsOpen().For(activity);
			skill.SkillType.Description = new Description("_");
			skill.DefaultResolution = 60;
			SkillTypeRepository.Add(skill.SkillType);
			SkillRepository.Add(skill);

			WorkloadRepository.AddRange(skill.WorkloadCollection);

			var ruleSetBag = new RuleSetBag(ruleSet) { Description = new Description("_") };
			RuleSetBagRepository.Add(ruleSetBag);
			WorkShiftRuleSetRepository.Add(ruleSetBag.RuleSetCollection.Single());

			var agents = new List<IPerson>();
			for (var i = 0; i < 14; i++)
			{
				var agent = new Person()
					.WithPersonPeriod(dateOnly, ruleSetBag, contract, contractSchedule, partTimePercentage, team, skill)
					.WithName(new Name("agentForShiftTradeRequest", i.ToString()))
					.InTimeZone(TimeZoneInfo.Utc);
				agent.WorkflowControlSet = workflowcontrolset;
				agents.Add(agent);
			}
			PersonRepository.AddRange(agents);
		}

		private void setupPersonRequests(ICollection<IPersonRequest> personRequests)
		{
			var dateOnly = Now.UtcDateTime().ToDateOnly();
			var personIds = PersonRepository.LoadAll().Where(a => a.Name.ToString().StartsWith("agentForShiftTradeRequest")).Take(14).Select(a => a.Id.Value).ToArray();
			var people = PersonRepository.FindPeople(personIds).ToList();
			for (var count = 0; count < personIds.Length; count = count + 2)
			{
				var personFrom = getPerson(people, personIds[count]);
				var personTo = getPerson(people, personIds[count + 1]);

				personFrom.MyTeam(dateOnly).Site.MaxSeats = 80;
				personTo.MyTeam(dateOnly).Site.MaxSeats = 80;

				ensureSkillsMatch(personFrom, personTo, dateOnly);

				personRequests.Add(createShiftTradeRequest(personFrom, personTo, dateOnly, dateOnly));

				var readModels = ReadModelScheduleProjectionUpdater.Build(personTo, dateOnly).ToList();

				ReadModelFixer.FixScheduleProjectionReadOnly(new ReadModelData
				{
					Date = dateOnly,
					PersonId = personTo.Id.Value,
					ScheduleProjectionReadOnly = readModels
				});

				var readModelsFrom = ReadModelScheduleProjectionUpdater.Build(personFrom, dateOnly).ToList();

				ReadModelFixer.FixScheduleProjectionReadOnly(new ReadModelData
				{
					Date = dateOnly,
					PersonId = personFrom.Id.Value,
					ScheduleProjectionReadOnly = readModelsFrom
				});
			}
		}

		private static void ensureSkillsMatch(IPerson personFrom, IPerson personTo, DateOnly dateOnly)
		{
			personTo.WorkflowControlSet.MustMatchSkills.ForEach(skill =>
			{
				personFrom.AddSkill(skill, dateOnly);
				personTo.AddSkill(skill, dateOnly);
			});

			personFrom.WorkflowControlSet.MustMatchSkills.ForEach(skill =>
		   {
			   personFrom.AddSkill(skill, dateOnly);
			   personTo.AddSkill(skill, dateOnly);
		   });
		}

		private IPerson getPerson(IEnumerable<IPerson> people, Guid id)
		{
			return people.Single(person => person.Id == id);
		}

		private IPersonRequest createShiftTradeRequest(IPerson personFrom, IPerson personTo, DateOnly shiftTradeDateFrom,
			DateOnly shiftTradeDateTo)
		{
			IPersonRequest request = new PersonRequest(personFrom);
			IShiftTradeRequest shiftTradeRequest = new ShiftTradeRequest(
				new List<IShiftTradeSwapDetail>
				{
					new ShiftTradeSwapDetail (personFrom, personTo, shiftTradeDateFrom, shiftTradeDateTo)
				});
			foreach (var shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
			{
				shiftTradeSwapDetail.ChecksumFrom = 50;
				shiftTradeSwapDetail.ChecksumTo = 57;
			}

			request.Request = shiftTradeRequest;
			PersonRequestRepository.Add(request);
			return request;
		}
	}
}
