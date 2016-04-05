using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IInitiatorIdentifierScope
	{
		IDisposable OnThisThreadUse(IInitiatorIdentifier initiator);
	}
}