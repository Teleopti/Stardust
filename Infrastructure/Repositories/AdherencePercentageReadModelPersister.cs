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
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public AdherencePercentageReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
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
			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.AdherencePercentage SET" +
				"			LastTimestamp = :LastTimestamp," +
				"				IsLastTimeInAdherence = :IsLastTimeInAdherence," +
				"					TimeInAdherence = :TimeInAdherence," +
				"						TimeOutOfAdherence = :TimeOutOfAdherence" +
				"  WHERE PersonId = :PersonId AND BelongsToDate =:Date")
							  .SetGuid("PersonId", model.PersonId)
							  .SetDateTime("Date", model.BelongsToDate)
							  .SetDateTime("LastTimestamp", model.LastTimestamp)
							  .SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
							  .SetParameter("TimeInAdherence", model.TimeInAdherence)
							  .SetParameter("TimeOutOfAdherence", model.TimeOutOfAdherence)
							  .ExecuteUpdate();
		}

		private void saveReadModel(AdherencePercentageReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.AdherencePercentage (PersonId,BelongsToDate,LastTimestamp,IsLastTimeInAdherence,TimeInAdherence,TimeOutOfAdherence)" +
					" VALUES (:PersonId,:Date,:LastTimestamp,:IsLastTimeInAdherence,:TimeInAdherence,:TimeOutOfAdherence)")
							  .SetGuid("PersonId", model.PersonId)
							  .SetDateTime("Date", model.BelongsToDate)
							  .SetDateTime("LastTimestamp", model.LastTimestamp)
							  .SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
							  .SetTimeSpan("TimeInAdherence", model.TimeInAdherence)
							  .SetTimeSpan("TimeOutOfAdherence", model.TimeOutOfAdherence)
							  .ExecuteUpdate();
		}


		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT PersonId, BelongsToDate as DATE,LastTimestamp,IsLastTimeInAdherence,TimeInAdherence,TimeOutOfAdherence  FROM ReadModel.AdherencePercentage WHERE PersonId =:PersonId and BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("LastTimestamp", NHibernateUtil.DateTime)
				.AddScalar("IsLastTimeInAdherence", NHibernateUtil.Boolean)
				.AddScalar("TimeInAdherence", NHibernateUtil.TimeSpan)
				.AddScalar("TimeOutOfAdherence", NHibernateUtil.TimeSpan)
				.SetGuid("PersonId", personId)
				.SetDateTime("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AdherencePercentageReadModel)))
				.List<AdherencePercentageReadModel>();
			return (AdherencePercentageReadModel) result.FirstOrNull();
		}

		
	}
}