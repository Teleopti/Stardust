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
				"			IsLastTimeInAdherence = :IsLastTimeInAdherence," +
				"			TimeInAdherence = :TimeInAdherence," +
				"			TimeOutOfAdherence = :TimeOutOfAdherence," +
				"			ShiftHasEnded = :ShiftHasEnded " +
				"WHERE " +
				"	PersonId = :PersonId AND " +
				"	BelongsToDate =:Date")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("Date", model.BelongsToDate)
				.SetParameter("LastTimestamp", model.LastTimestamp)
				.SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
				.SetParameter("TimeInAdherence", model.TimeInAdherence)
				.SetParameter("TimeOutOfAdherence", model.TimeOutOfAdherence)
				.SetParameter("ShiftHasEnded", model.ShiftHasEnded)
				.ExecuteUpdate();
		}

		private void saveReadModel(AdherencePercentageReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.AdherencePercentage " +
				"(" +
				"	PersonId," +
				"	BelongsToDate," +
				"	LastTimestamp," +
				"	IsLastTimeInAdherence," +
				"	TimeInAdherence," +
				"	TimeOutOfAdherence," +
				"	ShiftHasEnded" +
				") VALUES (" +
				"	:PersonId," +
				"	:BelongsToDate," +
				"	:LastTimestamp," +
				"	:IsLastTimeInAdherence," +
				"	:TimeInAdherence," +
				"	:TimeOutOfAdherence," +
				"	:ShiftHasEnded" +
				")")
				.SetGuid("PersonId", model.PersonId)
				.SetDateTime("BelongsToDate", model.BelongsToDate)
				.SetParameter("LastTimestamp", model.LastTimestamp, NHibernateUtil.DateTime)
				.SetParameter("IsLastTimeInAdherence", model.IsLastTimeInAdherence)
				.SetTimeSpan("TimeInAdherence", model.TimeInAdherence)
				.SetTimeSpan("TimeOutOfAdherence", model.TimeOutOfAdherence)
				.SetParameter("ShiftHasEnded", model.ShiftHasEnded)
				.ExecuteUpdate();
		}

		public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	PersonId," +
				"	BelongsToDate AS Date," +
				"	LastTimestamp," +
				"	IsLastTimeInAdherence," +
				"	TimeInAdherence," +
				"	TimeOutOfAdherence," +
				"	ShiftHasEnded " +
				"FROM ReadModel.AdherencePercentage WHERE" +
				"	PersonId =:PersonId AND " +
				"	BelongsToDate =:Date ")
				.AddScalar("PersonId", NHibernateUtil.Guid)
				.AddScalar("Date", NHibernateUtil.DateTime)
				.AddScalar("LastTimestamp", NHibernateUtil.DateTime)
				.AddScalar("IsLastTimeInAdherence", NHibernateUtil.Boolean)
				.AddScalar("TimeInAdherence", NHibernateUtil.TimeSpan)
				.AddScalar("TimeOutOfAdherence", NHibernateUtil.TimeSpan)
				.AddScalar("ShiftHasEnded", NHibernateUtil.Boolean)
				.SetGuid("PersonId", personId)
				.SetDateTime("Date", date)
				.SetResultTransformer(Transformers.AliasToBean(typeof(AdherencePercentageReadModel)))
				.List<AdherencePercentageReadModel>();
			return (AdherencePercentageReadModel)result.FirstOrNull();
		}


	}

	public class TeamAdherencePersister : ITeamAdherencePersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;

		public TeamAdherencePersister(ICurrentReadModelUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(TeamAdherenceReadModel model)
		{
			var existingReadModel = Get(model.TeamId);
			if (existingReadModel == null)
			{
				saveReadModel(model);
			}
			else
			{
				updateReadModel(model);
			}
		}

		public TeamAdherenceReadModel Get(Guid teamId)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	TeamId," +
				"	AgentsOutOfAdherence " +
				"FROM ReadModel.TeamAdherence WHERE" +
				"	TeamId =:TeamId ")
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("AgentsOutOfAdherence", NHibernateUtil.Int16)
				.SetGuid("TeamId", teamId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(TeamAdherenceReadModel))).List();
			return (TeamAdherenceReadModel)result.FirstOrNull();
		}

		private void updateReadModel(TeamAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"INSERT INTO ReadModel.TeamAdherence (TeamId, AgentsOutOfAdherence) VALUES (:TeamId, :AgentsOutOfAdherence)")
							  .SetGuid("TeamId", model.TeamId)
							  .SetParameter("AgentsOutOfAdherence", model.AgentsOutOfAdherence)
							  .ExecuteUpdate();

			_unitOfWork.Current().CreateSqlQuery(
				"UPDATE ReadModel.TeamAdherence SET" +
				"			AgentsOutOfAdherence = :AgentsOutOfAdherence " +
				"WHERE " +
				"	TeamId = :TeamId")
				.SetGuid("TeamId", model.TeamId)
				.SetParameter("AgentsOutOfAdherence", model.AgentsOutOfAdherence)
				.ExecuteUpdate();
		}

		private void saveReadModel(TeamAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
			"INSERT INTO ReadModel.TeamAdherence (TeamId, AgentsOutOfAdherence) VALUES (:TeamId, :AgentsOutOfAdherence)")
						  .SetGuid("TeamId", model.TeamId)
						  .SetParameter("AgentsOutOfAdherence", model.AgentsOutOfAdherence)
						  .ExecuteUpdate();
		}
	}
}