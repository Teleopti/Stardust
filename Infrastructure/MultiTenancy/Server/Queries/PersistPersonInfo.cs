using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersistPersonInfo : IPersistPersonInfo
	{
		private readonly ICurrentTenantSession _currentTenantSession;

		public PersistPersonInfo(ICurrentTenantSession currentTenantSession)
		{
			_currentTenantSession = currentTenantSession;
		}

		public void Persist(PersonInfo personInfo, string logonName, bool noNewPassword)
		{
			if (personInfo.Id == Guid.Empty)
				throw new ArgumentException("Missing explicitly set id on personInfo object.");

			var session = _currentTenantSession.CurrentSession();
			var oldPersonInfo = session.Get<PersonInfo>(personInfo.Id);
			if (oldPersonInfo == null)
			{
				session.Save(personInfo);
			}
			else
			{
				personInfo.ReuseTenantPassword(oldPersonInfo);
				// if we save an old we must reuse the old password if we get an logonname and no new password
				if (!string.IsNullOrEmpty(logonName) && !string.IsNullOrEmpty(oldPersonInfo.ApplicationLogonInfo.LogonPassword) && noNewPassword)
				{
					personInfo.ApplicationLogonInfo.ReuseLogon(logonName, oldPersonInfo.ApplicationLogonInfo.LogonPassword);
				}
					session.Merge(personInfo);
			}
			
		}
	}
}