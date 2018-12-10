using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.ReadModel;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class EtlReadModelRepository : IEtlReadModelRepository
	{
		private readonly IUnitOfWork _currentUnitOfWork;

		public EtlReadModelRepository(IUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public IList<IScheduleChangedReadModel> ChangedDataOnStep(DateTime afterDate, IBusinessUnit currentBusinessUnit, string stepName)
		{
			return ((NHibernateUnitOfWork)_currentUnitOfWork).Session.CreateSQLQuery(
					"exec mart.[ChangedDataOnStep] :step, :afterdate, :bu ")
					.SetDateTime("afterdate", afterDate)
					.SetGuid("bu", currentBusinessUnit.Id.GetValueOrDefault())
					.SetString("step", stepName)
					.SetResultTransformer(Transformers.AliasToBean(typeof(ScheduleChangedReadModel)))
					.SetReadOnly(true)
					.List<IScheduleChangedReadModel>();
		}

        public void WorkAroundFor27636()
        {
            ((NHibernateUnitOfWork)_currentUnitOfWork).Session.CreateSQLQuery("exec dbo.WorkAroundFor27636");
        }

		public ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTimePeriod period)
		{
			var res = ((NHibernateUnitOfWork)_currentUnitOfWork).Session.CreateSQLQuery(
					"exec mart.[LastChangedDateOnStep] @stepName=:step, @buId=:bu, @start=:start, @end=:end ")
					.AddScalar("ThisTime", NHibernateUtil.DateTime)
					.AddScalar("LastTime", NHibernateUtil.DateTime)
					.SetGuid("bu", currentBusinessUnit.Id.GetValueOrDefault())
					.SetString("step", stepName)
					.SetDateTime("start", period.StartDateTime)
					.SetDateTime("end", period.EndDateTime)
					.SetResultTransformer(Transformers.AliasToBean(typeof(LastChangedReadModel)))
					.SetReadOnly(true)
					.List<ILastChangedReadModel>();
			_currentUnitOfWork.PersistAll(); 
			return res[0];
		}

		public void UpdateLastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTime thisTime)
		{
			((NHibernateUnitOfWork)_currentUnitOfWork).Session.CreateSQLQuery(
					"exec mart.[UpdateLastChangedDateOnStep] @stepName=:step, @buId=:bu, @thisTime=:thisTime ")
					.SetDateTime("thisTime", thisTime)
					.SetGuid("bu", currentBusinessUnit.Id.GetValueOrDefault())
					.SetString("step", stepName)
					.ExecuteUpdate();
		}
	}


	public class ScheduleChangedReadModel : IScheduleChangedReadModel
	{
		public Guid Person { get; set; }
		public DateTime Date { get; set; }
	}

	public class LastChangedReadModel : ILastChangedReadModel
	{
		public DateTime ThisTime { get; set; }
		public DateTime LastTime { get; set; }
	}
}