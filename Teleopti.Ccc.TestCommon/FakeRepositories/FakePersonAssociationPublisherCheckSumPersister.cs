using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;

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
			Persist(checkSum.AsArray());
		}

		public void Persist(IEnumerable<PersonAssociationCheckSum> checkSums)
		{		
			_data = _data.Where(x => !checkSums.Select(y => y.PersonId).Contains(x.PersonId))
				.Union(checkSums)
				.ToArray();
		}
	}
}