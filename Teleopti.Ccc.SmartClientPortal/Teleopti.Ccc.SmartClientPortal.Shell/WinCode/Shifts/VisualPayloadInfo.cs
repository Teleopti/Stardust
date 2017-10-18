using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts
{
    public struct VisualPayloadInfo : IValidate
    {
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private readonly Color _color;
        private readonly string _name;
        private string _underlyingActivities;

        public VisualPayloadInfo(DateTime startTime, DateTime endTime, Color color, string name)
        {
            _startTime = startTime;
            _endTime = endTime;
            _color = color;
            _name = name;
            _underlyingActivities = "";
        }

        public DateTime StartTime => _startTime;

		public DateTime EndTime => _endTime;

		public Color Color => _color;

		public string Name => _name;

		public bool Validate()
        {
            return true;
        }

        public string UnderlyingActivities => _underlyingActivities;

		public void SetUnderlyingActivities(IList<IActivity> activities)
        {
            if (activities == null) throw new ArgumentNullException(nameof(activities));
            
			_underlyingActivities = string.Concat("\n", UserTexts.Resources.MasterActivityCanResultInColon, "\n",
				string.Join("\n", activities.Select(a => a.Name)));
        }
    }
}
