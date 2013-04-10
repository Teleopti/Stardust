﻿using System;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	[Category("LongRunning")]
	public abstract class AuditTest : DatabaseTestWithoutTransaction
	{
		protected IPersonAssignment PersonAssignment { get; private set; }
		protected IPerson Agent { get; private set; }
		protected IPersonAbsence PersonAbsence { get; private set; }
		protected IPersonDayOff PersonDayOff { get; private set; }
		protected DateTime Today { get; private set; }
		protected IScenario Scenario { get; private set; }
		protected IRepository Repository { get; private set; }
	

		protected override sealed void SetupForRepositoryTestWithoutTransaction()
		{
			turnOnAudit();
			Agent = PersonFactory.CreatePerson();
			Today = DateTime.UtcNow.Date;
			PersonAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(Agent, new DateTimePeriod(Today, Today.AddDays(1)));
			Scenario = PersonAssignment.Scenario;
			Scenario.DefaultScenario = true;
			PersonAbsence = PersonAbsenceFactory.CreatePersonAbsence(Agent, Scenario, new DateTimePeriod(Today, Today.AddDays(1)));
			PersonDayOff = new PersonDayOff(Agent, Scenario, new DayOffTemplate(new Description("test")) { Anchor = TimeSpan.FromMinutes(2) }, new DateOnly(Today));
			Repository = new Repository(UnitOfWorkFactory.Current);

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Repository.Add(Agent);
				Repository.Add(Scenario);
				Repository.Add(PersonAssignment.MainShift.ShiftCategory);
				Repository.Add(PersonAssignment.MainShift.LayerCollection[0].Payload);
				Repository.Add(PersonAssignment.MainShift.LayerCollection[0].Payload.GroupingActivity);
				Repository.Add(PersonAbsence.Layer.Payload);

				Repository.Add(PersonAssignment);
				Repository.Add(PersonAbsence);
				Repository.Add(PersonDayOff);
				uow.PersistAll();
			}
			AuditSetup();
		}

		private static void turnOnAudit()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				fetchSession(uow).CreateQuery(@"delete from Revision").ExecuteUpdate();
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

		private static void turnOffAudit()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var auditSettingRep = new AuditSettingRepository(UnitOfWorkFactory.CurrentUnitOfWork());
				var auditSetting = auditSettingRep.Read();
				auditSetting.TurnOffScheduleAuditing(UnitOfWorkFactory.Current.AuditSetting);
				//not persisting this one
			}
		}

		protected virtual void AuditSetup()
		{
		}

		protected override void TeardownForRepositoryTest()
		{
			//cleanup
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = fetchSession(uow);
				foreach (var scheduleData in session.CreateCriteria(typeof(IPersistableScheduleData)).List())
				{
					session.Delete(scheduleData);
				}
				session.CreateQuery(@"update Person p set p.IsDeleted = 1").ExecuteUpdate();
				session.CreateQuery(@"update Scenario p set p.IsDeleted = 1").ExecuteUpdate();
				session.CreateQuery(@"update ShiftCategory p set p.IsDeleted = 1").ExecuteUpdate();
				session.CreateQuery(@"update Activity p set p.IsDeleted = 1").ExecuteUpdate();
				session.CreateQuery(@"update GroupingActivity p set p.IsDeleted = 1").ExecuteUpdate();
				session.CreateQuery(@"update Absence p set p.IsDeleted = 1").ExecuteUpdate();
				uow.PersistAll();
			}
			turnOffAudit();
		}

		private static ISession fetchSession(IUnitOfWork uow)
		{
			return (ISession)typeof(NHibernateUnitOfWork).GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic)
															.GetValue(uow, null);
		}
	}
}