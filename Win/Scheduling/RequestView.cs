﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
    /// <summary>
    /// View for requests
    /// </summary>
    public class RequestView : IHelpContext
    {
        private readonly IUndoRedoContainer _container;
        private readonly IList<IPersonRequest> _personRequestList;
        private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
        private IList<PersonRequestViewModel> _source = new List<PersonRequestViewModel>();
        private readonly HandlePersonRequestViewModel _model;
        private IEventAggregator _eventAggregator;
        private bool _needReload;
        private readonly IPersonRequestCheckAuthorization _authorization;
        private bool _needUpdate;


        public RequestView(FrameworkElement handlePersonRequestView, ISchedulerStateHolder schedulerStateHolder, IUndoRedoContainer container, IDictionary<IPerson, IPersonAccountCollection> allAccountPersonCollection,IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _container = container;
            _personRequestList = schedulerStateHolder.PersonRequests;
            _authorization = new PersonRequestCheckAuthorization();
            _shiftTradeRequestStatusChecker = new ShiftTradeRequestStatusCheckerWithSchedule(schedulerStateHolder.Schedules,_authorization);
            _model = new HandlePersonRequestViewModel(schedulerStateHolder.RequestedPeriod.Period(), schedulerStateHolder.AllPermittedPersons, _container, allAccountPersonCollection, _eventAggregator, _authorization, schedulerStateHolder.TimeZoneInfo);
            CreatePersonRequestViewModels(schedulerStateHolder, handlePersonRequestView);
            InitObservableListEvents();
        }

        public void CreatePersonRequestViewModels(ISchedulerStateHolder schedulerStateHolder, FrameworkElement handlePersonRequestView)
        {
            _model.CreatePersonRequestViewModels(schedulerStateHolder.PersonRequests, _shiftTradeRequestStatusChecker, _authorization);
            handlePersonRequestView.DataContext = _model;
        }

        public void InsertPersonRequestViewModel(IPersonRequest personRequest)
        {
            _model.InsertPersonRequestViewModel(personRequest, _shiftTradeRequestStatusChecker, _authorization);
        }

        public void DeletePersonRequestViewModel(IPersonRequest personRequest)
        {
            _model.DeletePersonRequestViewModel(personRequest);
        }

        public void UpdatePersonRequestViewModel()
        {
            _model.UpdatePersonRequestViewModels();
        }

        public void FilterGrid(IList<string> filterWords)
        {
			var allModels = (IList<PersonRequestViewModel>)_model.PersonRequestViewModels.SourceCollection;
			var modelsToShow = RequestPresenter.FilterAdapters(allModels, filterWords);
			_source = RequestPresenter.FilterAdapters(modelsToShow, filterWords);
			_model.ShowOnly(_source);
        }

	    public bool IsSelectionEditable()
        {
            bool editable = false;

            if (_model.SelectedModel != null) 
                editable = _model.SelectedModel.IsEditable;

            return editable;
        }

        public IList<PersonRequestViewModel> SelectedAdapters()
        {
            return _model.SelectedModels;
        }

		public PersonRequestViewModel SelectedModel
		{
			get { return _model.SelectedModel; }
		}

        public bool NeedReload
        {
            get { return _needReload; }
            set { _needReload = value; }
        }

        public bool NeedUpdate
        {
            get { return _needUpdate; }
            set { _needUpdate = value; }
        }

        #region events

        // worry: this code only wires up events of PersonRequests that exist at construction
        // if PersonRequests are added or removed, that should be handled by the view (never happens?)
        // they are not wired up

        private void InitObservableListEvents()
        {
            foreach(IPersonRequest personRequest in _personRequestList)
            {
                personRequest.PropertyChanged += personRequest_PropertyChanged;
            }

        }

        void personRequest_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged(e.PropertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
        	var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion  
  
        #region IHelpContext Members

        public bool HasHelp
        {
            get { return true; }
        }

        public string HelpId
        {
            get { return GetType().Name; }
        }

        #endregion

        public void FilterDays(TimeSpan timeSpan)
        {
            _model.FilterOutOlderThan(timeSpan);
        }
    }
}