//using System;
//using System.Collections.Generic;
//using NHibernate.Transform;
//using Teleopti.Ccc.Domain.MessageBroker;
//using Teleopti.Ccc.Domain.MessageBroker.Server;
//using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.Infrastructure.MessageBroker
//{
//	public class MailboxRepository2 : IMailboxRepository
//	{
//		private readonly ICurrentMessageBrokerUnitOfWork _unitOfWork;
//		private readonly INow _now;

//		public MailboxRepository2(ICurrentMessageBrokerUnitOfWork unitOfWork, INow now)
//		{
//			_unitOfWork = unitOfWork;
//			_now = now;
//		}

//		public void Persist(Mailbox mailbox)
//		{
//			// make sure row exists in Mailbox
//			var updated = _unitOfWork.Current().CreateSqlQuery($@"
//				UPDATE [msg].Mailbox 
//					SET ExpiresAt = :{nameof(Mailbox.ExpiresAt)}
//				WHERE Id = :{nameof(Mailbox.Id)} AND Route = :{nameof(Mailbox.Route)}")
//				.SetGuid(nameof(Mailbox.Id), mailbox.Id)
//				.SetString(nameof(Mailbox.Route), mailbox.Route)
//				.SetParameter(nameof(Mailbox.ExpiresAt), mailbox.ExpiresAt.NullIfMinValue())
//				.ExecuteUpdate();

//			if (updated == 0)
//			{
//				// do insert
//				_unitOfWork.Current().CreateSqlQuery($@"
//					INSERT INTO [msg].Mailbox VALUES(:{nameof(Mailbox.Id)}, :{nameof(Mailbox.Route)}, '[]', :{nameof(Mailbox.ExpiresAt)})")
//					.SetGuid(nameof(Mailbox.Id), mailbox.Id)
//					.SetString(nameof(Mailbox.Route), mailbox.Route)
//					.SetParameter(nameof(Mailbox.ExpiresAt), mailbox.ExpiresAt.NullIfMinValue())
//					.ExecuteUpdate();
//			}

//			foreach (var notification in mailbox.Messages)
//			{
//				_unitOfWork.Current().CreateSqlQuery($@"
//					INSERT INTO [msg].MailboxNotification VALUES(
//						:{nameof(InternalMessage.MailboxId)}, 
//						:{nameof(Message.DataSource)},
//						:{nameof(Message.BusinessUnitId)},
//						:{nameof(Message.DomainType)},
//						:{nameof(Message.DomainQualifiedType)},
//						:{nameof(Message.DomainId)},
//						:{nameof(Message.ModuleId)},
//						:{nameof(Message.DomainReferenceId)},
//						:{nameof(Message.EndDate)},
//						:{nameof(Message.StartDate)},
//						:{nameof(Message.DomainUpdateType)},
//						:{nameof(Message.BinaryData)},
//						:{nameof(Message.TrackId)})")
//					.SetGuid(nameof(InternalMessage.MailboxId), mailbox.Id)
//					.SetParameter(nameof(Message.DataSource), notification.DataSource)
//					.SetParameter(nameof(Message.BusinessUnitId), notification.BusinessUnitId)
//					.SetParameter(nameof(Message.DomainType), notification.DomainType)
//					.SetParameter(nameof(Message.DomainQualifiedType), notification.DomainQualifiedType)
//					.SetParameter(nameof(Message.DomainId), notification.DomainId)
//					.SetParameter(nameof(Message.ModuleId), notification.ModuleId)
//					.SetParameter(nameof(Message.DomainReferenceId), notification.DomainReferenceId)
//					.SetParameter(nameof(Message.EndDate), notification.EndDate)
//					.SetParameter(nameof(Message.StartDate), notification.StartDate)
//					.SetParameter(nameof(Message.DomainUpdateType), notification.DomainUpdateType)
//					.SetParameter(nameof(Message.BinaryData), notification.BinaryData)
//					.SetParameter(nameof(Message.TrackId), notification.TrackId)
//					.ExecuteUpdate();
//			}
//		}

//		public Mailbox Load(Guid id)
//		{
//			var mailbox = _unitOfWork.Current().CreateSqlQuery($@"
//					SELECT Id, Route, ExpiresAt FROM [msg].Mailbox 
//					WHERE Id = :{nameof(Mailbox.Id)}
//				")
//				.SetGuid(nameof(Mailbox.Id), id)
//				.SetResultTransformer(Transformers.AliasToBean<Mailbox>())
//				.UniqueResult<Mailbox>();

//			addNotifications(mailbox);

//			return mailbox;
//		}

//		private void addNotifications(Mailbox mailbox)
//		{
//			if (mailbox == null) return;
//			var notifications = _unitOfWork.Current().CreateSqlQuery($@"
//					DELETE FROM [msg].MailboxNotification
//					OUTPUT DELETED.*
//					WHERE MailboxId = :{nameof(InternalMessage.MailboxId)}	
//				")
//				.SetGuid(nameof(InternalMessage.MailboxId), mailbox.Id)
//				.SetResultTransformer(Transformers.AliasToBean<InternalMessage>())
//				.List<Message>();
//			foreach (var notification in notifications)
//			{
//				mailbox.AddMessage(notification);
//			}
//		}

//		public IEnumerable<Mailbox> Load(string[] routes)
//		{
//			var mailboxes = _unitOfWork.Current().CreateSqlQuery($@"
//					SELECT Id, Route, ExpiresAt FROM [msg].Mailbox 
//					WHERE Route IN (:{nameof(routes)})
//				")
//				.SetParameterList(nameof(routes), routes)
//				.SetResultTransformer(Transformers.AliasToBean<Mailbox>())
//				.List<Mailbox>();
//			foreach (var mailbox in mailboxes)
//				addNotifications(mailbox);
//			return mailboxes;
//		}

//		public void Purge()
//		{
//			var utcDateTime = _now.UtcDateTime();
//			_unitOfWork.Current().CreateSqlQuery(
//				$"DELETE FROM [msg].Mailbox WITH (TABLOCK) WHERE ExpiresAt <= :{nameof(utcDateTime)};")
//				.SetParameter(nameof(utcDateTime), utcDateTime)
//				.ExecuteUpdate();
//		}

//		private class InternalMessage : Message
//		{
//			public Guid MailboxId { get; set; }
//		}
//	}
//}