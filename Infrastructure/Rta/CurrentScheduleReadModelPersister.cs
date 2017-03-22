using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class CurrentScheduleReadModelPersister : IScheduleReader, ICurrentScheduleReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly INow _now;
		private readonly IJsonSerializer _serializer;

		public CurrentScheduleReadModelPersister(
			ICurrentReadModelUnitOfWork unitOfWork,
			INow now,
			IJsonSerializer serializer)
		{
			_unitOfWork = unitOfWork;
			_now = now;
			_serializer = serializer;
		}

		public IEnumerable<CurrentSchedule> Read(DateTime? updatedAfter)
		{
			IQuery query;
			if (updatedAfter.HasValue)
				query = _unitOfWork.Current()
					.CreateSqlQuery("SELECT PersonId, Schedule FROM ReadModel.CurrentSchedule WHERE UpdatedAt >= :UpdatedAt")
					.SetParameter("UpdatedAt", updatedAfter.Value);
			else
				query = _unitOfWork.Current()
					.CreateSqlQuery("SELECT PersonId, Schedule FROM ReadModel.CurrentSchedule");

			return query
				.SetResultTransformer(Transformers.AliasToBean<internalCurrentSchedule>())
				.List<CurrentSchedule>();
		}

		public void Invalidate(Guid personId)
		{
			var updated = _unitOfWork.Current()
				.CreateSqlQuery("UPDATE ReadModel.CurrentSchedule SET Valid = 0 WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.ExecuteUpdate();

			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery("INSERT INTO ReadModel.CurrentSchedule (PersonId, Valid) VALUES (:PersonId, 0)")
					.SetParameter("PersonId", personId)
					.ExecuteUpdate();
		}

		public IEnumerable<Guid> GetInvalid()
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT PersonId FROM ReadModel.CurrentSchedule WHERE Valid = 0")
				.List<Guid>();
		}

		public void Persist(Guid personId, IEnumerable<ScheduledActivity> schedule)
		{
			var serializedSchedule = _serializer.SerializeObject(schedule);

			var updated = _unitOfWork.Current()
				.CreateSqlQuery("UPDATE ReadModel.CurrentSchedule SET Schedule = :Schedule, UpdatedAt = :UpdatedAt, Valid = 1 WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetParameter("UpdatedAt", _now.UtcDateTime())
				.SetParameter("Schedule", serializedSchedule, NHibernateUtil.StringClob)
				.ExecuteUpdate();

			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery("INSERT INTO ReadModel.CurrentSchedule (PersonId, Schedule, UpdatedAt, Valid) VALUES (:PersonId, :Schedule, :UpdatedAt, 1)")
					.SetParameter("PersonId", personId)
					.SetParameter("UpdatedAt", _now.UtcDateTime())
					.SetParameter("Schedule", serializedSchedule, NHibernateUtil.StringClob)
					.ExecuteUpdate();
		}

		private class internalCurrentSchedule : CurrentSchedule
		{
			public new string Schedule
			{
				set { base.Schedule = JsonConvert.DeserializeObject<IEnumerable<ScheduledActivity>>(value ?? "[]"); }
			}
		}
	}
}