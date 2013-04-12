using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.ReadModel;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IEtlReadModelRepository
	{
		IList<IScheduleChangedReadModel> ChangedDataOnStep(DateTimePeriod onPeriod, IBusinessUnit currentBusinessUnit, string stepName);
	}
	public class EtlReadModelRepository : IEtlReadModelRepository
	{
		private readonly IUnitOfWork _currentUnitOfWork;

		public EtlReadModelRepository(IUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IList<IScheduleChangedReadModel> ChangedDataOnStep(DateTimePeriod onPeriod, IBusinessUnit currentBusinessUnit, string stepName)
 		{
			return ((NHibernateUnitOfWork)_currentUnitOfWork).Session.CreateSQLQuery(
					"exec mart.[ChangedDataOnStep] :step, :startdate, :enddate, :bu ")
					.SetDateTime("startdate", onPeriod.StartDateTime)
					.SetDateTime("enddate", onPeriod.EndDateTime)
					.SetGuid("bu", currentBusinessUnit.Id.GetValueOrDefault())
					.SetString("step", stepName)
					.List<IScheduleChangedReadModel>();
			
 		}
	}


	public class ScheduleChangedReadModel : IScheduleChangedReadModel
	{
		public Guid Person { get; set; }
		public DateOnly Date { get; set; }
	}
}