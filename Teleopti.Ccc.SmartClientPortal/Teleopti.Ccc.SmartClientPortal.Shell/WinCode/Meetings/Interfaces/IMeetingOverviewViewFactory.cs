using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces
{
    public interface IMeetingOverviewViewFactory
    {
        IMeetingOverviewView Create(IEnumerable<Guid> selectedPersons, DateOnlyPeriod selectedPeriod, IScenario scenario);

        void ShowMeetingComposerView(IPersonSelectorView parent, IMeetingViewModel meetingViewModel, bool viewSchedulesPermission);
    }
}