using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface ITenantUserPersister
	{
		dynamic Persist(IPersonInfoModel tenantUserData);
		void RollbackAllPersistedTenantUsers(IList<Guid> tenantUserIds);
	}

	public class TenantUserPersister : ITenantUserPersister
	{
		private readonly IPersonInfoCreator _creator;

		public TenantUserPersister(IPersonInfoCreator creator)
		{
			_creator = creator;
		}
		public dynamic Persist(IPersonInfoModel tenantUserData)
		{
			var errorMessages = new List<string>();
			var persistedPersonInfoId = Guid.Empty;
			try
			{
				persistedPersonInfoId = PersistInternal(tenantUserData);
			}
			catch (PasswordStrengthException)
			{
				errorMessages.Add(Resources.PasswordPolicyErrorMsgSemicolon);
			}
			catch (DuplicateIdentityException)
			{
				errorMessages.Add(Resources.DuplicatedWindowsLogonErrorMsgSemicolon);
			}
			catch (DuplicateApplicationLogonNameException)
			{
				errorMessages.Add(Resources.DuplicatedApplicationLogonErrorMsgSemicolon);
			}
			catch (Exception exception)
			{
				errorMessages.Add(string.Format(Resources.InternalErrorMsg, exception.Message));
			}

			return new
			{
				TenantUserId = persistedPersonInfoId,
				ErrorMessages = errorMessages
			};
		}
	
		[TenantUnitOfWork]
		protected virtual Guid PersistInternal(IPersonInfoModel personInfo)
		{
			return _creator.CreateAndPersistPersonInfo(personInfo);
		}

		[TenantUnitOfWork]
		public virtual void RollbackAllPersistedTenantUsers(IList<Guid> tenantUserIds)
		{
			foreach (var personId in tenantUserIds)
			{
				_creator.RollbackPersistedTenantUsers(personId);
			}
		}
	}
}