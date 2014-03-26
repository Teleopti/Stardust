using System;
using System.Timers;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Rta.Server.Repeater
{
	public class MinuteTrigger : IMessageRepeaterTrigger
	{
		private readonly IConfigReader _configReader;
		public const string RepeatIntervalKey = "RepeatIntervalMinutes";

		public MinuteTrigger(IConfigReader configReader)
		{
			_configReader = configReader;
		}

		public void Initialize(Action flush)
		{
			var ms = fetchMillisecondsFromConfig();
			var timer = new Timer(ms);
			timer.Elapsed += (sender, args) => flush();
			timer.Enabled = true;
		}

		private int fetchMillisecondsFromConfig()
		{
			int ms;
			var appSetting = _configReader.AppSettings[RepeatIntervalKey];
			if (appSetting.Equals("extremelyLow"))
			{
				ms = 1;
			}
			else
			{
				ms = int.Parse(_configReader.AppSettings[RepeatIntervalKey])*1000*60;
				if (ms < 1000*60)
					throw new InvalidOperationException(string.Format("{0} must be positive!", RepeatIntervalKey));
			}
			return ms;
		}
	}
}