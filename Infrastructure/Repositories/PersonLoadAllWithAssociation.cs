using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonLoadAllWithAssociation : IPersonLoadAllWithAssociation
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonLoadAllWithAssociation(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<IPerson> LoadAll()
		{
			return _unitOfWork.Current().Session()
				.CreateCriteria<IPerson>()
				.Fetch("PersonPeriodCollection")
				.Fetch("PersonPeriodCollection.Team")
				.Fetch("PersonPeriodCollection.Team.Site")
				.Fetch("PersonPeriodCollection.ExternalLogOnCollection")
				.SetResultTransformer(new DistinctRootEntityResultTransformer())
				.List<Person>();
		}
	}
}