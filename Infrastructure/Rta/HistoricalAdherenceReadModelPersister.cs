using System;
using NHibernate;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalAdherenceReadModelPersister : IHistoricalAdherenceReadModelPersister {

		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;

		public HistoricalAdherenceReadModelPersister(ICurrentUnitOfWork unitOfWork, IJsonSerializer serializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
		}

		[LogInfo]
		public virtual void Persist(HistoricalAdherenceReadModel model)
		{
			var update = _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
					UPDATE [ReadModel].[HistoricalAdherence]
					SET
						AgentName = :AgentName,
						[Date] = :Date,
						Schedules = :Schedules,
						OutOfAdherences = :OutOfAdherences
					WHERE 
						PersonId = :PersonId
				")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("Date", model.Date.Date)
				.SetParameter("OutOfAdherences", _serializer.SerializeObject(model.OutOfAdherences), NHibernateUtil.StringClob)
				.ExecuteUpdate();

			if (update == 0) 
				_unitOfWork.Current().Session().CreateSQLQuery(@"
					INSERT INTO [ReadModel].[HistoricalAdherence]
					(
						PersonId,
						AgentName,
						[Date],
						Schedules,
						OutOfAdherences
					) VALUES (
						:PersonId,
						:AgentName,
						:Date,
						:Schedules,
						:OutOfAdherences
					)
				")
				.SetParameter("PersonId", model.PersonId)
				.SetParameter("Date", model.Date.Date)
				.SetParameter("OutOfAdherences", _serializer.SerializeObject(model.OutOfAdherences), NHibernateUtil.StringClob)
				.ExecuteUpdate();
		}

		public void SetInAdherence(Guid personId)
		{
			throw new NotImplementedException();
		}

		public void SetNeutralAdherence(Guid personId)
		{
			throw new NotImplementedException();
		}

		public void SetOutOfAdherence(Guid personId)
		{
			throw new NotImplementedException();
		}

		public void UpdateSchedule(Guid personId)
		{
			throw new NotImplementedException();
		}
	}

}