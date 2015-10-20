using System;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeCurrentInitiatorIdentifier : ICurrentInitiatorIdentifier
	{
		private IInitiatorIdentifier _initiatorIdentifier;

		public FakeCurrentInitiatorIdentifier()
			: this(new EmptyInitiatorIdentifier())
		{
		}

		public FakeCurrentInitiatorIdentifier(IInitiatorIdentifier initiatorIdentifier)
		{
			_initiatorIdentifier = initiatorIdentifier;
		}

		public IInitiatorIdentifier Current()
		{
			return _initiatorIdentifier;
		}

		public void InitiatorIdentifier(Guid initiator)
		{
			_initiatorIdentifier = new FakeInitiatorIdentifier { InitiatorId = initiator };
		}
	}
}