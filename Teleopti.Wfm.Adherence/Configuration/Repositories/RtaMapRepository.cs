using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Wfm.Adherence.Configuration.Repositories
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
				.Fetch("Activity")
				.Fetch("StateGroup")
				.Fetch("RtaRule")
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IRtaMap>();
		}
	}
}
