﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public interface IPersonFinderView
    {
        IList<Guid> SelectedPersonGuids { get; }
        IPersonFinderSearchCriteria PersonFinderSearchCriteria { get; }
        IPersonFinderSearchCriteria PersonFinderSearchCriteriaNextPrevious { get; }
        void AddRows(IList<IPersonFinderDisplayRow> rows);
        void UpdatePageOfStatusText();
        void UpdatePreviousNextStatus();
        void UpdateButtonOkStatus();
        //void SortListView(int column, SortOrder sortOrder);
        void AttemptDatabaseConnectionFind(IExecutableCommand command);
    }
}
