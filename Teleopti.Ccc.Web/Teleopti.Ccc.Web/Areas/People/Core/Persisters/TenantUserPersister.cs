using System;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.People.Core.Models;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public interface ITenantUserPersister
	{
		List<string> Persist(PersonInfoModel tenantUserData);
	}

	public class TenantUserPersister : ITenantUserPersister
	{
		private readonly IPersistPersonInfo _persistPersonInfo;
		private readonly IPersonInfoMapper _personInfoMapper;

		public TenantUserPersister(IPersistPersonInfo persistPersonInfo, IPersonInfoMapper personInfoMapper)
		{
			_persistPersonInfo = persistPersonInfo;
			_personInfoMapper = personInfoMapper;
		}
		public List<string> Persist(PersonInfoModel tenantUserData)
		{
			var errorMessages = new List<string>();

			try
			{
				PersistInternal(tenantUserData);
			}
			catch (PasswordStrengthException)
			{
				errorMessages.Add(Resources.PasswordPolicyErrorMsgSemicolon);
			}
			catch(DuplicateIdentityException)
			{
				errorMessages.Add(Resources.DuplicatedWindowsLogonErrorMsgSemicolon);
			}
			catch(DuplicateApplicationLogonNameException)
			{
				errorMessages.Add(Resources.DuplicatedApplicationLogonErrorMsgSemicolon);
			}
			catch(Exception exception)
			{
				errorMessages.Add(string.Format(Resources.InternalErrorXMsg, exception.Message));
			}

			return errorMessages;
		}
		[TenantUnitOfWork]
		protected virtual void PersistInternal(PersonInfoModel personInfo)
		{
			_persistPersonInfo.Persist(_personInfoMapper.Map(personInfo));
		}
	}
}