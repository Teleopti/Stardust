using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public static class TrackIdentifierScope
	{
		[ThreadStatic]
		private static ITrackInfo _commandIdentifier;

		public static IDisposable Create(ITrackInfo commandIdentifier)
		{
			_commandIdentifier = commandIdentifier;
			return new GenericDisposable(() => _commandIdentifier = null);
		}

		public static ITrackInfo Current()
		{
			return _commandIdentifier;
		}
	}
}