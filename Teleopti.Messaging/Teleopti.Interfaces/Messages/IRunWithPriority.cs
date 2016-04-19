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

	public class QueueNames
	{
		public static string[] From<T>()
		{
			Type t = typeof (T);
			if (t.IsEnum)
			{
				var values = from Enum e in Enum.GetValues(t)
					select e.ToString().ToLower();
				return values.ToArray();
			}
			return null;
		}
	}
}