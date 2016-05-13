using System;
using System.Linq;

namespace Teleopti.Interfaces.Messages
{
	/// <summary>
	/// Inherit on your Hangfire event handler to set high priority
	/// </summary>
	public interface IRunWithHighPriority
	{
	}
	/// <summary>
	/// Inherit on your Hangfire event handler to set default priority
	/// </summary>
	public interface IRunWithDefaultPriority
	{
	}
	/// <summary>
	/// Inherit on your Hangfire event handler to set low priority
	/// </summary>
	public interface IRunWithLowPriority
	{
	}

	public enum Priority
	{
		High,
		Default,
		Low
	}

	public class QueueName
	{
		public static string[] All()
		{
			var values = from Priority e in Enum.GetValues(typeof (Priority))
				select For(e);
			return values.ToArray();
		}

		public static string For(Priority e)
		{
			return e.ToString().ToLower();
		}
	}
}