using System;
using System.Collections.Concurrent;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopContext
	{
		private readonly ConcurrentDictionary<Guid, IDesktopContextData> _contextPerCommand = new ConcurrentDictionary<Guid, IDesktopContextData>();

		public IDesktopContextData CurrentContext()
		{
			var currentScope = CommandScope.Current();
			return currentScope == null ? null : _contextPerCommand[currentScope.CommandId];
		}

		public IDisposable SetContextFor(ICommandIdentifier commandIdentifier, IDesktopContextData contextData)
		{
			_contextPerCommand[commandIdentifier.CommandId] = contextData;
			return new GenericDisposable(() =>
			{
				_contextPerCommand.TryRemove(commandIdentifier.CommandId, out IDesktopContextData _);
			});
		}
	}
}