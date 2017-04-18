using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters
{
    public class CustomEfficiencyShrinkageUpdatedEventArgs : EventArgs
    {
        public Guid? Id { get; set; }
        public string ShrinkageName { get; set; }
        public bool IncludedInAllowance { get; set; }
    }
}