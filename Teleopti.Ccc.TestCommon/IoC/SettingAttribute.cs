using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class SettingAttribute : Attribute
	{
		public string Setting { get; }
		public string Value { get; }

		public SettingAttribute(string setting, string value)
		{
			Setting = setting;
			Value = value;
		}

		public SettingAttribute(string setting, bool value)
		{
			Setting = setting;
			Value = value.ToString();
		}
	}
}