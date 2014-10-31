using System;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AdherencePercentageReadModelPersister : IAdherencePercentageReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _currentUnitOfWork;

		public AdherencePercentageReadModelPersister(ICurrentReadModelUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public void Persist(AdherencePercentageReadModel model)
		{
			var existingReadModel = Get(model.BelongsToDate, model.PersonId);
			if (existingReadModel == null)
			{
				saveReadModel(model);
			}
			else
			{
				updateReadModel(model);
			}
		}

		private void updateReadModel(AdherencePercentageReadModel model)
		{
			_currentUnitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.AdherencePercentage SET" +
				"	MinutesInAdherence = :MinutesInAdherence," +
				"		MinutesOutOfAdherence = :MinutesOutOfAdherence," +
				"			LastTimestamp = :LastTimestamp," +
				"				IsLastTimeInAdherence = :IsLastTimeInAdherence" +
				"  WHERE PersonId = :PersonId AND BelongsToDate =:Date")
							  .SetGuid("PersonId", model.PersonId)
							  .SetDateTime("Date", model.BelongsToDate)
							  .SetInt32("MinutesInAdherence", model.MinutesInAdherence)
							  .SetInt32("MinutesOutOfAdherence", model.MinutesOutOfAdherence)
							  .SetDateTime("LastTimestamp", model.LastTimestamp)
							  .SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
							  .ExecuteUpdate();
		}

		private void saveReadModel(AdherencePercentageReadModel model)
		{
			_currentUnitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.AdherencePercentage (PersonId,BelongsToDate,MinutesInAdherence,MinutesOutOfAdherence,LastTimestamp,IsLastTimeInAdherence)" +
					" VALUES (:PersonId,:Date,:MinutesInAdherence,:MinutesOutOfAdherence,:LastTimestamp,:IsLastTimeInAdherence)")
							  .SetGuid("PersonId", model.PersonId)
							  .SetDateTime("Date", model.BelongsToDate)
							  .SetInt32("MinutesInAdherence", model.MinutesInAdherence)
							  .SetInt32("MinutesOutOfAdherence", model.MinutesOutOfAdherence)
							  .SetDateTime("LastTimestamp", model.LastTimestamp)
							  .SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
							  .ExecuteUpdate();
		}


		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			var result = _currentUnitOfWork.Current().CreateSqlQuery(
				"SELECT PersonId, BelongsToDate as DATE,LastTimestamp,MinutesInAdherence,MinutesOutOfAdherence,IsLastTimeInAdherence  FROM ReadModel.AdherencePercentage WHERE PersonId =:PersonId and BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("LastTimestamp", NHibernateUtil.DateTime)
				.AddScalar("MinutesInAdherence", NHibernateUtil.Int16)
				.AddScalar("MinutesOutOfAdherence", NHibernateUtil.Int16)
				.AddScalar("IsLastTimeInAdherence", NHibernateUtil.Boolean)
				.SetGuid("PersonId", personId)
				.SetDateTime("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AdherencePercentageReadModel)))
				.List<AdherencePercentageReadModel>();
			return (AdherencePercentageReadModel) result.FirstOrNull();
		}

		
	}
}