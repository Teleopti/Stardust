using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakePushMessagePersister: IPushMessagePersister
	{
		private readonly List<IPerson> _persons = new List<IPerson>();

		private IPushMessage _message;

		public ISendPushMessageReceipt Add(IPushMessage pushMessage, IEnumerable<IPerson> receivers)
		{
			_persons.AddRange(receivers);
			_message = pushMessage;
			return null;
		}

		public IList<IPerson> GetReceivers()
		{
			return _persons;
		}

		public IPushMessage GetMessage()
		{
			return _message;
		}

		public void Remove(IPushMessage pushMessage)
		{
			throw new NotImplementedException();
		}
	}
}
