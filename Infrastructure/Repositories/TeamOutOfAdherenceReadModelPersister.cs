using NHibernate;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class TeamOutOfAdherenceReadModelPersister : ITeamOutOfAdherenceReadModelPersister
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public TeamOutOfAdherenceReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork, IJsonSerializer serializer, IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		public void Persist(TeamOutOfAdherenceReadModel model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"MERGE ReadModel.TeamOutOfAdherence AS T " +
				"USING (VALUES(:TeamId)) AS S (TeamId) " +
				"ON T.TeamId = S.TeamId " +
				"WHEN NOT MATCHED THEN " +
				"	INSERT " +
				"	(" +
				"		TeamId," +
				"		SiteId," +
				"		Count," +
				"		[State]" +
				"	) VALUES (" +
				"		:TeamId," +
				"		:SiteId," +
				"		:Count," +
				"		:State" +
				"	) " +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		SiteId = :SiteId," +
				"		Count = :Count," +
				"		[State] = :State " +
				";")
				.SetParameter("TeamId", model.TeamId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("State", _serializer.SerializeObject(model.State))
				.SetParameter("Count", model.Count)
				.ExecuteUpdate();
		}

		public TeamOutOfAdherenceReadModel Get(Guid teamId)
		{
			var result = _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"		TeamId, " +
					"		SiteId, " +
					"		Count, " +
					"		State as StateJson " +
					"FROM " +
					"		ReadModel.TeamOutOfAdherence " +
					"WHERE " +
					"		TeamId =:TeamId")
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetParameter("TeamId", teamId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List()
				.Cast<internalModel>().SingleOrDefault();

			if (result == null) return null;
			result.State = _deserializer.DeserializeObject<TeamOutOfAdherenceReadModelState[]>(result.StateJson);
			return result;
		}

		public IEnumerable<TeamOutOfAdherenceReadModel> GetForSite(Guid siteId)
		{
			var models = _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"		TeamId, " +
					"		SiteId, " +
					"		Count, " +
					"		State as StateJson " +
					"FROM " +
					"		ReadModel.TeamOutOfAdherence " +
					"WHERE " +
					"		SiteId =:SiteId")
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetParameter("SiteId", siteId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalModel)))
				.List()
				.Cast<internalModel>();

			models.ForEach(model =>
			{
				model.State = _deserializer.DeserializeObject<TeamOutOfAdherenceReadModelState[]>(model.StateJson);
				model.StateJson = null;
			});
			return models;
		}

		public bool HasData()
		{
			return (int)_unitOfWork.Current().CreateSqlQuery("SELECT COUNT(*) FROM ReadModel.TeamOutOfAdherence ").UniqueResult() > 0;
		}

		public void Clear()
		{
			_unitOfWork.Current().CreateSqlQuery("DELETE FROM ReadModel.TeamOutOfAdherence ").ExecuteUpdate();
		}

		private class internalModel : TeamOutOfAdherenceReadModel
		{
			public string StateJson { get; set; }
		} 
	}
}