using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class PersonInfoController : Controller
	{
		private readonly IPersistPersonInfo _persister;
		private readonly IPersonInfoMapper _mapper;
		private readonly IDeletePersonInfo _deletePersonInfo;
		private readonly IFindLogonInfo _findLogonInfo;

		public PersonInfoController(IPersistPersonInfo persister, 
																IPersonInfoMapper mapper,
																IDeletePersonInfo deletePersonInfo,
																IFindLogonInfo findLogonInfo)
		{
			_persister = persister;
			_mapper = mapper;
			_deletePersonInfo = deletePersonInfo;
			_findLogonInfo = findLogonInfo;
		}

		[HttpPost]
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
		public virtual void Delete(IEnumerable<Guid> personIdsToDelete)
		{
			foreach (var personInfoDelete in personIdsToDelete)
			{
				_deletePersonInfo.Delete(personInfoDelete);
			}
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult LogonInfoFromGuids(IEnumerable<Guid> personIdsToGet)
		{
			return Json(_findLogonInfo.GetForIds(personIdsToGet));
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult LogonInfoFromLogonName(string logonName)
		{
			return Json(_findLogonInfo.GetForLogonName(logonName));
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult LogonInfoFromIdentity(string identity)
		{
			return Json(_findLogonInfo.GetForIdentity(identity));
		}
	}
}