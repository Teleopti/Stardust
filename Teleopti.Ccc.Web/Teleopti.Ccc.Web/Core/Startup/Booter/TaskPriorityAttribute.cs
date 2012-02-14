using System;

namespace Teleopti.Ccc.Web.Core.Startup.Booter
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class TaskPriorityAttribute : Attribute
	{
		public TaskPriorityAttribute(int priority)
		{
			Priority = priority;
		}

		public int Priority { get; private set; }
	}
}