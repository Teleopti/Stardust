using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class CurrentScheduleReadModelPersister : IScheduleReader, ICurrentScheduleReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public CurrentScheduleReadModelPersister(
			ICurrentReadModelUnitOfWork unitOfWork, 
			IJsonSerializer serializer, 
			IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		public IEnumerable<ScheduledActivity> Read()
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT Schedule FROM ReadModel.CurrentSchedule")
				.List<string>()
				.SelectMany(x => _deserializer.DeserializeObject<IEnumerable<ScheduledActivity>>(x ?? "[]"))
				.ToArray();
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
			if (schedule.IsEmpty())
			{
				_unitOfWork.Current()
					.CreateSqlQuery("DELETE ReadModel.CurrentSchedule WHERE PersonId = :PersonId")
					.SetParameter("PersonId", personId)
					.ExecuteUpdate();
				return;
			}
			var serializedSchedule = _serializer.SerializeObject(schedule);

			var updated = _unitOfWork.Current()
				.CreateSqlQuery("UPDATE ReadModel.CurrentSchedule SET Schedule = :Schedule, Valid = 1 WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetParameter("Schedule", serializedSchedule, NHibernateUtil.StringClob)
				.ExecuteUpdate();

			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery("INSERT INTO ReadModel.CurrentSchedule (PersonId, Schedule, Valid) VALUES (:PersonId, :Schedule, 1)")
					.SetParameter("PersonId", personId)
					.SetParameter("Schedule", serializedSchedule, NHibernateUtil.StringClob)
					.ExecuteUpdate();
		}
	}
}