using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Util;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class PersonInfoController : ApiController
	{
		private readonly IPersistPersonInfo _persister;
		private readonly IPersonInfoMapper _mapper;
		private readonly IDeletePersonInfo _deletePersonInfo;
		private readonly IFindLogonInfo _findLogonInfo;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly SignatureCreator _signatureCreator;

		public PersonInfoController(IPersistPersonInfo persister,
									IPersonInfoMapper mapper,
									IDeletePersonInfo deletePersonInfo,
									IFindLogonInfo findLogonInfo,
									ITenantUnitOfWork tenantUnitOfWork,
									SignatureCreator signatureCreator)
		{
			_persister = persister;
			_mapper = mapper;
			_deletePersonInfo = deletePersonInfo;
			_findLogonInfo = findLogonInfo;
			_tenantUnitOfWork = tenantUnitOfWork;
			_signatureCreator = signatureCreator;
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

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.PeopleAccess)]
		[TenantUnitOfWork, HttpPost, Route("PersonInfo/PersistApplicationLogonNames")]
		public virtual IHttpActionResult PersistApplicationLogonNames(SignedArgument<PersonApplicationLogonInputModel> input)
		{
			// Verify signature. Do this with attribute and HTTP headers later.
			if (input == null || !CheckValidSignedArgument(input.Body.ToJson(), input.Body.Intent, input.Signature, input.Body.TimeStamp, TimeSpan.FromMinutes(1), DefinedRaptorApplicationFunctionForeignIds.PeopleAccess))
			{
				var res = new BaseResultModel();
				res.Errors.Add("Invalid argument");
				return Ok(res);
			}

			var resultModel = new BaseResultModel
			{
				Errors = input.Body.People
					.Select(p =>
					{
						var model = new PersonInfoModel { PersonId = p.PersonId, ApplicationLogonName = p.ApplicationLogonName };
						return new { PersistResult = _persister.PersistApplicationLogonName(new AppLogonChangeActionObj(){PersonInfo =  _mapper.Map(model),ThrowOnError =  false}), p.PersonId };
					})
					.Where(r => !string.IsNullOrEmpty(r.PersistResult))
					.Select(r => (object)new PersonInfoGenericModel { Message = r.PersistResult, PersonId = r.PersonId }).ToList()
			};

			if (resultModel.Errors.Any())
			{
				_tenantUnitOfWork.CancelAndDisposeCurrent();
				return Ok(resultModel);
			}

			return Ok(resultModel);
		}

		[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.PeopleAccess)]
		[TenantUnitOfWork, HttpPost, Route("PersonInfo/PersistIdentities")]
		public virtual IHttpActionResult PersistIdentities(SignedArgument<PersonIdentitiesInputModel> input)
		{
			// Verify signature. Do this with attribute and HTTP headers later.
			if (input == null || !CheckValidSignedArgument(input.Body.ToJson(), input.Body.Intent, input.Signature, input.Body.TimeStamp, TimeSpan.FromMinutes(1), DefinedRaptorApplicationFunctionForeignIds.PeopleAccess))
			{
				var res = new BaseResultModel();
				res.Errors.Add("Invalid argument");
				return Ok(res);
			}

			var resultModel = new BaseResultModel
			{
				Errors = input.Body.People
					.Select(p =>
					{
						var model = new PersonInfoModel { PersonId = p.PersonId, Identity = p.Identity };
						return new { PersistResult = _persister.PersistIdentity(new IdentityChangeActionObj(){PersonInfo = _mapper.Map(model),ThrowOnError =  false}), p.PersonId };
					})
					.Where(r => !string.IsNullOrEmpty(r.PersistResult))
					.Select(r => (object)new PersonInfoGenericModel { Message = r.PersistResult, PersonId = r.PersonId }).ToList()
			};

			if (resultModel.Result.Any())
			{
				_tenantUnitOfWork.CancelAndDisposeCurrent();
				return Ok(resultModel);
			}

			return Ok(resultModel);
		}

		private bool CheckValidSignedArgument(string body, string intent, string signature, string timeStampString, TimeSpan validAge, string appFuncForeginId)
		{
			var timeSpan = DateTime.Parse(timeStampString);
			return timeSpan.Add(validAge) > DateTime.UtcNow && intent == appFuncForeginId && _signatureCreator.Verify(body, signature);
		}

		[TenantUnitOfWork]
		protected virtual void PersistInternal(PersonInfoModel personInfoModel)
		{
			_persister.Persist(new GenericPersistApiCallActionObj() { PersonInfo =  _mapper.Map(personInfoModel)});
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