using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.ReadModelUnitOfWork;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.Infrastructure.Rta.Persisters
{
	public class SiteOutOfAdherenceReadModelPersister : ISiteOutOfAdherenceReadModelPersister, ISiteOutOfAdherenceReadModelReader
	{
		private readonly ICurrentReadModelUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public SiteOutOfAdherenceReadModelPersister(ICurrentReadModelUnitOfWork unitOfWork, IJsonSerializer serializer, IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		public void Persist(SiteOutOfAdherenceReadModel model)
		{
			//var data = _serializer.SerializeObject(model.State)
			_unitOfWork.Current().CreateSqlQuery(
				"MERGE ReadModel.SiteOutOfAdherence AS T " +
				"USING (" +
				"	VALUES" +
				"	(" +
				"		:SiteId, " +
				"		:BusinessUnitId, " +
				"		:Count, " +
				"		:State" +
				"	)" +
				") AS S (" +
				"	SiteId," +
				"	BusinessUnitId," +
				"	Count," +
				"	[State]" +
				") " +
				"ON T.SiteId = S.SiteId " +
				"WHEN NOT MATCHED THEN " +
				"	INSERT " +
				"	(" +
				"		SiteId," +
				"		BusinessUnitId," +
				"		Count," +
				"		[State]" +
				"	) VALUES (" +
				"		S.SiteId," +
				"		S.BusinessUnitId," +
				"		S.Count," +
				"		S.State" +
				"	) " +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		BusinessUnitId = S.BusinessUnitId," +
				"		Count = S.Count," +
				"		[State] = S.State " +
				";")
				.SetParameter("SiteId", model.SiteId)
				.SetParameter("BusinessUnitId", model.BusinessUnitId)
				.SetParameter("Count", model.Count)
				.SetParameter("State", _serializer.SerializeObject(model.State), NHibernateUtil.StringClob)
				.ExecuteUpdate();
		}

		public SiteOutOfAdherenceReadModel Get(Guid siteId)
		{
			return _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"	SiteId," +
					"	BusinessUnitId," +
					"	Count," +
					"	[State] AS StateJson " +
					"FROM ReadModel.SiteOutOfAdherence " +
					"WHERE SiteId =:SiteId")
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetParameter("SiteId", siteId)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List()
				.Cast<internalModel>()
				.Select(m =>
				{
					m.State = _deserializer.DeserializeObject<SiteOutOfAdherenceReadModelState[]>(m.StateJson);
					m.StateJson = null;
					return m;
				})
				.SingleOrDefault();
		}

		public IEnumerable<SiteOutOfAdherenceReadModel> GetAll()
		{
			return _unitOfWork.Current()
				.CreateSqlQuery(
					"SELECT " +
					"	SiteId," +
					"	BusinessUnitId," +
					"	Count," +
					"	[State] AS StateJson " +
					"FROM ReadModel.SiteOutOfAdherence ")
				.AddScalar("SiteId", NHibernateUtil.Guid)
				.AddScalar("BusinessUnitId", NHibernateUtil.Guid)
				.AddScalar("Count", NHibernateUtil.Int32)
				.AddScalar("StateJson", NHibernateUtil.StringClob)
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List()
				.Cast<internalModel>()
				.Select(m =>
				{
					m.State = _deserializer.DeserializeObject<SiteOutOfAdherenceReadModelState[]>(m.StateJson);
					m.StateJson = null;
					return m;
				})
				.ToArray();
		}

		public IEnumerable<SiteOutOfAdherenceReadModel> Read()
		{
			return _unitOfWork.Current()
			.CreateSqlQuery(
				"SELECT " +
				"SiteId," +
				"Count " +
				"FROM ReadModel.SiteOutOfAdherence")
			.AddScalar("SiteId", NHibernateUtil.Guid)
			.AddScalar("Count", NHibernateUtil.Int32)
			.SetResultTransformer(Transformers.AliasToBean(typeof(SiteOutOfAdherenceReadModel)))
			.List()
			.Cast<SiteOutOfAdherenceReadModel>();
		}

		public void Clear()
		{
			_unitOfWork.Current().CreateSqlQuery("DELETE FROM ReadModel.SiteOutOfAdherence ").ExecuteUpdate();
		}

		public bool HasData()
		{
			return (int) _unitOfWork.Current().CreateSqlQuery("SELECT COUNT(*) FROM ReadModel.SiteOutOfAdherence ").UniqueResult() > 0;
		}

		private class internalModel : SiteOutOfAdherenceReadModel
		{
			public string StateJson { get; set; }
		}
	}

	
}