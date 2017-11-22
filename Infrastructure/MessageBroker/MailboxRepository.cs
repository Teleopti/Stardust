using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;

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

		public void Add(Mailbox model)
		{
			persist(model, Enumerable.Empty<Message>());
		}

		public Mailbox Load(Guid id)
		{
			return load(id, null).SingleOrDefault();
		}

		public IEnumerable<Message> PopMessages(Guid id, DateTime? expiredAt)
		{
			var mailbox = load(id, null).SingleOrDefault();
			if (mailbox == null)
				return Enumerable.Empty<Message>();
			var messages = mailbox.Notifications;
			if (messages.Any() || expiredAt.HasValue)
			{
				mailbox.ExpiresAt = expiredAt ?? mailbox.ExpiresAt;
				persist(mailbox, Enumerable.Empty<Message>());
			}
			return messages;
		}

		public void AddMessage(Message message)
		{
			load(null, message.Routes())
				.ForEach(mailbox => persist(mailbox, mailbox.Notifications.Append(message).ToArray()));
		}

		private void persist(Mailbox model, IEnumerable<Message> notifications)
		{
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
				.SetParameter("Notifications", _serializer.SerializeObject(notifications), NHibernateUtil.StringClob)
				.SetParameter("ExpiresAt", model.ExpiresAt.NullIfMinValue())
				.ExecuteUpdate();
		}

		private IEnumerable<mailboxWithNotifications> load(Guid? id, string[] routes)
		{
			var where = "Id =:Id";
			if (routes != null)
				where = "Route IN (:Routes)";

			var sql =
					"SELECT " +
					"	Id," +
					"	Route," +
					"	Notifications AS NotificationsJson, " +
					"	ExpiresAt " +
					"FROM [msg].Mailbox WHERE" +
					$"	{where} "
				;

			var query = _unitOfWork.Current().CreateSqlQuery(sql)
				.AddScalar("Id", NHibernateUtil.Guid)
				.AddScalar("Route", NHibernateUtil.String)
				.AddScalar("NotificationsJson", NHibernateUtil.StringClob)
				.AddScalar("ExpiresAt", NHibernateUtil.DateTime)
				.SetResultTransformer(Transformers.AliasToBean(typeof(mailboxWithNotifications)))
				;

			if (routes != null)
				query.SetParameterList("Routes", routes);
			else
				query.SetGuid("Id", id.Value);

			var result = query.List<mailboxWithNotifications>();
			result.ForEach(m => m.ParseJson(_deserializer));
			return result;
		}

		private class mailboxWithNotifications : Mailbox
		{
			public IEnumerable<Message> Notifications { get; set; }
			public string NotificationsJson { get; set; }

			public void ParseJson(IJsonDeserializer deserializer)
			{
				Notifications = deserializer.DeserializeObject<List<Message>>(NotificationsJson);
				NotificationsJson = null;
			}
		}

		public void Purge()
		{
			_unitOfWork.Current().CreateSqlQuery(
				"DELETE FROM [msg].Mailbox WITH (TABLOCK) " +
				"WHERE ExpiresAt <= :utcDateTime;")
				.SetParameter("utcDateTime", _now.UtcDateTime())
				.ExecuteUpdate();
		}

	}
}