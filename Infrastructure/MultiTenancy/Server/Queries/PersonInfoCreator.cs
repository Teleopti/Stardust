using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		
		public void CreateAndPersistPersonInfo(IPersonInfoModel personInfo)
		{
			_persister.Persist(_infoMapper.Create(personInfo));
		}

	}
}