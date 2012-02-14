﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Events;
using Teleopti.Ccc.WinCode.Meetings.Events;
using Teleopti.Ccc.WinCode.Meetings.Overview;

namespace Teleopti.Ccc.Win.Meetings.Overview
{
   public interface IMeetingOverviewFilter
    {
        void Close();
        Point Location { get; set; }
        bool Visible { get; set; }
        void Show();
    }

   public partial class MeetingOverviewFilter : BaseDialogForm, IMeetingOverviewFilter
    {
        private readonly IMeetingOverviewViewModel _model;
        private readonly IEventAggregator _eventAggregator;
       private readonly IPersonSelectorPresenter _personSelectorPresenter;
       private bool _treeloaded;
       private bool _selectionChanged;

       public MeetingOverviewFilter(IMeetingOverviewViewModel model, IEventAggregator eventAggregator, IPersonSelectorPresenter personSelectorPresenter)
        {
            _model = model;
            _eventAggregator = eventAggregator;
            _personSelectorPresenter = personSelectorPresenter;
            if(_eventAggregator == null)
                throw new ArgumentNullException("eventAggregator");
            _eventAggregator.GetEvent<GroupPageNodeCheckedChange>().Subscribe(selectionChanged);
            
            InitializeComponent();
            buttonClose.Text = UserTexts.Resources.Close;
            
         }

        private void meetingOverviewFilterLoad(object sender, EventArgs e)
        {
            checkTree();
        }

        private void checkTree()
        {
            if(_treeloaded) return;
            var appFunction = ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                DefinedRaptorApplicationFunctionPaths.OpenSchedulePage);
            _personSelectorPresenter.ApplicationFunction = appFunction;
            _personSelectorPresenter.ShowPersons = true;
           
            //_personSelectorPresenter.ShowFind = true;

            var view = (Control)_personSelectorPresenter.View;
            panel1.Controls.Add(view);
            view.Dock = DockStyle.Fill;

            var selectorView = _personSelectorPresenter.View;
            selectorView.PreselectedPersonIds = _model.FilteredPersonsId;
            selectorView.ShowCheckBoxes = true;
            selectorView.ShowDateSelection = false;
            selectorView.HideMenu = true;
            _personSelectorPresenter.LoadTabs();
            
            _treeloaded = true;
        }

        private void selectionChanged(string something)
        {
            _model.FilteredPersonsId = _personSelectorPresenter.CheckedPersonGuids;
            _selectionChanged = true;
            //publish this when form hidden instead if changes have been done
            //_eventAggregator.GetEvent<MeetingModificationOccurred>().Publish(string.Empty);
        }

        private void meetingOverviewFilterDeactivate(object sender, EventArgs e)
        {
            Hide();
            sendEvents();
        }

        private void buttonCloseClick(object sender, EventArgs e)
        {
            Hide();
            sendEvents();
        }

       private void sendEvents()
       {
           _eventAggregator.GetEvent<PersonSelectionFormHideEvent>().Publish(string.Empty);
           if(_selectionChanged)
                _eventAggregator.GetEvent<MeetingModificationOccurred>().Publish("");
       }

        private void meetingOverviewFilterFormClosed(object sender, FormClosedEventArgs e)
        {
            _eventAggregator.GetEvent<GroupPageNodeCheckedChange>().Unsubscribe(selectionChanged);
        }
        
    }
}
