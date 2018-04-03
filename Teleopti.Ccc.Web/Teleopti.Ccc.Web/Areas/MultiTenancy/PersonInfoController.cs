using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class PersonInfoController : ApiController
	{
		private readonly IPersistPersonInfo _persister;
		private readonly IPersonInfoMapper _mapper;
		private readonly IDeletePersonInfo _deletePersonInfo;
		private readonly IFindLogonInfo _findLogonInfo;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;


		public PersonInfoController(IPersistPersonInfo persister,
									IPersonInfoMapper mapper,
									IDeletePersonInfo deletePersonInfo,
									IFindLogonInfo findLogonInfo,
									ITenantUnitOfWork tenantUnitOfWork)
		{
			_persister = persister;
			_mapper = mapper;
			_deletePersonInfo = deletePersonInfo;
			_findLogonInfo = findLogonInfo;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		[HttpPost, Route("PersonInfo/Persist")]
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
			catch (DuplicateApplicationLogonNameException e)
			{
				ret.ApplicationLogonNameIsValid = false;
				ret.ExistingPerson = e.ExistingPerson;
			}
			catch (DuplicateIdentityException e)
			{
				ret.IdentityIsValid = false;
				ret.ExistingPerson = e.ExistingPerson;
			}
			return Ok(ret);
		}

		[TenantUnitOfWork, HttpPost, Route("PersonInfo/PersistApplicationLogonNames")]
		public virtual IHttpActionResult PersistApplicationLogonNames(PersonApplicationLogonInputModel personApplicationLogonInputModel)
		{
			var resultModel = new PersonInfoGenericResultModel();
			// If locking db will be a problem here we might have to do some prevalidation, instead of validating on each update in the transaction.
			foreach (var person in personApplicationLogonInputModel.People)
			{
				var personObj = new PersonInfoModel { PersonId = person.PersonId, ApplicationLogonName = person.ApplicationLogonName };
				var persistResult = _persister.PersistEx(_mapper.Map(personObj));

				if (!string.IsNullOrEmpty(persistResult))
				{
					resultModel.ResultList.Add(new PersonInfoGenericModel { MessageId = persistResult, PersonId = person.PersonId });
				}
			}

			if (resultModel.ResultList.Any())
			{
				_tenantUnitOfWork.CancelAndDisposeCurrent();
				return Content(HttpStatusCode.BadRequest, resultModel);
			}

			return Ok();
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

		[HttpPost, Route("PersonInfo/LogonInfosFromIdentities")]
		[TenantUnitOfWork]
		public virtual IHttpActionResult LogonInfosFromIdentities([FromBody]IEnumerable<string> identities)
		{
			return Ok(_findLogonInfo.GetForIdentities(identities));
		}
	}
}