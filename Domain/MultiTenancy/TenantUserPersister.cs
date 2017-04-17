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
		void RollbackAllPersistedTenantUsers();
	}

	public class TenantUserPersister : ITenantUserPersister
	{
		private readonly IPersonInfoCreator _creator;
		private readonly IList<Guid> _persistedTenantUserIds = new List<Guid>();

		public TenantUserPersister(IPersonInfoCreator creator)
		{
			_creator = creator;
		}
		public List<string> Persist(IPersonInfoModel tenantUserData)
		{
			var errorMessages = new List<string>();

			try
			{
				_persistedTenantUserIds.Add(PersistInternal(tenantUserData));
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

			return errorMessages;
		}
	
		[TenantUnitOfWork]
		protected virtual Guid PersistInternal(IPersonInfoModel personInfo)
		{
			return _creator.CreateAndPersistPersonInfo(personInfo);
		}

		[TenantUnitOfWork]
		public virtual void RollbackAllPersistedTenantUsers()
		{
			foreach (var personId in _persistedTenantUserIds)
			{
				_creator.RollbackPersistedTenantUsers(personId);
			}
		}
	}
}