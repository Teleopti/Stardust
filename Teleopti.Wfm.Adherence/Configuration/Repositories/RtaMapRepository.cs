using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Wfm.Adherence.Configuration.Repositories
{
	public class RtaMapRepository : 
		Repository<IRtaMap>,
		IRtaMapRepository
	{
		public static RtaMapRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new RtaMapRepository(currentUnitOfWork);
		}

		public RtaMapRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
		
		public IEnumerable<IRtaMap> LoadAllCompleteGraph()
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
