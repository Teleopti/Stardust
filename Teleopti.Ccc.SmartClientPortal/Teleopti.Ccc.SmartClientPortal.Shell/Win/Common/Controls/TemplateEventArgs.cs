using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls
{
    public class TemplateEventArgs : EventArgs
    {
        public string TemplateName { get; set; }
        public TemplateTarget TemplateTarget { get; set; }
    }
}
