using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.Rta.Persisters
{
	public class TeamOutOfAdherenceReadModelPersister : ITeamOutOfAdherenceReadModelPersister, ITeamOutOfAdherenceReadModelReader
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
			var fromVersion = model.Version;
			var toVersion = model.Version + 1;
			_unitOfWork.Current().CreateSqlQuery(
				"MERGE ReadModel.TeamOutOfAdherence AS T " +
				"USING (" + 
				"	VALUES" +
				"	(" +
				"		:TeamId, " +
				"		:SiteId, " +
				"		:FromVersion," +
				"		:ToVersion," +
				"		:Count, " +
				"		:State" +
				"	)" +
				") AS S (" +
				"	TeamId, " +
				"	SiteId, " +
				"	FromVersion," +
				"	ToVersion," +
				"	Count, " +
				"	[State]" +
				") " +
				"ON T.TeamId = S.TeamId AND" +
				"	T.[Version] = S.FromVersion " +
				"WHEN NOT MATCHED THEN " +
				"	INSERT " +
				"	(" +
				"		TeamId," +
				"		SiteId," +
				"		[Version]," +
				"		Count," +
				"		[State]" +
				"	) VALUES (" +
				"		S.TeamId," +
				"		S.SiteId," +
				"		S.ToVersion," +
				"		S.Count," +
				"		S.[State]" +
				"	) " +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		[Version] = S.ToVersion," +
				"		SiteId = S.SiteId," +
				"		Count = S.Count," +
				"		[State] = S.State " +
				";")
				.SetParameter("TeamId", model.TeamId)
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("FromVersion", fromVersion)
				.SetParameter("ToVersion", toVersion)
				.SetParameter("State", _serializer.SerializeObject(model.State),NHibernateUtil.StringClob)
				.SetParameter("Count", model.Count)
				.ExecuteUpdate();
			model.Version = toVersion;
		}

		public TeamOutOfAdherenceReadModel Get(Guid teamId)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"	TeamId, " +
					"	SiteId, " +
					"	Count, " +
					"	State as StateJson, " +
					"	[Version] " +
					"FROM ReadModel.TeamOutOfAdherence WITH (UPDLOCK) " +
					"WHERE TeamId =:TeamId")
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.AddScalar("Version", NHibernateUtil.Int32)
				.SetParameter("TeamId", teamId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List()
				.Cast<internalModel>()
				.Select(m =>
				{
					m.State = _deserializer.DeserializeObject<TeamOutOfAdherenceReadModelState[]>(m.StateJson);
					m.StateJson = null;
					return m;
				})
				.SingleOrDefault();
		}

		public IEnumerable<TeamOutOfAdherenceReadModel> GetAll()
		{
			return _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"	TeamId, " +
					"	SiteId, " +
					"	Count, " +
					"	State as StateJson, " +
					"	[Version] " +
					"FROM ReadModel.TeamOutOfAdherence  WITH (UPDLOCK) ")
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.AddScalar("Version", NHibernateUtil.Int32)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List()
				.Cast<internalModel>()
				.Select(m =>
				{
					m.State = _deserializer.DeserializeObject<TeamOutOfAdherenceReadModelState[]>(m.StateJson);
					m.StateJson = null;
					return m;
				})
				.ToArray();
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

		public IEnumerable<TeamOutOfAdherenceReadModel> Read(Guid siteId)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"		TeamId, " +
					"		Count " +
					"FROM " +
					"		ReadModel.TeamOutOfAdherence " +
					"WHERE " +
					"		SiteId =:SiteId")
				.AddScalar("TeamId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.SetParameter("SiteId", siteId)
				.SetResultTransformer(Transformers.AliasToBean(typeof(TeamOutOfAdherenceReadModel)))
				.List()
				.Cast<TeamOutOfAdherenceReadModel>();
		}
	}
}