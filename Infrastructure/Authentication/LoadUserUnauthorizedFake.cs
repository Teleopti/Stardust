using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Authentication
{
	public class LoadUserUnauthorizedFake : ILoadUserUnauthorized
	{
		private readonly IDictionary<Guid, IPerson> data = new Dictionary<Guid, IPerson>();

		public IPerson LoadFullPersonInSeperateTransaction(IUnitOfWorkFactory unitOfWorkFactory, Guid personId)
		{
			return data.ContainsKey(personId) ? data[personId] : null;
		}

		public void Has(IPerson person)
		{
			data[person.Id.Value] = person;
		}
	}
}