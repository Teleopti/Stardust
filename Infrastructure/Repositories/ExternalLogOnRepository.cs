using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// ExternalLogOnRepository
	/// </summary>
	public class ExternalLogOnRepository : Repository<IExternalLogOn>, IExternalLogOnRepository
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExternalLogOnRepository"/> class.
		/// </summary>
		/// <param name="unitOfWork">The unitofwork</param>
		public ExternalLogOnRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public ExternalLogOnRepository(ICurrentUnitOfWork currentUnitOfWork)
			: base(currentUnitOfWork, null, null)
		{
		}

		public IList<IExternalLogOn> LoadByAcdLogOnNames(IEnumerable<string> externalLogOnNames)
		{
			return Session.CreateCriteria<ExternalLogOn>()
				.Add(Restrictions.InG(nameof(ExternalLogOn.AcdLogOnName), externalLogOnNames))
				.List<IExternalLogOn>();
		}
	}
}