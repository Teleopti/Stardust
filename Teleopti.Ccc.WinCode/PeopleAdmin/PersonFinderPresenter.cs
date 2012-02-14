﻿using System;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
    public class PersonFinderPresenter
    {
        private readonly IPersonFinderView _view;
        public IPersonFinderModel Model { get; private set; }

        public PersonFinderPresenter(IPersonFinderView view, IPersonFinderModel model)
        {
            _view = view;
            Model = model;
        }

        public void Find(IExecutableCommand command)
        {
            if(command == null) throw new ArgumentNullException("command");

            command.Execute();
            _view.AddRows(Model.SearchCriteria.DisplayRows);
            _view.UpdatePageOfStatusText();
            _view.UpdatePreviousNextStatus();
            _view.UpdateButtonOkStatus();
        }

        public int SortColumn { get; set; }
        public SortOrder SortOrder { get; set; }

        public void ListViewColumnClick(int column)
        {
            if (column == SortColumn)
            {
                SortOrder = SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                SortColumn = column;
                SortOrder = SortOrder.Ascending;
            }
            
            Model.SearchCriteria.SortColumn = column;
            Model.SearchCriteria.SortDirection = (int)SortOrder;

            var command = new PersonFinderFindCommand(Model);
            _view.AttemptDatabaseConnectionFind(command);
        }

        public void ButtonFindClick(IExecutableCommand command)
        {
            Model.SearchCriteria = _view.PersonFinderSearchCriteria;
            _view.AttemptDatabaseConnectionFind(command);    
        }

        public void ComboBoxRowsPerPageSelectedIndexChanged(IExecutableCommand command)
        {
            Model.SearchCriteria = _view.PersonFinderSearchCriteria;
            _view.AttemptDatabaseConnectionFind(command);
        }

        public void LinkLabelPreviousLinkClicked(IExecutableCommand command)
        {
            Model.SearchCriteria = _view.PersonFinderSearchCriteriaNextPrevious;
            _view.AttemptDatabaseConnectionFind(command);     
        }

        public void LinkLabelNextLinkClicked(IExecutableCommand command)
        {
            Model.SearchCriteria = _view.PersonFinderSearchCriteriaNextPrevious;
            _view.AttemptDatabaseConnectionFind(command); 
        }
    }
}
