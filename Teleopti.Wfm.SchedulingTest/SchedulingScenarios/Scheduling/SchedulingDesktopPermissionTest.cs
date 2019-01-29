using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	[LoggedOff]
	[DefaultData]
	[RealPermissions]
	[NonParallelizable] //would be good to get rid of but currently reading service locator on Ientity.businessunit
	public class SchedulingDesktopPermissionTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public Func<IGridlockManager> LockManager;
		public FakeDatabase Database;
		public FakePersonRepository PersonRepository;
		public ILogOnOff LogOnOff;
		public ICurrentAuthorization CurrentAuthorization;

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = false)]
		public bool ShouldScheduleWriteProtectedDayWhenHavingPermission(bool havePermissonToModifyWriteProtected)
		{
			var personId = Guid.NewGuid();
			Database.WithTenant("_").WithPerson(personId, "_");
			var applicationFunctions = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
			foreach (var applicationFunction in applicationFunctions)
			{
				if(applicationFunction.FunctionPath.Equals(DefinedRaptorApplicationFunctionPaths.All))
					continue;
					
				if (!havePermissonToModifyWriteProtected && applicationFunction.FunctionPath.Equals(DefinedRaptorApplicationFunctionPaths.ModifyWriteProtectedSchedule))
					continue;
				
				Database.WithRole(AvailableDataRangeOption.Everyone, applicationFunction.FunctionPath);
			}

			var person = PersonRepository.Load(personId);
			LogOnOff.LogOn("_", person, Database.CurrentBusinessUnitId());
			var firstDay = new DateOnly(2017, 5, 15);
			var period = new DateOnlyPeriod(firstDay, firstDay);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory().WithId()));
			var contract = new ContractWithMaximumTolerance();
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, contract, skill).WithSchedulePeriodOneWeek(firstDay);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 10, 10, 10, 10, 10, 10, 10);
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays, CurrentAuthorization);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff };
			LockManager().AddLock(agent, firstDay, LockType.WriteProtected);

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			return schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay).IsScheduled();
		}
		
		
		[TestCase(true)]
		[TestCase(false)]
		public void ShouldConsiderDataTheEndUserHasNoPermissionFor(bool havePermissonOtherAgentsSchedule)
		{
			var personId = Guid.NewGuid();
			Database.WithTenant("_").WithPerson(personId, "_");
			var dataRangeOptions = havePermissonOtherAgentsSchedule
				? AvailableDataRangeOption.Everyone
				: AvailableDataRangeOption.MyOwn; 
			foreach (var applicationFunction in new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions)
			{
				Database.WithRole(dataRangeOptions, applicationFunction.FunctionPath);
			}
			var date = new DateOnly(2017, 5, 15);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 9, 0, 60), new TimePeriodWithSegment(16, 0, 17, 0, 60), new ShiftCategory().WithId()));
			var agent = new Person().WithId(personId).InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			var otherAgent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill);
			var otherAgentsSchedule = new PersonAssignment(otherAgent, scenario, date).WithLayer(activity, new TimePeriod(8, 9));
			var skillDay = skill.CreateSkillDayWithDemandOnInterval(scenario, date, 1, new Tuple<TimePeriod, double>(new TimePeriod(8, 9), 1.5));
			LogOnOff.LogOn("_", agent, Database.CurrentBusinessUnitId());	
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, date, new[] { agent, otherAgent }, otherAgentsSchedule, skillDay, CurrentAuthorization);

			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, date.ToDateOnlyPeriod());

			schedulerStateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().ShiftLayers.Single().Period.StartDateTime.Hour
				.Should().Be.EqualTo(9);
		}
		
		

		public SchedulingDesktopPermissionTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}
