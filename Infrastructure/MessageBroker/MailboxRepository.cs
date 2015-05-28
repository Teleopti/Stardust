using System;
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

		public MailboxRepository(ICurrentMessageBrokerUnitOfWork unitOfWork, IJsonSerializer serializer, IJsonDeserializer deserializer)
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
				"		Notifications " +
				"	) VALUES (" +
				"		:Id," +
				"		:Route," +
				"		:Notifications " +
				"	) " +
				"WHEN MATCHED THEN " +
				"	UPDATE SET" +
				"		Notifications = :Notifications" +
				";")
				.SetGuid("Id", model.Id)
				.SetString("Route", model.Route)
				.SetParameter("Notifications", _serializer.SerializeObject(model.Notifications), NHibernateUtil.StringClob)
				.ExecuteUpdate();
		}

		public Mailbox Load(Guid id)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	Id," +
				"	Route," +
				"	Notifications AS NotificationsJson " +
				"FROM [msg].Mailbox WHERE" +
				"	Id =:Id ")
				.AddScalar("Id", NHibernateUtil.Guid)
				.AddScalar("Route", NHibernateUtil.String)
				.AddScalar("NotificationsJson", NHibernateUtil.StringClob)
				.SetGuid("Id", id)
				.SetResultTransformer(Transformers.AliasToBean(typeof(getModel)))
				.List<getModel>()
				.SingleOrDefault();

			if (result == null) return null;
			if (result.NotificationsJson == null) return result;

			result.Notifications = _deserializer.DeserializeObject<Notification[]>(result.NotificationsJson);
			result.NotificationsJson = null;

			return result;
		}
		
		public IEnumerable<Mailbox> Load(string[] routes)
		{
			var result = _unitOfWork.Current().CreateSqlQuery(
				"SELECT " +
				"	Id," +
				"	Route," +
				"	Notifications AS NotificationsJson " +
				"FROM [msg].Mailbox WHERE" +
				"	Route IN (:Routes) ")
				.AddScalar("Id", NHibernateUtil.Guid)
				.AddScalar("Route", NHibernateUtil.String)
				.AddScalar("NotificationsJson", NHibernateUtil.StringClob)
				.SetParameterList("Routes", routes)
				.SetResultTransformer(Transformers.AliasToBean(typeof(getModel)))
				.List<getModel>();

			result.ForEach(m =>
			{
				if (m.NotificationsJson != null) return;
				m.Notifications = _deserializer.DeserializeObject<Notification[]>(m.NotificationsJson);
				m.NotificationsJson = null;
			});

			return result;
		}

		private class getModel : Mailbox
		{
			public string NotificationsJson { get; set; }
		}

	}

}