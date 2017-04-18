using System;
using System.Collections.Generic;
using System.Drawing;
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

        public DateTime StartTime
        {
            get { return _startTime; }
        }

        public DateTime EndTime
        {
            get { return _endTime; }
        }

        public Color Color
        {
            get { return _color; }
        }

        public string Name
        {
            get { return _name; }
        }

        public bool Validate()
        {
            return true;
        }

        public string UnderlyingActivities
        {get { return _underlyingActivities; }
        }

        public void SetUnderlyingActivities(IList<IActivity> activities)
        {
            if (activities == null) throw new ArgumentNullException("activities");
            // loop through and create a string for tooltip

            _underlyingActivities = "\n" + UserTexts.Resources.MasterActivityCanResultInColon;

            foreach (var activity in activities)
            {
                _underlyingActivities += "\n" + activity.Name;
            }
        }
    }
}
