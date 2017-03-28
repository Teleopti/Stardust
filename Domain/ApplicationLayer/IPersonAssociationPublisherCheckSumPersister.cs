using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IPersonAssociationPublisherCheckSumPersister
	{
		IEnumerable<PersonAssociationCheckSum> Get();
		void Persist(PersonAssociationCheckSum checkSum);
		PersonAssociationCheckSum Get(Guid personId);
	}

	public class PersonAssociationCheckSum
	{
		public Guid PersonId;
		public int CheckSum;
	}
}