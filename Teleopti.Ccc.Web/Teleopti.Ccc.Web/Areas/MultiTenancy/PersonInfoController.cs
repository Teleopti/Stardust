using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
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
		//TODO: tenant - change later to some sort of authentication
		//TODO: tenant - change from returning an json object with errors to non 200 http error codes
		public virtual JsonResult Persist(PersonInfoModel personInfoModel)
		{
			var ret = new PersistPersonInfoResult();
			try
			{
				PersistInternal(personInfoModel);
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

		[TenantUnitOfWork]
		protected virtual void PersistInternal(PersonInfoModel personInfoModel)
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

		[HttpPost]
		[TenantUnitOfWork]
		//TODO: tenant - change later to some sort of authentication
		//TODO: tenant - make sure to only get user info from calling tenant
		public virtual JsonResult LogonInfoFromGuids(IEnumerable<Guid> personIdsToGet)
		{
			//dummy implementation
			var ret = new List<LogonInfoModel>();
			foreach (var guid in personIdsToGet)
			{
				ret.Add(new LogonInfoModel{PersonId = guid, Identity = "DummyIdenty", LogonName = "name" + personIdsToGet});
			}
			
			return Json(ret);
		}
	}
}