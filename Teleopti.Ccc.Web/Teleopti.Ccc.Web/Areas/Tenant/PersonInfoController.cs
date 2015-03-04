using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.Web.Areas.Tenant
{
	public class PersonInfoController : Controller
	{
		private readonly IPersonInfoPersister _persister;
		private readonly IPersonInfoMapper _mapper;

		public PersonInfoController(IPersonInfoPersister persister, IPersonInfoMapper mapper)
		{
			_persister = persister;
			_mapper = mapper;
		}

		[HttpPost]
		[TenantUnitOfWork]
		//TODO: tenant - probably return some kind of json result later
		public void Persist(PersonInfoDto personInfoDto)
		{
			_persister.Persist(_mapper.Map(personInfoDto));
		}
	}
}