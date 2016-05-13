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

	public class QueueName
	{
		public const string HighPriority = "high";
		public const string DefaultPriority = "default";
		public const string LowPriority = "low";

		public static string[] All()
		{
			return new[] {HighPriority, DefaultPriority, LowPriority};
		}
	}
}