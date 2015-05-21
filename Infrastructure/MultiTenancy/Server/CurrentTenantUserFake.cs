using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class CurrentTenantUserFake : ICurrentTenantUser
	{
		private PersonInfo _currentUser;

		public CurrentTenantUserFake()
		{
			_currentUser = new PersonInfo(new Tenant("_"), Guid.Empty);
		}

		public PersonInfo CurrentUser()
		{
			return _currentUser;
		}

		public void Set(PersonInfo personInfo)
		{
			_currentUser = personInfo;
		}
	}
}