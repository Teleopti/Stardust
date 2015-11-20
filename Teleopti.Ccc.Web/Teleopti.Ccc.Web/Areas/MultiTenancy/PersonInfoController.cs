using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class PersonInfoController : ApiController
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

		[HttpPost, Route("PersonInfo/Persist")]
		//TODO: tenant - change from returning an json object with errors to non 200 http error codes
		public virtual IHttpActionResult Persist([FromBody]PersonInfoModel personInfoModel)
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
			return Ok(ret);
		}

		[TenantUnitOfWork]
		protected virtual void PersistInternal(PersonInfoModel personInfoModel)
		{
			_persister.Persist(_mapper.Map(personInfoModel));
		}

		[HttpPost, Route("PersonInfo/Delete")]
		[TenantUnitOfWork]
		public virtual void Delete([FromBody]IEnumerable<Guid> personIdsToDelete)
		{
			foreach (var personInfoDelete in personIdsToDelete)
			{
				_deletePersonInfo.Delete(personInfoDelete);
			}
		}

		[HttpPost, Route("PersonInfo/LogonInfoFromGuids")]
		[TenantUnitOfWork]
		public virtual IHttpActionResult LogonInfoFromGuids([FromBody]IEnumerable<Guid> personIdsToGet)
		{
			return Ok(_findLogonInfo.GetForIds(personIdsToGet));
		}

		[HttpGet, Route("PersonInfo/LogonFromName")]
		[TenantUnitOfWork]
		public virtual IHttpActionResult LogonInfoFromLogonName(string logonName)
		{
			return Ok(_findLogonInfo.GetForLogonName(logonName));
		}

		[HttpGet, Route("PersonInfo/LogonFromIdentity")]
		[TenantUnitOfWork]
		public virtual IHttpActionResult LogonInfoFromIdentity(string identity)
		{
			return Ok(_findLogonInfo.GetForIdentity(identity));
		}
	}
}