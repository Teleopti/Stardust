using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public interface ILoadUserUnauthorized
	{
		IPerson LoadFullPersonInSeperateTransaction(IUnitOfWorkFactory unitOfWorkFactory, Guid personId);
	}
}