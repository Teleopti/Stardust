using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	[LoggedOff]
	[DefaultData]
	[RealPermissions]
	public class SchedulingDesktopPermissionTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		public Func<IGridlockManager> LockManager;
		public FakeDatabase Database;
		public FakePersonRepository PersonRepository;
		public ILogOnOff LogOnOff;

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
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);
			var schedulingOptions = new SchedulingOptions { ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff };
			LockManager().AddLock(agent, firstDay, LockType.WriteProtected);

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] { agent }, period);

			return schedulerStateHolder.Schedules[agent].ScheduledDay(firstDay).IsScheduled();
		}

		public SchedulingDesktopPermissionTest(SeperateWebRequest seperateWebRequest, RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, RemoveImplicitResCalcContext removeImplicitResCalcContext46680, bool resourcePlannerTimeZoneIssues45818) : base(seperateWebRequest, resourcePlannerRemoveClassicShiftCat46582, removeImplicitResCalcContext46680, resourcePlannerTimeZoneIssues45818)
		{
		}
	}
}
