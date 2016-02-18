using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MessageBroker
{
	public class MailboxRepository : IMailboxRepository
	{
		private readonly ICurrentMessageBrokerUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;
		private readonly INow _now;

		public MailboxRepository(
			ICurrentMessageBrokerUnitOfWork unitOfWork,
			IJsonSerializer serializer,
			IJsonDeserializer deserializer,
			INow now
			)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
			_now = now;
		}

		public void Persist(Mailbox model)
		{
			Debug.WriteLine("Persist" + model.Messages.Count());
			_unitOfWork.Current().CreateSqlQuery(
				"MERGE INTO [msg].Mailbox AS T " +
				"USING (" +
				"	VALUES " +
				"	(" +
				"		:Id, " +
				"		:Route, " +
				"		:Notifications" +
				"	)" +
				") AS S (" +
				"		Id, " +
				"		Route, " +
				"		Notifications" +
				"	) " +
				"ON " +
				"	T.Id = S.Id AND " +
				"	T.Route = S.Route " +
				"WHEN NOT MATCHED THEN " +
				"	INSERT " +
				"	(" +
				"		Id," +
				"		Route," +
				"		Notifications, " +
				"		ExpiresAt " +
				"	) VALUES (" +
				"		S.Id," +
				"		S.Route," +
				"		S.Notifications, " +
				"		:ExpiresAt " +
				"	) " +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		Notifications = S.Notifications," +
				"		ExpiresAt = :ExpiresAt" +
				";")
				.SetGuid("Id", model.Id)
				.SetString("Route", model.Route)
				.SetParameter("Notifications", _serializer.SerializeObject(model.Messages), NHibernateUtil.StringClob)
				.SetParameter("ExpiresAt", model.ExpiresAt.NullIfMinValue())
				.ExecuteUpdate();
		}

		public Mailbox Load(Guid id)
		{
			return load(id, null).SingleOrDefault();
		}
		
		public IEnumerable<Mailbox> Load(string[] routes)
		{
			return load(null, routes);
		}

		private IEnumerable<Mailbox> load(Guid? id, string[] routes)
		{
			var where = "Id =:Id";
			if (routes != null)
				where = "Route IN (:Routes)";

			var sql = string.Format(
				"SELECT " +
				"	Id," +
				"	Route," +
				"	Notifications AS NotificationsJson, " +
				"	ExpiresAt " +
				"FROM [msg].Mailbox WHERE" +
				"	{0} ", where);

			var query = _unitOfWork.Current().CreateSqlQuery(sql)
				.AddScalar("Id", NHibernateUtil.Guid)
				.AddScalar("Route", NHibernateUtil.String)
				.AddScalar("NotificationsJson", NHibernateUtil.StringClob)
				.AddScalar("ExpiresAt", NHibernateUtil.DateTime)
				.SetResultTransformer(Transformers.AliasToBean(typeof(getModel)))
				;

			if (routes != null)
				query.SetParameterList("Routes", routes);
			else
				query.SetGuid("Id", id.Value);

			var result = query.List<getModel>();
			result.ForEach(m => m.ParseJson(_deserializer));
			return result;
		}

		private class getModel : Mailbox
		{
			public void ParseJson(IJsonDeserializer deserializer)
			{
				_messages = deserializer.DeserializeObject<List<Message>>(NotificationsJson);
				NotificationsJson = null;
			}

			public string NotificationsJson { private get; set; }
		}

		public void Purge()
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"DELETE FROM [msg].Mailbox " +
				"WHERE ExpiresAt <= :utcDateTime;")
				.SetParameter("utcDateTime", _now.UtcDateTime())
				.ExecuteUpdate();
			Debug.WriteLine("Purge " + result);
		}

	}
}