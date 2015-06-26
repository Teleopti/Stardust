﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Infrastructure.MessageBroker
{
	public class MailboxRepository : IMailboxRepository
	{
		private readonly ICurrentMessageBrokerUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public MailboxRepository(ICurrentMessageBrokerUnitOfWork unitOfWork,
			IJsonSerializer serializer,
			IJsonDeserializer deserializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		public void Persist(Mailbox model)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"MERGE INTO [msg].Mailbox AS T " +
				"USING (VALUES (:Id, :Route )) AS S (Id, Route) " +
				"ON T.Id = S.Id AND T.Route = S.Route " +
				"WHEN NOT MATCHED THEN " +
				"	INSERT " +
				"	(" +
				"		Id," +
				"		Route," +
				"		Notifications, " +
				"		ExpiresAt " +
				"	) VALUES (" +
				"		:Id," +
				"		:Route," +
				"		:Notifications, " +
				"		:ExpiresAt " +
				"	) " +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		Notifications = :Notifications," +
				"		ExpiresAt = :ExpiresAt" +
				";")
				.SetGuid("Id", model.Id)
				.SetString("Route", model.Route)
				.SetParameter("Notifications", _serializer.SerializeObject(model.Messages), NHibernateUtil.StringClob)
				.SetParameter("ExpiresAt", model.ExpiresAt)
				.ExecuteUpdate();
		}

		public Mailbox Load(Guid id)
		{
			return Load(id, null).SingleOrDefault();
		}
		
		public IEnumerable<Mailbox> Load(string[] routes)
		{
			return Load(null, routes);
		}

		public void Purge(DateTime utcDateTime)
		{
			_unitOfWork.Current().CreateSqlQuery(
				"DELETE FROM [msg].Mailbox "+
				"WHERE ExpiresAt < :utcDateTime;")
				.SetParameter("utcDateTime", utcDateTime)
				.ExecuteUpdate();
		}

		public IEnumerable<Mailbox> Load(Guid? id, string[] routes)
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

	}

}