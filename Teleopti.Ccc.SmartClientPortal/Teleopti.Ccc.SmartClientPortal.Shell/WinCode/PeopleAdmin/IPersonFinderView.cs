using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin
{
    public interface IPersonFinderView
    {
        IList<Guid> SelectedPersonGuids { get; }
        IPeoplePersonFinderSearchCriteria PersonFinderSearchCriteria { get; }
        IPeoplePersonFinderSearchCriteria PersonFinderSearchCriteriaNextPrevious { get; }
        void AddRows(IList<IPersonFinderDisplayRow> rows);
        void UpdatePageOfStatusText();
        void UpdatePreviousNextStatus();
        void UpdateButtonOkStatus();
        //void SortListView(int column, SortOrder sortOrder);
        void AttemptDatabaseConnectionFind(IExecutableCommand command);
    }
}
