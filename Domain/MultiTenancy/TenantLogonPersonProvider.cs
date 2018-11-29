using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public interface ITenantLogonPersonProvider
	{
		IEnumerable<IPersonInfoModel> GetByLogonNames(IEnumerable<string> logonNames);
	}

	public class TenantLogonPersonProvider : ITenantLogonPersonProvider
	{
		private readonly ITenantPersonLogonQuerier _personLogonQuerier;

		public TenantLogonPersonProvider(ITenantPersonLogonQuerier personLogonQuerier)
		{
			_personLogonQuerier = personLogonQuerier;
		}

		public IEnumerable<IPersonInfoModel> GetByLogonNames(IEnumerable<string> logonNames)
		{
			return FindInternal(logonNames);
		}

		[TenantUnitOfWork]
		protected virtual IEnumerable<IPersonInfoModel> FindInternal(IEnumerable<string> logonNames)
		{
			return _personLogonQuerier.FindApplicationLogonUsers(logonNames);
		}
	}
}
