using System;
using System.Collections.Concurrent;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class DesktopContext
	{
		private readonly ConcurrentDictionary<Guid, DesktopContextStateData> _contextPerCommand = new ConcurrentDictionary<Guid, DesktopContextStateData>();

		public DesktopContextStateData CurrentContext()
		{
			var currentScope = CommandScope.Current();
			return currentScope == null ? null : _contextPerCommand[currentScope.CommandId];
		}

		public IDisposable SetContextFor(ICommandIdentifier commandIdentifier, DesktopContextStateData contextStateData)
		{
			_contextPerCommand[commandIdentifier.CommandId] = contextStateData;
			return new GenericDisposable(() =>
			{
				_contextPerCommand.TryRemove(commandIdentifier.CommandId, out _);
			});
		}
	}
}