using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public class ZoomButtonsEventArgs : EventArgs
    {
        public WorkingInterval Interval { get; set; }

        public TemplateTarget Target { get; set; }

        public string GridKey { get; set; }
    }
}