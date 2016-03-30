using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public static class CommandScope
	{
		[ThreadStatic]
		private static ICommandIdentifier _commandIdentifier;

		public static IDisposable Create(ICommandIdentifier commandIdentifier)
		{
			_commandIdentifier = commandIdentifier;
			return new GenericDisposable(() => _commandIdentifier = null);
		}

		public static ICommandIdentifier Current()
		{
			return _commandIdentifier;
		}
	}
}