using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Infrastructure.MessageBroker
{
	public class MailboxRepository : IMailboxRepository
	{
		private readonly ICurrentMessageBrokerUnitOfWork _unitOfWork;

		public MailboxRepository(ICurrentMessageBrokerUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public void Persist(Mailbox mailbox)
		{
			_unitOfWork.Current().Persist(mailbox);
		}

		public Mailbox Load(Guid id)
		{
			return _unitOfWork.Current()
				.NamedQuery("LoadMailbox")
				.SetParameter("id", id)
				.List<Mailbox>()
				.SingleOrDefault();
		}
		
		public IEnumerable<Mailbox> Load(string[] routes)
		{
			return _unitOfWork.Current()
					.NamedQuery("MailboxesOfRoutes")
					.SetParameterList("routes", routes)
					.List<Mailbox>();
		}
	}
}