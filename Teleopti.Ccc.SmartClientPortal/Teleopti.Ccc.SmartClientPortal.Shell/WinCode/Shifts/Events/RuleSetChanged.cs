using System.Collections.Generic;
using Microsoft.Practices.Composite.Presentation.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Events
{
    public class RuleSetChanged : CompositePresentationEvent<IList<IWorkShiftRuleSet>>
    {
        
    }
}