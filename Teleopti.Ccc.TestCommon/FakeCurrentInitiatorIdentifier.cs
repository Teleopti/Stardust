using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

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