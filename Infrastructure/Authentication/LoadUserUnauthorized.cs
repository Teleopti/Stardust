using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class LoadUserUnauthorized : ILoadUserUnauthorized
	{
		public IPerson LoadFullPersonInSeperateTransaction(IUnitOfWorkFactory unitOfWorkFactory, Guid personId)
		{
			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				return new PersonRepository(uow).LoadPersonAndPermissions(personId);
			}
		}
	}
}