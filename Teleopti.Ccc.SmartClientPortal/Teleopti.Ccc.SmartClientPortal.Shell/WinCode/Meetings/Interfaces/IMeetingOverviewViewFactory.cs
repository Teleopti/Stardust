using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Meetings.Interfaces
{
    public interface IMeetingOverviewViewFactory
    {
        IMeetingOverviewView Create(IEnumerable<Guid> selectedPersons, DateOnlyPeriod selectedPeriod, IScenario scenario);

        void ShowMeetingComposerView(IPersonSelectorView parent, IMeetingViewModel meetingViewModel, bool viewSchedulesPermission);
    }
}