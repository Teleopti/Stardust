using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class CurrentScheduleReadModelPersister : IScheduleReader, ICurrentScheduleReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;

		public CurrentScheduleReadModelPersister(
			ICurrentReadModelUnitOfWork unitOfWork,
			IJsonSerializer serializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
		}

		public IEnumerable<CurrentSchedule> Read(int? lastUpdate)
		{
			var sql = "SELECT PersonId, LastUpdate, Schedule FROM ReadModel.CurrentSchedule WITH (NOLOCK)";
			if (lastUpdate.HasValue)
				return _unitOfWork.Current()
					.CreateSqlQuery($"{sql} WHERE LastUpdate > :LastUpdate")
					.SetParameter("LastUpdate", lastUpdate.Value)
					.SetResultTransformer(Transformers.AliasToBean<internalCurrentSchedule>())
					.List<CurrentSchedule>();

			return _unitOfWork.Current()
				.CreateSqlQuery(sql)
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
				.CreateSqlQuery(@"
UPDATE ReadModel.CurrentSchedule 
SET
	Schedule = :Schedule,
	Valid = 1,
	LastUpdate = CASE WHEN LastUpdate IS NULL THEN 1 ELSE LastUpdate + 1 END
WHERE
	PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetParameter("Schedule", serializedSchedule, NHibernateUtil.StringClob)
				.ExecuteUpdate();

			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery("INSERT INTO ReadModel.CurrentSchedule (PersonId, Schedule, LastUpdate, Valid) VALUES (:PersonId, :Schedule, 1, 1)")
					.SetParameter("PersonId", personId)
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