using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	[Category("LongRunning")]
	public abstract class AuditTest : DatabaseTestWithoutTransaction
	{
		protected IPersonAssignment PersonAssignment { get; private set; }
		protected IPerson Agent { get; private set; }
		protected IPersonAbsence PersonAbsence { get; private set; }
		protected DateTime Today { get; private set; }
		protected IScenario Scenario { get; private set; }
		protected IDayOffTemplate DayOffTemplate { get; private set; }
		protected IMultiplicatorDefinitionSet MultiplicatorDefinitionSet { get; private set; }
	

		protected override sealed void SetupForRepositoryTestWithoutTransaction()
		{
			turnOnAudit();
			Agent = PersonFactory.CreatePerson();
			Today = DateTime.UtcNow.Date;
			PersonAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(Agent, new DateTimePeriod(Today, Today.AddDays(1)));
			Scenario = PersonAssignment.Scenario;
			Scenario.DefaultScenario = true;
			PersonAbsence = PersonAbsenceFactory.CreatePersonAbsence(Agent, Scenario, new DateTimePeriod(Today, Today.AddDays(1)));
			DayOffTemplate = DayOffFactory.CreateDayOff(new Description("AuditTestDayOff", "ATDO"));
			MultiplicatorDefinitionSet = MultiplicatorDefinitionSetFactory.CreateMultiplicatorDefinitionSet("AuditTestMultiplicatorDefinitionSet", MultiplicatorType.Overtime);

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = uow.FetchSession();
				session.Save(Agent);
				session.Save(Scenario);
				session.Save(PersonAssignment.ShiftCategory);
				session.Save(PersonAssignment.MainActivities().First().Payload);
				session.Save(PersonAbsence.Layer.Payload);
				session.Save(PersonAssignment);
				session.Save(PersonAbsence);
				session.Save(DayOffTemplate);
				session.Save(MultiplicatorDefinitionSet);
				uow.PersistAll();
			}
			AuditSetup();
		}

		private static void turnOnAudit()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.FetchSession().CreateQuery(@"delete from Revision").ExecuteUpdate();
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var auditSettingRep = new AuditSettingRepository(UnitOfWorkFactory.CurrentUnitOfWork());
				var auditSetting = auditSettingRep.Read();
				auditSetting.TurnOnScheduleAuditing(auditSettingRep, UnitOfWorkFactory.Current.AuditSetting);
				//not persisting this one
			}
		}

		protected virtual void AuditSetup()
		{
		}

	}
}