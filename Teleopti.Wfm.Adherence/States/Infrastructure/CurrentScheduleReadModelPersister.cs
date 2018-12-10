using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
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

		public IEnumerable<CurrentSchedule> Read()
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT PersonId, Schedule FROM ReadModel.CurrentSchedule WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean<internalCurrentSchedule>())
				.List<CurrentSchedule>();
		}

		public IEnumerable<CurrentSchedule> Read(int fromRevision)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery("SELECT PersonId, Schedule FROM ReadModel.CurrentSchedule WITH (NOLOCK) WHERE Revision > :Revision")
				.SetParameter("Revision", fromRevision)
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

		public void Persist(Guid personId, int revision, IEnumerable<ScheduledActivity> schedule)
		{
			var serializedSchedule = _serializer.SerializeObject(schedule);

			var updated = _unitOfWork.Current()
				.CreateSqlQuery("UPDATE ReadModel.CurrentSchedule SET Schedule = :Schedule, Valid = 1, Revision = :Revision WHERE PersonId = :PersonId")
				.SetParameter("PersonId", personId)
				.SetParameter("Revision", revision)
				.SetParameter("Schedule", serializedSchedule, NHibernateUtil.StringClob)
				.ExecuteUpdate();

			if (updated == 0)
				_unitOfWork.Current()
					.CreateSqlQuery("INSERT INTO ReadModel.CurrentSchedule (PersonId, Schedule, Revision, Valid) VALUES (:PersonId, :Schedule, :Revision, 1)")
					.SetParameter("PersonId", personId)
					.SetParameter("Revision", revision)
					.SetParameter("Schedule", serializedSchedule, NHibernateUtil.StringClob)
					.ExecuteUpdate();
		}

		private class internalCurrentSchedule : CurrentSchedule
		{
			public new string Schedule
			{
				set
				{
					base.Schedule = value == null ? Enumerable.Empty<ScheduledActivity>() : JsonConvert.DeserializeObject<IEnumerable<ScheduledActivity>>(value); 
					
				}
			}
		}
	}
}