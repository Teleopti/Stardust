using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public interface ILoadUserUnauthorized
	{
		IPerson LoadFullPersonInSeperateTransaction(IUnitOfWorkFactory unitOfWorkFactory, Guid personId);
	}
}