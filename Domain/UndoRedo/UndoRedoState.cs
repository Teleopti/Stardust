using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.UndoRedo
{
	public class UndoRedoState
	{
		[ThreadStatic]
		private static IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public static IScheduleDayChangeCallback ScheduleDayChangeCallback => _scheduleDayChangeCallback;

		public static IDisposable Create(IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
			return new GenericDisposable(() =>
			{
				_scheduleDayChangeCallback = null;
			});
		}
	}
}