using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePersonAssociationPublisherCheckSumPersister : IPersonAssociationPublisherCheckSumPersister
	{
		private IEnumerable<PersonAssociationCheckSum> _data = Enumerable.Empty<PersonAssociationCheckSum>();

		public IEnumerable<PersonAssociationCheckSum> Get()
		{
			return _data.ToArray();
		}

		public PersonAssociationCheckSum Get(Guid personId)
		{
			return _data.SingleOrDefault(c => c.PersonId == personId);
		}

		public void Persist(PersonAssociationCheckSum checkSum)
		{
			_data = _data.Except(x => x.PersonId == checkSum.PersonId)
				.Append(checkSum)
				.ToArray();
		}
	}
}