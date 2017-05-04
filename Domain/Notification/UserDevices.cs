using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.Notification
{
	[Serializable]
	public class UserDevices : SettingValue
	{
		public const string Key = "DevicesUserToken";

		private readonly ICollection<string> tokens = new HashSet<string>();

		public void AddToken(string token)
		{
			tokens.Add(token);
		}

		public IEnumerable<string> TokenList => tokens;

		public void RemoveToken(params string[] ts)
		{
			foreach (var t in ts)
			{
				tokens.Remove(t);
			}
		}
	}
}