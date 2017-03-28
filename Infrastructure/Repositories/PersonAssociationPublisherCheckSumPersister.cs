using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonAssociationPublisherCheckSumPersister : IPersonAssociationPublisherCheckSumPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonAssociationPublisherCheckSumPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<PersonAssociationCheckSum> Get()
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM dbo.PersonAssociationCheckSum")
				.SetResultTransformer(Transformers.AliasToBean<PersonAssociationCheckSum>())
				.List<PersonAssociationCheckSum>();
		}

		public void Persist(PersonAssociationCheckSum checkSum)
		{
			if (checkSum.CheckSum == 0)
			{
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
DELETE dbo.PersonAssociationCheckSum 
WHERE
PersonId = :PersonId")
					.SetParameter("PersonId", checkSum.PersonId)
					.ExecuteUpdate();
				return;
			}

			var updated = _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
UPDATE dbo.PersonAssociationCheckSum 
SET  CheckSum = :CheckSum 
WHERE
PersonId = :PersonId")
				.SetParameter("PersonId", checkSum.PersonId)
				.SetParameter("CheckSum", checkSum.CheckSum)
				.ExecuteUpdate();

			if (updated == 0)
				_unitOfWork.Current().Session()
					.CreateSQLQuery(@"
INSERT INTO dbo.PersonAssociationCheckSum (PersonId, [CheckSum])
VALUES (:PersonId, :CheckSum)")
					.SetParameter("PersonId", checkSum.PersonId)
					.SetParameter("CheckSum", checkSum.CheckSum)
					.ExecuteUpdate();
		}

		public PersonAssociationCheckSum Get(Guid personId)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery("SELECT * FROM dbo.PersonAssociationCheckSum WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetResultTransformer(Transformers.AliasToBean<PersonAssociationCheckSum>())
				.UniqueResult<PersonAssociationCheckSum>();
		}
	}
}