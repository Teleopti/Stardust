using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries
{
	public class PersonInfoCreator : IPersonInfoCreator
	{
		private readonly IPersonInfoHelper _infoMapper;
		private readonly IPersistPersonInfo _persister;


		public PersonInfoCreator(IPersonInfoHelper infoMapper, IPersistPersonInfo persister)
		{
			_infoMapper = infoMapper;
			_persister = persister;
		}

		public void RollbackPersistedTenantUsers(Guid personId)
		{
			_persister.RollBackPersonInfo(personId, _infoMapper.GetCurrentTenant().Name);
		}


		public Guid CreateAndPersistPersonInfo(IPersonInfoModel personInfo)
		{
			var tenantUser = _infoMapper.Create(personInfo);
			_persister.Persist(new GenericPersistApiCallActionObj(){PersonInfo = tenantUser});
			return tenantUser.Id;
		}
	}
}