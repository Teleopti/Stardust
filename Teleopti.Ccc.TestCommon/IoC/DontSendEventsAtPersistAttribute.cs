using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	/// <summary>
	/// there are some bugs when sending events at persist in tests, so this is just a workaround to prevent that
	/// if you don't care about these events
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DontSendEventsAtPersistAttribute : Attribute
	{
	}
}