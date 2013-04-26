﻿using System;
using System.Collections.Generic;
using NHibernate.Transform;
using NHibernate.Util;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.ReadModel;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public interface IEtlReadModelRepository
	{
		IList<IScheduleChangedReadModel> ChangedDataOnStep(DateTime afterDate, IBusinessUnit currentBusinessUnit, string stepName);
		ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName);
		void UpdateLastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTime thisTime);
	}
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

		public ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName)
		{
			var res = ((NHibernateUnitOfWork)_currentUnitOfWork).Session.CreateSQLQuery(
					"exec mart.[LastChangedDateOnStep] @stepName=:step, @buId=:bu ")
					.SetGuid("bu", currentBusinessUnit.Id.GetValueOrDefault())
					.SetString("step", stepName)
					.SetResultTransformer(Transformers.AliasToBean(typeof(LastChangedReadModel)))
					.SetReadOnly(true)
					.List<ILastChangedReadModel>();
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