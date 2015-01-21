using System;

namespace Teleopti.Ccc.TestCommon
{
	public static class Exceptions
	{
		public static void Ignore(Action action)
		{
			try
			{
				action.Invoke();
			}
			catch (Exception)
			{
			}
		}
	}
}