using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersonInfoCreator : IPersonInfoCreator
	{
		private readonly IPersonInfoHelper _infoMapper;
		private readonly IPersistPersonInfo _persister;
		private readonly IDeletePersonInfo _deleter;


		public PersonInfoCreator(IPersonInfoHelper infoMapper, IPersistPersonInfo persister, IDeletePersonInfo deleter)
		{
			_infoMapper = infoMapper;
			_persister = persister;
			_deleter = deleter;
		}

		public void RollbackPersistedTenantUsers(Guid personId)
		{
			_deleter.Delete(personId);
		}


		public Guid CreateAndPersistPersonInfo(IPersonInfoModel personInfo)
		{
			var tenantUser = _infoMapper.Create(personInfo);
			_persister.Persist(tenantUser);
			return tenantUser.Id;
		}
	}
}