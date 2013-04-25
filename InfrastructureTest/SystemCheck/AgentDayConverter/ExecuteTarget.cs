﻿using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	public static class ExecuteTarget
	{
		 public static void AuditDateSetterAndWrapInTransaction(Guid personId, TimeZoneInfo timeZone)
		 {
			 using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
			 {
				 conn.Open();
				 using (var tran = conn.BeginTransaction())
				 {
					 var target = new PersonAssignmentAuditDateSetter(tran);
					 target.Execute(personId, timeZone);
					 tran.Commit();
				 }
			 }
		 }

		 public static void DateSetterAndWrapInTransaction(Guid personId, TimeZoneInfo timeZone)
		 {
			 using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
			 {
				 conn.Open();
				 using (var tran = conn.BeginTransaction())
				 {
					 var target = new PersonAssignmentDateSetter(tran);
					 target.Execute(personId, timeZone);
					 tran.Commit();
				 }
			 }
		 }

		public static void PersonTimeZoneSetterAndWrapInTransaction(Guid personId, TimeZoneInfo timeZone)
		{
			using (var conn = new SqlConnection(UnitOfWorkFactory.Current.ConnectionString))
			{
				conn.Open();
				using (var tran = conn.BeginTransaction())
				{
					var target = new PersonTimeZoneSetter(tran);
					target.Execute(personId, timeZone);
					tran.Commit();
				}
			}
		}
	}
}