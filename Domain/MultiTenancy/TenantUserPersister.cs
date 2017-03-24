using System;
using System.Collections.Generic;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface ITenantUserPersister
	{
		List<string> Persist(IPersonInfoModel tenantUserData);
	}

	public class TenantUserPersister : ITenantUserPersister
	{
		private readonly IPersonInfoCreator _creator;

		public TenantUserPersister(IPersonInfoCreator creator)
		{
			_creator = creator;
		}
		public List<string> Persist(IPersonInfoModel tenantUserData)
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
		protected virtual void PersistInternal(IPersonInfoModel personInfo)
		{
			_creator.CreateAndPersistPersonInfo(personInfo);
		}
	}
}