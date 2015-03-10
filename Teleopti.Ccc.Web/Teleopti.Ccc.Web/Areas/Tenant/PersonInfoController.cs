using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.Web.Areas.Tenant
{
	public class PersonInfoController : Controller
	{
		private readonly IPersistPersonInfo _persister;
		private readonly IPersonInfoMapper _mapper;

		public PersonInfoController(IPersistPersonInfo persister, IPersonInfoMapper mapper)
		{
			_persister = persister;
			_mapper = mapper;
		}

		[HttpPost]
		[TenantUnitOfWork]
		//TODO: tenant - probably return some kind of json result later
		// change later to some sort of authentication
		public void Persist(PersonInfoModels personInfoModels)
		{
			foreach (var personInfoModel in personInfoModels.PersonInfos)
			{
				_persister.Persist(_mapper.Map(personInfoModel));
			}
		}
	}
}