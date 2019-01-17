using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.InfrastructureTest.Requests
{
	[DatabaseTest]
	public class AbsenceRequestTest
	{

		public IScenarioRepository ScenarioRepository;
		public IDayOffTemplateRepository DayOffTemplateRepository;
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IRuleSetBagRepository RuleSetBagRepository;
		public IWorkShiftRuleSetRepository WorkShiftRuleSetRepository;
		public IActivityRepository ActivityRepository;
		public IShiftCategoryRepository ShiftCategoryRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IWorkloadRepository WorkloadRepository;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IAbsenceRepository AbsenceRepository;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public ISkillDayRepository SkillDayRepository;
		public MutableNow Now;

		public IAbsenceRequestProcessor AbsenceRequestProcessor;



		[Test, Ignore("Obviousy flaky and doesn't add any value")]
		public void ShouldApprovaAllRequestsWhenRunningAtTheSameTime()
		{
			Now.Is("2018-03-15 8:00");
			var scenario = new Scenario("_") {DefaultScenario = true};
			var activity = new Activity("_");
			var absence = new Absence
			{
				Description = new Description("_"),
				Requestable = true
			};
			var team = new Team
			{
				Site = new Site("_")
			};
			team.SetDescription(new Description("_"));
			var partTimePercentage = new PartTimePercentage("_");
			var contract = new Contract("_");
			var contractSchedule = new ContractSchedule("_");

			var skill = new Skill().IsOpen().For(activity);
			skill.SkillType.Description = new Description("_");
			skill.DefaultResolution = 60;
			var shiftCategory = new ShiftCategory("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(1, 1, 1, 1, 1),
				new TimePeriodWithSegment(1, 1, 1, 1, 1), shiftCategory)) {Description = new Description("_")};
			var ruleSetBag = new RuleSetBag(ruleSet) {Description = new Description("_")};
			var date = new DateOnly(2018, 3, 15);
			var wfcs = new WorkflowControlSet("_");
			wfcs.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence,
				AbsenceRequestProcess = new GrantAbsenceRequest(),
				StaffingThresholdValidator = new StaffingThresholdValidator(),
				BetweenDays = new MinMax<int>(0, 14),
				OpenForRequestsPeriod = new DateOnlyPeriod(2018, 1, 1, 2018, 12, 31)
			});

			var assignments = new List<IPersonAssignment>();
			var agents = new List<IPerson>();
			var requests = new List<IPersonRequest>();
			for (var i = 0; i < 10; i++)
			{
				var agent = new Person()
					.WithPersonPeriod(date, ruleSetBag, contract, contractSchedule, partTimePercentage, team, skill).InTimeZone(TimeZoneInfo.Utc)
					.WithSchedulePeriodOneWeek(date);
				agent.WorkflowControlSet = wfcs;
				agents.Add(agent);
				assignments.Add(new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 16))
					.ShiftCategory(shiftCategory));
				var personRequest = new PersonRequest(agent,
					new AbsenceRequest(absence, new DateTimePeriod(2018, 03, 15, 10, 2018, 03, 15, 11)));
				personRequest.Pending();
				requests.Add(personRequest);
			}
			

			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				AbsenceRepository.Add(absence);
				WorkflowControlSetRepository.Add(wfcs);
				ScenarioRepository.Add(scenario);
				DayOffTemplateRepository.Add(new DayOffTemplate(new Description("_")));
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				PartTimePercentageRepository.Add(partTimePercentage);
				ContractRepository.Add(contract);
				ContractScheduleRepository.Add(contractSchedule);
				ActivityRepository.Add(activity);
				SkillTypeRepository.Add(skill.SkillType);
				SkillRepository.Add(skill);
				WorkloadRepository.AddRange(skill.WorkloadCollection);
				ShiftCategoryRepository.Add(shiftCategory);
				WorkShiftRuleSetRepository.Add(ruleSetBag.RuleSetCollection.Single());
				RuleSetBagRepository.Add(ruleSetBag);
				PersonRepository.AddRange(agents);
				PersonAssignmentRepository.AddRange(assignments);
				PersonRequestRepository.AddRange(requests);
				SkillDayRepository.Add(skill.CreateSkillDayWithDemand(scenario, new DateOnly(2018, 03, 15), 5)); //Need 5

				uow.PersistAll();

				var skillcombinationresources = new List<SkillCombinationResource>
				{
					new SkillCombinationResource
					{
						StartDateTime = new DateTime(2018, 03, 15, 10, 0, 0),
						EndDateTime = new DateTime(2018, 03, 15, 11, 0, 0),
						Resource = 100, //have 100
						SkillCombination = new HashSet<Guid> {skill.Id.GetValueOrDefault()}
					}
				};


				SkillCombinationResourceRepository.PersistSkillCombinationResource(Now.UtcDateTime(), skillcombinationresources);
				uow.PersistAll();
			}

			Parallel.ForEach(requests, (request) =>
			{
				using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					AbsenceRequestProcessor.Process(request);
					uow.PersistAll();
				}
			});

			using (UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var reqs = PersonRequestRepository.LoadAll();
				reqs.Count(req => req.IsApproved).Should().Be.EqualTo(10);
			}
		}
	}
}
