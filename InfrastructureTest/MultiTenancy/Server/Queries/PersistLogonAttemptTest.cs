using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class PersistLogonAttemptTest
	{
		[Test]
		public void ShouldLogWhenPersonIdExists()
		{
			using (var tenantUowManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(UnitOfWorkFactory.Current.ConnectionString))
			{
				tenantUowManager.EnsureUnitOfWorkIsStarted();
				var target = new PersistLogonAttempt(tenantUowManager);
				var model = new LoginAttemptModel
				{
					Client = Guid.NewGuid().ToString(),
					ClientIp = Guid.NewGuid().ToString(),
					PersonId = Guid.NewGuid(),
					Provider = Guid.NewGuid().ToString(),
					Result = Guid.NewGuid().ToString(),
					UserCredentials = Guid.NewGuid().ToString()
				};
				target.SaveLoginAttempt(model);

				var res = target.ReadLast();
				res.Client.Trim().Should().Be.EqualTo(model.Client);
				res.ClientIp.Trim().Should().Be.EqualTo(model.ClientIp);
				res.PersonId.Should().Be.EqualTo(model.PersonId);
				res.Provider.Trim().Should().Be.EqualTo(model.Provider);
				res.Result.Trim().Should().Be.EqualTo(model.Result);
				res.UserCredentials.Trim().Should().Be.EqualTo(model.UserCredentials);
			}
		}

		[Test]
		public void ShouldLogWhenPersonIdNotExists()
		{
			using (var tenantUowManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(UnitOfWorkFactory.Current.ConnectionString))
			{
				tenantUowManager.EnsureUnitOfWorkIsStarted();
				var target = new PersistLogonAttempt(tenantUowManager);
				var model = new LoginAttemptModel
				{
					Client = Guid.NewGuid().ToString(),
					ClientIp = Guid.NewGuid().ToString(),
					Provider = Guid.NewGuid().ToString(),
					Result = Guid.NewGuid().ToString(),
					UserCredentials = Guid.NewGuid().ToString()
				};
				target.SaveLoginAttempt(model);

				var res = target.ReadLast();
				res.Client.Trim().Should().Be.EqualTo(model.Client);
				res.ClientIp.Trim().Should().Be.EqualTo(model.ClientIp);
				res.PersonId.Should().Be.EqualTo(Guid.Empty); //this must be wrong but keep old logic
				res.Provider.Trim().Should().Be.EqualTo(model.Provider);
				res.Result.Trim().Should().Be.EqualTo(model.Result);
				res.UserCredentials.Trim().Should().Be.EqualTo(model.UserCredentials);
			}
		}
	}
}