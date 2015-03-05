using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.Web.Areas.Tenant
{
	
	[System.Web.Mvc.Authorize]
	public class PersonInfoController : Controller
	{
		private readonly IPersonInfoPersister _persister;
		private readonly IPersonInfoMapper _mapper;

		public PersonInfoController(IPersonInfoPersister persister, IPersonInfoMapper mapper)
		{
			_persister = persister;
			_mapper = mapper;
		}

		[System.Web.Mvc.AllowAnonymous] 
		[System.Web.Mvc.HttpPost]
		[TenantUnitOfWork]
		//TODO: tenant - probably return some kind of json result later
		// change later to some sort of authentication
		public void Persist([FromBody]  List<PersonInfoDto> personInfoDtos)
		{
			foreach (var personInfoDto in personInfoDtos)
			{
				_persister.Persist(_mapper.Map(personInfoDto));
			}
		}
	}
}