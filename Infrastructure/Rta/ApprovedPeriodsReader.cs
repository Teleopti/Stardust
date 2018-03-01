using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.AgentAdherenceDay;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class ApprovedPeriodsReader : IApprovedPeriodsReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public ApprovedPeriodsReader(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<ApprovedPeriod> Read(Guid personId, DateTime startTime, DateTime endTime)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
SELECT * FROM [rta].[ApprovedPeriods] 
WHERE PersonId = :PersonId
AND StartTime <= :EndTime
AND EndTime >= :StartTime
")
				.SetParameter("PersonId", personId)
				.SetParameter("StartTime", startTime)
				.SetParameter("EndTime", endTime)
				.SetResultTransformer(Transformers.AliasToBean<approvedPeriod>())
				.List<ApprovedPeriod>();
		}

		internal class approvedPeriod : ApprovedPeriod
		{
			public new DateTime StartTime
			{
				set => base.StartTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
			}
			
			public new DateTime EndTime
			{
				set => base.EndTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
			}
		}
	}
}