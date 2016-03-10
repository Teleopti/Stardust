using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.SystemSetting;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	[Serializable]
	public class ScheduleChangeSubscriptions : SettingValue
	{
		public const string Key = "ScheduleChangeSubscriptions";

		private readonly IList<ScheduleChangeListener> subscriptions = new List<ScheduleChangeListener>();

		public void Add(ScheduleChangeListener listener)
		{
			subscriptions.Add(listener);
		}

		public void Remove(ScheduleChangeListener listener)
		{
			subscriptions.Remove(listener);
		}

		public ScheduleChangeListener[] Subscriptions()
		{
			return subscriptions.ToArray();
		}
	}
}