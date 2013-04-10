using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class AgentPreferencePresenter
	{
		private readonly IAgentPreferenceView _view;
		private readonly IScheduleDay _scheduleDay;

		public AgentPreferencePresenter(IAgentPreferenceView view, IScheduleDay scheduleDay)
		{
			_view = view;
			_scheduleDay = scheduleDay;
		}

		public IAgentPreferenceView View
		{
			get { return _view; }
		}

		public IScheduleDay ScheduleDay
		{
			get { return _scheduleDay; }
		}

		public void UpdateView()
		{
			foreach (var persistableScheduleData in _scheduleDay.PersistableScheduleDataCollection())
			{
				if (!(persistableScheduleData is IPreferenceDay)) continue;
				var preferenceRestriction = ((IPreferenceDay) persistableScheduleData).Restriction;
				_view.Update(preferenceRestriction);
				break;
			}
	
			_view.Update(null);
		}
	}
}
