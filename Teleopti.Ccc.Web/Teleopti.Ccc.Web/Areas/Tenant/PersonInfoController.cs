﻿using System.Web.Mvc;
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
		private readonly IDeletePersonInfo _deletePersonInfo;

		public PersonInfoController(IPersistPersonInfo persister, 
																IPersonInfoMapper mapper,
																IDeletePersonInfo deletePersonInfo)
		{
			_persister = persister;
			_mapper = mapper;
			_deletePersonInfo = deletePersonInfo;
		}

		[HttpPost]
		[TenantUnitOfWork]
		//TODO: tenant - probably return some kind of json result later
		//TODO: tenant - change later to some sort of authentication
		public virtual void Persist(PersonInfoModels personInfoModels)
		{
			foreach (var personInfoModel in personInfoModels.PersonInfos)
			{
				_persister.Persist(_mapper.Map(personInfoModel));
			}
		}

		[HttpPost]
		[TenantUnitOfWork]
		//TODO: tenant - probably return some kind of json result later
		//TODO: tenant - change later to some sort of authentication
		public virtual void Delete(PersonInfoDeletes personInfoDeletes)
		{
			foreach (var personInfoDelete in personInfoDeletes.PersonInfosToDelete)
			{
				_deletePersonInfo.Delete(personInfoDelete);
			}
		}
	}
}