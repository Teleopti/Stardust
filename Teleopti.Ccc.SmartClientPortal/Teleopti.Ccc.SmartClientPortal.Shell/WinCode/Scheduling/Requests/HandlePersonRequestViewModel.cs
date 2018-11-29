using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Collections;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
	/// <summary>
	/// ViewModel for handling requests, holds a collection of RequestsViewModels
	/// </summary>
	/// <remarks>
	/// Created by: henrika
	/// Created date: 2009-11-11
	/// </remarks>
	public class HandlePersonRequestViewModel : INotifyPropertyChanged
	{
		private readonly DateTimePeriod _schedulePeriod;
		private ObservableCollection<PersonRequestViewModel> RequestViewModels { get; set; }
		private readonly TimeSpan _filterTimeSpan;
		private readonly List<PersonRequestViewModel> _showOnlymodels = new List<PersonRequestViewModel>();
		private readonly IList<IPerson> _permittedPersons;
		private readonly IUndoRedoContainer _undoRedoContainer;
		private readonly IDictionary<IPerson, IPersonAccountCollection> _allAccounts;
		private readonly IEventAggregator _eventAggregator;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly TimeZoneInfo _timeZoneInfo;
		private readonly object FilterLock = new object();
	   public ListCollectionView PersonRequestViewModels { get; set; }

		public IList<PersonRequestViewModel> SelectedModels
		{
			get
			{
				return RequestViewModels.Where(m => m.IsSelected && m.IsWithinSchedulePeriod).ToList();
			}
		}

		public PersonRequestViewModel SelectedModel
		{
			get { return PersonRequestViewModels.CurrentItem as PersonRequestViewModel; }
		}

	    public HandlePersonRequestViewModel(DateTimePeriod schedulePeriod, 
												IList<IPerson> permittedPersons, 
												IUndoRedoContainer undoRedoContainer,
												IDictionary<IPerson, IPersonAccountCollection> allAccounts,
												IEventAggregator eventAggregator,
			                                    IPersonRequestCheckAuthorization authorization,
                                                TimeZoneInfo timeZoneInfo)
		{
			_eventAggregator = eventAggregator;
			_authorization = authorization;
			_undoRedoContainer = undoRedoContainer;
			_allAccounts = allAccounts;
			_schedulePeriod = schedulePeriod;
			_permittedPersons = permittedPersons;
			RequestViewModels = new ObservableCollection<PersonRequestViewModel>();
			PersonRequestViewModels = (ListCollectionView)CollectionViewSource.GetDefaultView(RequestViewModels);

			PersonRequestViewModels.SortDescriptions.Add(new SortDescription("LastUpdated",ListSortDirection.Descending));

			setupforEventAggregator();
			_filterTimeSpan = TimeSpan.FromDays(3650);
			filterItems();
			_undoRedoContainer.ChangedHandler += _undoRedoContainer_ChangedHandler;
		    _timeZoneInfo = timeZoneInfo;
		}

		void _undoRedoContainer_ChangedHandler(object sender, EventArgs e)
		{
			UpdatePersonRequestViewModels();
		}

		public void UpdatePersonRequestViewModels()
		{
			foreach (var personRequestViewModel in RequestViewModels)
			{
				personRequestViewModel.NotifyStatusChanged();
			}
		}

		private void setupforEventAggregator()
		{
			//Listen for changes in IsSelected and notify
		   _eventAggregator.GetEvent<GenericEvent<PersonRequestViewModelIsSelectedChanged>>().Subscribe(
				delegate
					{
						new HandlePersonRequestSelectionChanged(SelectedModels,_authorization).PublishEvent("PersonRequestViewModelIsSelectedChanged", _eventAggregator);
					});
		}

		public void CreatePersonRequestViewModels(IList<IPersonRequest> personRequests,IShiftTradeRequestStatusChecker statusChecker, IPersonRequestCheckAuthorization authorization)
		{
			lock (FilterLock)
			{
				RequestViewModels.Clear();
				_showOnlymodels.Clear();
				_showOnlymodels.AddRange(personRequests.Select(request => insertPersonRequestViewModel(request, statusChecker, authorization)));
			}
            filterItems();
		}

		public void UpdateSorting(IList<SortDescription> sortDescriptions)
		{
			var customSorter = new HandlePersonRequestComparer {SortDescriptions = sortDescriptions};
			PersonRequestViewModels.CustomSort = customSorter;
		}

		public void InsertPersonRequestViewModel(IPersonRequest request, IShiftTradeRequestStatusChecker statusChecker,
			IPersonRequestCheckAuthorization authorization)
		{
			var model = insertPersonRequestViewModel(request,statusChecker,authorization);
			if (model != null)
			{
				_showOnlymodels.Add(model);
				filterItems();
			}
		}

		private PersonRequestViewModel insertPersonRequestViewModel(IPersonRequest request, IShiftTradeRequestStatusChecker statusChecker, IPersonRequestCheckAuthorization authorization)
		{
			if (!authorization.HasViewRequestPermission(request) || request.IsNew) return null;

			var model = new PersonRequestViewModel(request, statusChecker, _allAccounts[request.Person], _eventAggregator, _timeZoneInfo);
			var okByMeSpecification = new ShiftTradeRequestOkByMeSpecification(statusChecker);
			var referredSpecification = new ShiftTradeRequestReferredSpecification(statusChecker);
			var afterPeriodSpecification = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(_schedulePeriod);

			if (afterPeriodSpecification.IsSatisfiedBy(request) ||
				(!okByMeSpecification.IsSatisfiedBy(request) &&
				 !referredSpecification.IsSatisfiedBy(request)))
			{
				addPersonRequestViewModel(model);
			}
			return model;
		}

		private void addPersonRequestViewModel(PersonRequestViewModel personRequestViewModel)
		{
			personRequestViewModel.ValidateIfWithinSchedulePeriod(_schedulePeriod, _permittedPersons);
			RequestViewModels.Add(personRequestViewModel);
		}

		/// <summary>
		/// Filters out all the other viewmodels
		/// </summary>
		/// <param name="modelsToShow">The models to show.</param>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-11-18
		/// </remarks>
		public void ShowOnly(IList<PersonRequestViewModel> modelsToShow)
		{
			_showOnlymodels.Clear();
			_showOnlymodels.AddRange(modelsToShow);
			filterItems();
		}

		private void commitStatusInAddingAndEditing()
		{
			if (PersonRequestViewModels.IsAddingNew)
			{
				PersonRequestViewModels.CommitNew();
			}
			else if (PersonRequestViewModels.IsEditingItem)
			{
				PersonRequestViewModels.CommitEdit();
			}
		}

		private bool canApplyFilter()
		{
			if(PersonRequestViewModels.IsAddingNew || PersonRequestViewModels.IsEditingItem)
				return false;
			return true;
		}

		private void filterItems()
		{
			commitStatusInAddingAndEditing();
			if (canApplyFilter())
			{
				lock (FilterLock)
				{
					var updatedOnFilter = new PersonRequestViewModelFilter(_filterTimeSpan);
					var showOnlyfilter = new ShowOnlyPersonRequestViewModelSpecification(_showOnlymodels);
					RequestViewModels.FilterOutBySpecification(updatedOnFilter.Or(showOnlyfilter));
				}
			}
		}

		/// <summary>
		/// Shows all models
		/// </summary>
		/// <remarks>
		/// Created by: henrika
		/// Created date: 2009-11-18
		/// </remarks>
		public void ShowAll()
		{
			ShowOnly(RequestViewModels);
		}

		public void DeletePersonRequestViewModel(IPersonRequest personRequest)
		{
			var modelToDelete = RequestViewModels.FirstOrDefault(r => r.PersonRequest.Id == personRequest.Id);
			if (modelToDelete == null)
				return;
			
			RequestViewModels.Remove(modelToDelete);
		}

		public void ShowRequestDetailsView()
		{
			new ShowRequestDetailsView().PublishEvent("ShowRequestDetailsView", _eventAggregator);
		}

		//Just for fixing mem leaks in WPF
#pragma warning disable 0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067
	}
}
