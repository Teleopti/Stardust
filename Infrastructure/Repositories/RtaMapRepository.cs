using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
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

		public override bool ValidateUserLoggedOn
		{
			get { return false; }
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
