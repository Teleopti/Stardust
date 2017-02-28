using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public class ZoomButtonsEventArgs : EventArgs
    {
        public WorkingInterval Interval { get; set; }

        public TemplateTarget Target { get; set; }

        public string GridKey { get; set; }
    }
}