using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IEtlReadModelRepository
	{
		void InsertScheduledChanged(ScheduleChangedReadModel model);
	}
	public class EtlReadModelRepository : IEtlReadModelRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public EtlReadModelRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

 		public void InsertScheduledChanged(ScheduleChangedReadModel model)
 		{
			((NHibernateUnitOfWork)_currentUnitOfWork.Current()).Session.CreateSQLQuery(
					"INSERT INTO [ReadModel].[EtlScheduledChanged] (Scenario,Person,Date) VALUES (:ScenarioId,:PersonId,:Date)")
					.SetGuid("ScenarioId", model.ScenarioId)
					.SetGuid("PersonId", model.PersonId)
					.SetDateTime("Date", model.DateTime)
					.ExecuteUpdate();
 		}
	}

	public class ScheduleChangedReadModel
	{
		public Guid ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public DateTime DateTime { get; set; }
	}
}