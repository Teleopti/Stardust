using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Server;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;

namespace Teleopti.Ccc.Infrastructure.ScheduleChangeMessage
{
	public class PersonScheduleChangeMessageRepository: IPersonScheduleChangeMessageRepository
	{
		private readonly ICurrentMessageBrokerUnitOfWork _unitOfWork;
		private readonly INow _now;

		public PersonScheduleChangeMessageRepository(ICurrentMessageBrokerUnitOfWork unitOfWork, INow now)
		{
			_unitOfWork = unitOfWork;
			_now = now;
		}

		public void Add(PersonScheduleChangeMessage scheduleChangeMessage)
		{
			_unitOfWork.Current().CreateSqlQuery($@"
						INSERT INTO [msg].PersonScheduleChangeMessage (
					[{nameof(scheduleChangeMessage.Id)}]
					 ,[{nameof(scheduleChangeMessage.StartDate)}]
					 ,[{nameof(scheduleChangeMessage.EndDate)}]
					 ,[{nameof(scheduleChangeMessage.PersonId)}]
					 ,[{nameof(scheduleChangeMessage.TimeStamp)}])
					VALUES 
					(:{nameof(scheduleChangeMessage.Id)},
					:{nameof(scheduleChangeMessage.StartDate)},
					:{nameof(scheduleChangeMessage.EndDate)}, 
					:{nameof(scheduleChangeMessage.PersonId)}, 
					:{nameof(scheduleChangeMessage.TimeStamp)})")
				.SetGuid(nameof(scheduleChangeMessage.Id), scheduleChangeMessage.Id)
				.SetDateTime(nameof(scheduleChangeMessage.StartDate), scheduleChangeMessage.StartDate)
				.SetDateTime(nameof(scheduleChangeMessage.EndDate), scheduleChangeMessage.EndDate)
				.SetGuid(nameof(scheduleChangeMessage.PersonId), scheduleChangeMessage.PersonId)
				.SetDateTime(nameof(scheduleChangeMessage.TimeStamp), scheduleChangeMessage.TimeStamp)
				.ExecuteUpdate();
		}

		public IList<PersonScheduleChangeMessage> PopMessages(Guid userId)
		{
			return _unitOfWork.Current().CreateSqlQuery($@"
						DELETE FROM [msg].PersonScheduleChangeMessage
						OUTPUT DELETED.*
						WHERE PersonId = :{nameof(PersonScheduleChangeMessage.PersonId)}	
					")
				.SetGuid(nameof(PersonScheduleChangeMessage.PersonId), userId)
				.SetResultTransformer(Transformers.AliasToBean<PersonScheduleChangeMessage>())
				.List<PersonScheduleChangeMessage>();
		}
	}
}
