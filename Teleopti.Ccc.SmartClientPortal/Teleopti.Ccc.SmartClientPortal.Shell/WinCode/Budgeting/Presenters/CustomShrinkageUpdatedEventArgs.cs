using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
    public class CustomShrinkageUpdatedEventArgs : EventArgs
    {
        public ICustomShrinkage CustomShrinkage { get; set; }
    }
}