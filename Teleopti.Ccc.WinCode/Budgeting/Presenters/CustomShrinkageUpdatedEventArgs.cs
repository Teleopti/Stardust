using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting.Presenters
{
    public class CustomShrinkageUpdatedEventArgs : EventArgs
    {
        public ICustomShrinkage CustomShrinkage { get; set; }
    }
}