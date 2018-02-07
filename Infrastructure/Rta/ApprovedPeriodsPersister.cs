using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ApprovePeriodAsInAdherence;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class ApprovedPeriodsPersister : IApprovedPeriodsPersister
	{
		private readonly ICurrentUnitOfWork _unitOfWork;

		public ApprovedPeriodsPersister(ICurrentUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(ApprovedPeriod model)
		{
			_unitOfWork.Current().Session()
				.CreateSQLQuery(@"
INSERT INTO [rta].[ApprovedPeriods]
(
	PersonId,
	StartTime,
	EndTime
)
VALUES
(
	:PersonId,
	:StartTime,
	:EndTime
)")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("StartTime", model.StartTime)
				.SetParameter("EndTime", model.EndTime)
				.ExecuteUpdate();
		}
	}
}