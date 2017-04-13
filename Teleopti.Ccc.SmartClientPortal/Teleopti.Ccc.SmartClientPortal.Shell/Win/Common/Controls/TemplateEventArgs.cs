using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls
{
    public class TemplateEventArgs : EventArgs
    {
        public string TemplateName { get; set; }
        public TemplateTarget TemplateTarget { get; set; }
    }
}
