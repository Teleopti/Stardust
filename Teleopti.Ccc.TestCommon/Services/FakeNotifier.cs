using System.Collections.Generic;
using System.Threading.Tasks;
using NHibernate.Util;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;

namespace Teleopti.Ccc.TestCommon.Services
{
	public class FakeNotifier : INotifier
	{
		public List<SentMessage> SentMessages = new List<SentMessage>(); 
		public void Notify(INotificationMessage messages, IPerson person)
		{
			SentMessages.Add(new SentMessage
			{
				Message = messages,
				Person = person
			});
		}

		public Task<bool> Notify(INotificationMessage messages, IPerson[] persons)
		{
			persons.ForEach(person =>
			{
				SentMessages.Add(new SentMessage
				{
					Message = messages,
					Person = person
				});
			});

			return Task.FromResult(false);
		}

		public class SentMessage
		{
			public INotificationMessage Message { get; set; }
			public IPerson Person { get; set; }
		}
	}
}