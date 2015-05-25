using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Infrastructure.MessageBroker
{
	public class MailboxRepository : IMailboxRepository
	{
		private readonly ICurrentMessageBrokerUnitOfWork _currentMessageBrokerUnitOfWork;
		private readonly IJsonSerializer _serializer;
		private readonly IJsonDeserializer _deserializer;

		public MailboxRepository(ICurrentMessageBrokerUnitOfWork currentMessageBrokerUnitOfWork, IJsonSerializer serializer, IJsonDeserializer deserializer)
		{
			_currentMessageBrokerUnitOfWork = currentMessageBrokerUnitOfWork;
			_serializer = serializer;
			_deserializer = deserializer;
		}

		[MessageBrokerUnitOfWork]
		public void Persist(Mailbox mailbox)
		{
			var builder = new StringBuilder();
			IQuery sqlQuery;

			var existingMailbox = Get(mailbox.Id);
			if (existingMailbox == null)
			{
				builder.Append(@"INSERT INTO [dbo].[Mailbox] VALUES (:Id, :Route) " + Environment.NewLine);

				var parameters = prepareNotificationSqlAndGetParameterList(mailbox, mailbox.Notifications, builder);
				sqlQuery = _currentMessageBrokerUnitOfWork.Current()
					.CreateSqlQuery(builder.ToString())
					.SetParameter("Id", mailbox.Id)
					.SetParameter("Route", mailbox.Route);
				setParameters(parameters, sqlQuery);
			}
			else
			{
				if (!mailbox.Notifications.Any())
					sqlQuery = deleteNotifications(mailbox.Id);
				else
				{
					var notificationAdded = mailbox.Notifications.Where(x => !existingMailbox.Notifications.Contains(x)).ToList();
					var parameters = prepareNotificationSqlAndGetParameterList(mailbox, notificationAdded, builder);
					sqlQuery = _currentMessageBrokerUnitOfWork.Current()
						.CreateSqlQuery(builder.ToString());
					setParameters(parameters, sqlQuery);
				}
			}
			sqlQuery.ExecuteUpdate();
		}

		private List<Pair<Guid, string>> prepareNotificationSqlAndGetParameterList(Mailbox mailbox, IEnumerable<Notification> notifications, StringBuilder sql)
		{
			var parameters = new List<Pair<Guid, string>>();
			for (var i = 0; i < notifications.Count(); i++)
			{
				sql.Append(string.Format(@"INSERT INTO [dbo].[Notification] VALUES (:Id{0}, :Notification{1})" + Environment.NewLine,
					i, i));
				parameters.Add(new Pair<Guid, string>(mailbox.Id, _serializer.SerializeObject(mailbox.Notifications.ElementAt(i))));
			}
			return parameters;
		}

		private static void setParameters(IList<Pair<Guid, string>> parameters, IQuery sqlQuery)
		{
			for (var i = 0; i < parameters.Count(); i++)
			{
				sqlQuery
					.SetParameter("Id" + i, parameters[i].First)
					.SetParameter("Notification" + i, parameters[i].Second);
			}
		}

		private IQuery deleteNotifications(Guid id)
		{
			var sql = (@"DELETE FROM [dbo].[Notification] WHERE Parent = :IdToRemove" + Environment.NewLine);
			return _currentMessageBrokerUnitOfWork.Current()
				.CreateSqlQuery(sql)
				.SetParameter("IdToRemove", id);
		}

		private const string selectSql = @"SELECT Mailbox.Id, Mailbox.Route, Notification.Message FROM [dbo].[Mailbox] LEFT OUTER JOIN [dbo].[Notification] ON Mailbox.Id = Notification.Parent ";

		[MessageBrokerUnitOfWork]
		public Mailbox Get(Guid id)
		{
			var result = get(selectSql + " WHERE Mailbox.Id = :Id", new parameter {Name = "Id", Value = id});
			return result == null
				? null
				: result.First();
		}

		[MessageBrokerUnitOfWork]
		public IEnumerable<Mailbox> Get(string route)
		{
			return get(selectSql + " WHERE Mailbox.Route = :Route", new parameter {Name = "Route", Value = route}) ?? Enumerable.Empty<Mailbox>();
		}

		private IEnumerable<Mailbox> get(string sql, parameter param)
		{
			var result =  _currentMessageBrokerUnitOfWork.Current()
				.CreateSqlQuery(sql)
				.SetParameter(param.Name, param.Value)
				.SetResultTransformer(Transformers.AliasToBean<mailboxThing>())
				.List<mailboxThing>();

			if (!result.Any())
				return null;
			return result
				.Select(x => new {x.Id, x.Route})
				.Distinct()
				.Select(mailBoxIdRoutes => createMailbox(mailBoxIdRoutes.Id, mailBoxIdRoutes.Route, result));
		}

		private Mailbox createMailbox(Guid id, string route, IEnumerable<mailboxThing> result)
		{
			var mailbox = new Mailbox { Id = id, Route = route };
			foreach (var mailboxThing in result.Where(mailboxThing => mailboxThing.Message != null))
				mailbox.AddNotification(_deserializer.DeserializeObject<Notification>(mailboxThing.Message));
			return mailbox;
		}

		private class parameter
		{
			public string Name { get; set; }
			public object Value { get; set; }
		}

		private class mailboxThing
		{
			public Guid Id { get; set; }
			public string Route { get; set; }
			public string Message { get; set; }
		}
	}
}