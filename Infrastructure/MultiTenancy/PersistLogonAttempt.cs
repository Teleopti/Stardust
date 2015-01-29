using NHibernate.Transform;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class PersistLogonAttempt : IPersistLogonAttempt
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public PersistLogonAttempt(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void SaveLoginAttempt(LoginAttemptModel model)
		{
			_currentTenantSession.CurrentSession().CreateSQLQuery(
					"INSERT INTO [Auditing].[Security] (Result, UserCredentials, Provider, Client, ClientIp, PersonId) VALUES (:Result, :UserCredentials, :Provider, :Client, :ClientIp, :PersonId)")
					.SetString("Result", model.Result)
					.SetString("UserCredentials", model.UserCredentials)
					.SetString("Provider", model.Provider)
					.SetString("Client", model.Client)
					.SetString("ClientIp", model.ClientIp)
					.SetGuid("PersonId", model.PersonId.GetValueOrDefault())
					.ExecuteUpdate();
		}

		public LoginAttemptModel ReadLast()
		{
			return _currentTenantSession.CurrentSession().CreateSQLQuery(
				"select top 1 Result, UserCredentials, Provider, Client, ClientIp, PersonId from [Auditing].[Security] order by DateTimeUTC desc")
				.SetResultTransformer(Transformers.AliasToBean(typeof(LoginAttemptModel)))
				.UniqueResult<LoginAttemptModel>();
		}
	}
}