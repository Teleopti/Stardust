using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IInitiatorIdentifierScope
	{
		IDisposable OnThisThreadUse(IInitiatorIdentifier initiator);
	}
}