using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class LoadUserUnauthorized : ILoadUserUnauthorized
	{
		public IPerson LoadFullPersonInSeperateTransaction(IUnitOfWorkFactory unitOfWorkFactory, Guid personId)
		{
			using (var uow = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				return PersonRepository.DONT_USE_CTOR(new ThisUnitOfWork(uow), null, null).LoadPersonAndPermissions(personId);
			}
		}
	}
}