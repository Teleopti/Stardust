using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IInitiatorIdentifierScope
	{
		IDisposable OnThisThreadUse(IInitiatorIdentifier initiator);
	}
}