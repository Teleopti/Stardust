using System;
using Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeMessageBrokerUnitOfWorkScope : IMessageBrokerUnitOfWorkScope
	{
		public void Start()
		{
		}

		public void End(Exception exception)
		{
		}
	}
}