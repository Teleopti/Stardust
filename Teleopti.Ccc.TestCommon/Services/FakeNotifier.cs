﻿using System.Collections.Generic;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Interfaces.Domain;

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

		public void Notify(INotificationMessage messages, IPerson[] persons)
		{
			persons.ForEach(person =>
			{
				SentMessages.Add(new SentMessage
				{
					Message = messages,
					Person = person
				});
			});
		}

		public class SentMessage
		{
			public INotificationMessage Message { get; set; }
			public IPerson Person { get; set; }
		}
	}
}