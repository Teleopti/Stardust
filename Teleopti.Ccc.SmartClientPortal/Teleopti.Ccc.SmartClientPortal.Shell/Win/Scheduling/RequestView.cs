using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests;
using Teleopti.Ccc.WinCode.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    /// <summary>
    /// View for requests
    /// </summary>
    public class RequestView : IHelpContext
    {
        private readonly IList<IPersonRequest> _personRequestList;
        private readonly IShiftTradeRequestStatusChecker _shiftTradeRequestStatusChecker;
        private IList<PersonRequestViewModel> _source = new List<PersonRequestViewModel>();
        private readonly HandlePersonRequestViewModel _model;
	    private readonly SchedulingScreenState _schedulerStateHolder;
        private readonly IPersonRequestCheckAuthorization _authorization;
		private bool _isWindowLoaded;
	    private readonly RequestPresenter _presenter;

		public RequestView(FrameworkElement handlePersonRequestView, SchedulingScreenState schedulerStateHolder,
			IUndoRedoContainer container, IDictionary<IPerson, IPersonAccountCollection> allAccountPersonCollection,
			IEventAggregator eventAggregator, ITimeZoneGuard timeZoneGuard)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_personRequestList = schedulerStateHolder.PersonRequests;
			_authorization = new PersonRequestCheckAuthorization();
			_presenter = new RequestPresenter(_authorization);
			_shiftTradeRequestStatusChecker =
				new ShiftTradeRequestStatusCheckerWithSchedule(schedulerStateHolder.SchedulerStateHolder.Schedules,
					_authorization);
			_model = new HandlePersonRequestViewModel(
				schedulerStateHolder.SchedulerStateHolder.RequestedPeriod.Period(),
				schedulerStateHolder.SchedulerStateHolder.ChoosenAgents, container, allAccountPersonCollection,
				eventAggregator, _authorization, timeZoneGuard.CurrentTimeZone());
			CreatePersonRequestViewModels(schedulerStateHolder, handlePersonRequestView);

			InitObservableListEvents();
		}

		public void CreatePersonRequestViewModels(SchedulingScreenState schedulerStateHolder, FrameworkElement handlePersonRequestView)
        {
            _model.CreatePersonRequestViewModels(schedulerStateHolder.PersonRequests, _shiftTradeRequestStatusChecker, _authorization);
            handlePersonRequestView.DataContext = _model;
        }

		public event EventHandler SelectionChanged
		{
			add { _model.PersonRequestViewModels.CurrentChanged += value; }
			remove { _model.PersonRequestViewModels.CurrentChanged -= value; }
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

        public void FilterGrid(IList<string> filterWords, IDictionary<Guid, IPerson> filteredPersonDictionary)
        {
			var allModels = (IList<PersonRequestViewModel>)_model.PersonRequestViewModels.SourceCollection;
            _presenter.InitializeFileteredPersonDictionary(filteredPersonDictionary);
            var modelsToShow = _presenter.FilterAdapters(allModels, filterWords);
			_source = _presenter.FilterAdapters(modelsToShow, filterWords);
			_model.ShowOnly(_source);
	        foreach (var model in allModels)
		        model.IsSelected = false;
			_model.SelectedModels.Clear();

        }

		public void FilterPersons(IEnumerable<Guid> persons)
		{
			_source = _presenter.FilterAdapters((IList<PersonRequestViewModel>) _model.PersonRequestViewModels.SourceCollection,
				                                persons);
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

        public bool NeedReload { get; set; }

        public bool NeedUpdate { get; set; }

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

	    public bool IsWindowLoaded
	    {
		    get { return _isWindowLoaded; }
		    set { _isWindowLoaded = value; }
	    }

	    #endregion

		public void ShowRequestAllowanceView(IWin32Window owner)
		{
			var defaultRequest = SelectedAdapters().Count > 0
														 ? SelectedAdapters().First().PersonRequest
														 : _schedulerStateHolder.PersonRequests.FirstOrDefault(
															 r =>
															 r.Request is AbsenceRequest &&
															 _schedulerStateHolder.SchedulerStateHolder.RequestedPeriod.Period().Contains(r.Request.Period));

			if (defaultRequest == null)
			{
				var allowanceView = new RequestAllowanceView(null, _schedulerStateHolder.SchedulerStateHolder.RequestedPeriod.DateOnlyPeriod.StartDate);

				if (!_isWindowLoaded)
				{
					allowanceView.Show(owner);
					_isWindowLoaded = true;
					allowanceView.FormClosed += allowanceView_FormClosed;
				}
				else
				{
					_isWindowLoaded = false;
				}
			}
			else
			{
				var requestDate = new DateOnly(defaultRequest.RequestedDate);
				var personPeriod = defaultRequest.Person.PersonPeriodCollection.FirstOrDefault(p => p.Period.Contains(requestDate));
				if (personPeriod != null)
				{
					var allowanceView = new RequestAllowanceView(personPeriod.BudgetGroup, requestDate);

					if (!_isWindowLoaded)
					{
						allowanceView.Show(owner);
						_isWindowLoaded = true;
						allowanceView.FormClosed += allowanceView_FormClosed;
					}
					else
					{
						_isWindowLoaded = false;
					}
				}
			}
		}

		private void allowanceView_FormClosed(object sender, FormClosedEventArgs e)
		{
			_isWindowLoaded = false;
		}
    }
}