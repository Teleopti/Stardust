using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface ISchedulingScreenSettings
    {
        Guid? DefaultScheduleTag { get; set; }
        IList<string> QuickAccessButtons { get; }
        TimeSpan EditorSnapToResolution { get; set; }
        bool HideEditor { get; set; }
        bool HideGraph { get; set; }
        bool HideResult { get; set; }
        bool HideRibbonTexts { get; set; }
        IList<Guid> PinnedSkillTabs { get; }
    }
}