using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class DeletePersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public DeletePersonInfo(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Delete(PersonInfo personInfo)
		{
			_currentTenantSession.CurrentSession().Delete(personInfo);
		}
	}
}