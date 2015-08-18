using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class SettingAttribute : Attribute
	{
		public string Setting { get; private set; }
		public string Value { get; private set; }

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

		public SettingAttribute(string setting, int value)
		{
			Setting = setting;
			Value = value.ToString();
		}
	}
}