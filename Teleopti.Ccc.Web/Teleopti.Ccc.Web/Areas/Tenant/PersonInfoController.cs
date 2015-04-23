using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.Web.Areas.Tenant
{
	//TODO: tenant - when adding scenarios for multi tenancy scenario(s), use this controller!
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
		public virtual void Persist(IEnumerable<PersonInfoModel> personInfos)
		{
			foreach (var personInfoModel in personInfos)
			{
				_persister.Persist(_mapper.Map(personInfoModel));
			}
		}

		[HttpPost]
		//TODO: tenant rename to Persist when old method is gone
		//TODO: tenant - change from returning an json object with errors to non 200 http error codes
		public virtual JsonResult PersistNew(PersonInfoModel personInfoModel)
		{
			var ret = new PersistPersonInfoResult();
			try
			{
				PersistNewInternal(personInfoModel);
			}
			catch (PasswordStrengthException)
			{
				ret.PasswordStrengthIsValid = false;
			}
			catch (DuplicateApplicationLogonNameException)
			{
				ret.ApplicationLogonNameIsValid = false;
			}
			catch (DuplicateIdentityException)
			{
				ret.IdentityIsValid = false;
			}
			return Json(ret);
		}

		//TODO: tenant, needs to be extra method to catch uow attribute errors. Fix soon!
		[TenantUnitOfWork]
		protected virtual void PersistNewInternal(PersonInfoModel personInfoModel)
		{
			_persister.Persist(_mapper.Map(personInfoModel));
		}

		[HttpPost]
		[TenantUnitOfWork]
		//TODO: tenant - probably return some kind of json result later
		//TODO: tenant - change later to some sort of authentication
		public virtual void Delete(IEnumerable<Guid> personIdsToDelete)
		{
			foreach (var personInfoDelete in personIdsToDelete)
			{
				_deletePersonInfo.Delete(personInfoDelete);
			}
		}
	}
}