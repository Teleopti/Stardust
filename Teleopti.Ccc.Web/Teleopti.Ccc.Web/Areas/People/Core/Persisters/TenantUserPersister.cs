using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

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

		[TenantUnitOfWork]
		public List<string> Persist(PersonInfoModel tenantUserData)
		{
			var errorMessages = new List<string>();

			try
			{
				_persistPersonInfo.Persist(_personInfoMapper.Map(tenantUserData));
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
			catch(Exception e)
			{
				errorMessages.Add(Resources.InternalErrorXMsg);
			}

			return errorMessages;
		}
	}
}