using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest.Core
{
	[WfmAdminTest]
	[AllTogglesOn]
	public class PurgeOldSignInAttemptsTest
	{
		public ICurrentTenantSession TenantUnitOfWork;
		public IPurgeOldSignInAttempts Target;
		public MutableNow Now;
		public ITenantServerConfiguration TenantServerConfiguration;

		[Test]
		public void PurgeSignInAttemptsOlderThan30Days()
		{
			Now.Is(new DateTime(2018,06,04));
			var uow = TenantUnitOfWork as TenantUnitOfWorkManager;
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (uow.EnsureUnitOfWorkIsStarted())
			{
				var session = uow.CurrentSession();
				var model = new LoginAttemptModel
				{
					Client = Guid.NewGuid().ToString(),
					ClientIp = Guid.NewGuid().ToString(),
					PersonId = Guid.NewGuid(),
					Provider = Guid.NewGuid().ToString(),
					Result = Guid.NewGuid().ToString(),
					UserCredentials = Guid.NewGuid().ToString()
				};

				var initialDate = Now.UtcDateTime().AddDays(-60);
				foreach (var dayCounter in Enumerable.Range(0, 60))
				{
					SaveLoginAttempt(session, model, initialDate.AddDays(dayCounter));
				}

				Target.Purge();
			}
			using (uow.EnsureUnitOfWorkIsStarted())
			{
				var session = uow.CurrentSession();

				var loginAttempts = GetAllLoginAttempts(session);
				loginAttempts.Count.Should().Be(30);
			}
		}

		[Test]
		public void ShouldHandleWhenPreserveLogonAttemptsDaysIsMissingInDb()
		{
			
			Now.Is(new DateTime(2018, 06, 04));
			var uow = TenantUnitOfWork as TenantUnitOfWorkManager;
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			using (uow.EnsureUnitOfWorkIsStarted())
			{
				var session = uow.CurrentSession();
				DeletePreserveLogonAttemptsDays(session);

				var model = new LoginAttemptModel
				{
					Client = Guid.NewGuid().ToString(),
					ClientIp = Guid.NewGuid().ToString(),
					PersonId = Guid.NewGuid(),
					Provider = Guid.NewGuid().ToString(),
					Result = Guid.NewGuid().ToString(),
					UserCredentials = Guid.NewGuid().ToString()
				};

				var initialDate = Now.UtcDateTime().AddDays(-60);
				foreach (var dayCounter in Enumerable.Range(0, 60))
				{
					SaveLoginAttempt(session, model, initialDate.AddDays(dayCounter));
				}

				Target.Purge();
			}
			using (uow.EnsureUnitOfWorkIsStarted())
			{
				var session = uow.CurrentSession();
				var loginAttempts = GetAllLoginAttempts(session);
				loginAttempts.Count.Should().Be(30);
			}
		}

		private static void DeletePreserveLogonAttemptsDays(ISession currentTenantSession)
		{
			currentTenantSession.CreateSQLQuery("DELETE FROM Tenant.ServerConfiguration WHERE [Key]='PreserveLogonAttemptsDays'").ExecuteUpdate();
		}

		private static void SaveLoginAttempt(ISession currentTenantSession, LoginAttemptModel model, DateTime dateTimeUtc)
		{
			currentTenantSession.CreateSQLQuery(
					"INSERT INTO [Tenant].[Security] (Result, UserCredentials, Provider, Client, ClientIp, PersonId, DateTimeUtc) VALUES (:Result, :UserCredentials, :Provider, :Client, :ClientIp, :PersonId, :dateTimeUtc)")
				.SetString("Result", model.Result)
				.SetString("UserCredentials", model.UserCredentials)
				.SetString("Provider", model.Provider)
				.SetString("Client", model.Client)
				.SetString("ClientIp", model.ClientIp)
				.SetGuid("PersonId", model.PersonId.GetValueOrDefault())
				.SetDateTime("dateTimeUtc", dateTimeUtc)
				.ExecuteUpdate();
		}

		private static IList<LoginAttemptModelTest> GetAllLoginAttempts(ISession currentTenantSession)
		{
			return currentTenantSession.CreateSQLQuery(
					"SELECT * FROM [Tenant].[Security]")
				.SetResultTransformer(Transformers.AliasToBean(typeof(LoginAttemptModelTest)))
				.List<LoginAttemptModelTest>();
		}
	}

	public class LoginAttemptModelTest
	{
		public int Id { get; set; }
		public DateTime DateTimeUtc { get; set; }
		public string Result { get; set; }
		public string UserCredentials { get; set; }
		public string Provider { get; set; }
		public string Client { get; set; }
		public string ClientIp { get; set; }
		public Guid? PersonId { get; set; }
	}
}
