using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Wfm.Adherence.Domain.Configuration;

namespace Teleopti.Wfm.Adherence.Domain.Infrastructure.Repositories
{
	public class RtaMapRepository : Repository<IRtaMap>,
		IRtaMapRepository
	{
		public RtaMapRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public RtaMapRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
		
		public IList<IRtaMap> LoadAllCompleteGraph()
		{
			return Session.CreateCriteria(typeof (RtaMap))
				.SetFetchMode("Activity", FetchMode.Join)
				.SetFetchMode("StateGroup", FetchMode.Join)
				.SetFetchMode("RtaRule", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IRtaMap>();
		}
	}
}
