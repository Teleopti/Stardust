using System.Collections.Generic;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PersonLoadAllWithPeriodAndExternalLogOn : IPersonLoadAllWithPeriodAndExternalLogOn
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public PersonLoadAllWithPeriodAndExternalLogOn(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<IPerson> LoadAll()
		{
			return _unitOfWork.Current().Session()
				.CreateCriteria<IPerson>()
				.SetFetchMode("PersonPeriodCollection", FetchMode.Join)
				.SetFetchMode("PersonPeriodCollection.ExternalLogOnCollection", FetchMode.Join)
				.List<Person>();
		}
	}
}