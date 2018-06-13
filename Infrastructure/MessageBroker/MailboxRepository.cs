using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.MessageBroker
{
	public class MailboxRepository : IMailboxRepository
	{
		private readonly ICurrentMessageBrokerUnitOfWork _unitOfWork;
		private readonly INow _now;

		public MailboxRepository(ICurrentMessageBrokerUnitOfWork unitOfWork, INow now)
		{
			_unitOfWork = unitOfWork;
			_now = now;
		}

		public void Add(Mailbox mailbox)
		{
			_unitOfWork.Current().CreateSqlQuery($@"
						INSERT INTO [msg].Mailbox VALUES(:{nameof(Mailbox.Id)}, :{nameof(Mailbox.Route)}, '[]', :{nameof(Mailbox.ExpiresAt)})")
				.SetGuid(nameof(Mailbox.Id), mailbox.Id)
				.SetString(nameof(Mailbox.Route), mailbox.Route)
				.SetParameter(nameof(Mailbox.ExpiresAt), mailbox.ExpiresAt.NullIfMinValue())
				.ExecuteUpdate();
		}

		public Mailbox Load(Guid id)
		{
			return _unitOfWork.Current().CreateSqlQuery($@"
						SELECT Id, Route, ExpiresAt FROM [msg].Mailbox 
						WHERE Id = :{nameof(Mailbox.Id)}
					")
				.SetGuid(nameof(Mailbox.Id), id)
				.SetResultTransformer(Transformers.AliasToBean<Mailbox>())
				.UniqueResult<Mailbox>();
		}

		public IEnumerable<Message> PopMessages(Guid id, DateTime? expiredAt)
		{
			if (expiredAt.HasValue)
				_unitOfWork.Current().CreateSqlQuery($@"
					UPDATE [msg].Mailbox 
						SET ExpiresAt = :{nameof(Mailbox.ExpiresAt)}
					WHERE Id = :{nameof(Mailbox.Id)}")
					.SetGuid(nameof(Mailbox.Id), id)
					.SetParameter(nameof(Mailbox.ExpiresAt), expiredAt.Value)
					.ExecuteUpdate();

			return _unitOfWork.Current().CreateSqlQuery($@"
						DELETE FROM [msg].MailboxNotification
						OUTPUT DELETED.*
						WHERE MailboxId = :{nameof(InternalMessage.MailboxId)}	
					")
				.SetGuid(nameof(InternalMessage.MailboxId), id)
				.SetResultTransformer(Transformers.AliasToBean<InternalMessage>())
				.List<Message>();
		}

		public void AddMessage(Message message)
		{
			_unitOfWork.Current().CreateSqlQuery($@"
					INSERT INTO [msg].MailboxNotification SELECT 
						m.Id, 
						:{nameof(Message.DataSource)},
						:{nameof(Message.BusinessUnitId)},
						:{nameof(Message.DomainType)},
						:{nameof(Message.DomainQualifiedType)},
						:{nameof(Message.DomainId)},
						:{nameof(Message.ModuleId)},
						:{nameof(Message.DomainReferenceId)},
						:{nameof(Message.EndDate)},
						:{nameof(Message.StartDate)},
						:{nameof(Message.DomainUpdateType)},
						:{nameof(Message.BinaryData)},
						:{nameof(Message.TrackId)}
						FROM [msg].Mailbox m 
						WHERE m.Route IN (:{nameof(message.Routes)})")
				.SetParameter(nameof(Message.DataSource), message.DataSource)
				.SetParameter(nameof(Message.BusinessUnitId), message.BusinessUnitId)
				.SetParameter(nameof(Message.DomainType), message.DomainType)
				.SetParameter(nameof(Message.DomainQualifiedType), message.DomainQualifiedType)
				.SetParameter(nameof(Message.DomainId), message.DomainId)
				.SetParameter(nameof(Message.ModuleId), message.ModuleId)
				.SetParameter(nameof(Message.DomainReferenceId), message.DomainReferenceId)
				.SetParameter(nameof(Message.EndDate), message.EndDate)
				.SetParameter(nameof(Message.StartDate), message.StartDate)
				.SetParameter(nameof(Message.DomainUpdateType), message.DomainUpdateType)
				.SetParameter(nameof(Message.BinaryData), message.BinaryData, NHibernateUtil.StringClob)
				.SetParameter(nameof(Message.TrackId), message.TrackId)
				.SetParameterList(nameof(message.Routes), message.Routes())
				.ExecuteUpdate();
		}

		public void Purge()
		{
			var utcDateTime = _now.UtcDateTime();
			_unitOfWork.Current().CreateSqlQuery(
					$"DELETE FROM [msg].Mailbox WITH (TABLOCKX) WHERE ExpiresAt <= :{nameof(utcDateTime)};")
				.SetParameter(nameof(utcDateTime), utcDateTime)
				.ExecuteUpdate();
			_unitOfWork.Current().CreateSqlQuery(
					"DELETE mn FROM [msg].MailboxNotification mn LEFT JOIN [msg].Mailbox m ON mn.MailboxId = m.Id WHERE m.Id IS NULL")
				.ExecuteUpdate();
		}

		private class InternalMessage : Message
		{
			public Guid MailboxId { get; set; }
			public int Id { get; set; }
		}
	}
}