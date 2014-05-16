using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
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
		private TimeSpan _filterTimeSpan;
		private IList<PersonRequestViewModel> _showOnlymodels = new List<PersonRequestViewModel>();
		private readonly IList<IPerson> _permittedPersons;
		private readonly IUndoRedoContainer _undoRedoContainer;
		private readonly IDictionary<IPerson, IPersonAccountCollection> _allAccounts;
		private IEventAggregator _eventAggregator;
		private readonly IPersonRequestCheckAuthorization _authorization;
		private readonly TimeZoneInfo _timeZoneInfo;
		private static readonly object FilterLock = new object();
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
				_showOnlymodels = new List<PersonRequestViewModel>();
				foreach (var request in personRequests)
				{
					InsertPersonRequestViewModel(request, statusChecker, authorization);
				}	
			}
            filterItems();
		}

		public void UpdateSorting(IList<SortDescription> sortDescriptions)
		{
			var customSorter = new HandlePersonRequestComparer {SortDescriptions = sortDescriptions};
			PersonRequestViewModels.CustomSort = customSorter;
		}

		public void InsertPersonRequestViewModel(IPersonRequest request, IShiftTradeRequestStatusChecker statusChecker, IPersonRequestCheckAuthorization authorization)
		{
			if (!authorization.HasViewRequestPermission(request) || request.IsNew) return;

			var model = new PersonRequestViewModel(request, statusChecker, _allAccounts[request.Person], _eventAggregator, _timeZoneInfo);
			var okByMeSpecification = new ShiftTradeRequestOkByMeSpecification(statusChecker);
			var referredSpecification = new ShiftTradeRequestReferredSpecification(statusChecker);
			var afterPeriodSpecification = new ShiftTradeRequestIsAfterLoadedPeriodSpecification(_schedulePeriod);

			if (afterPeriodSpecification.IsSatisfiedBy(request) ||
				(!okByMeSpecification.IsSatisfiedBy(request) &&
				!referredSpecification.IsSatisfiedBy(request)))
				AddPersonRequestViewModel(model);
		}

		public void AddPersonRequestViewModel(PersonRequestViewModel personRequestViewModel)
		{

			personRequestViewModel.ValidateIfWithinSchedulePeriod(_schedulePeriod, _permittedPersons);
			RequestViewModels.Add(personRequestViewModel);
			_showOnlymodels.Add(personRequestViewModel);
            filterItems();
		}

		public void FilterOutOlderThan(TimeSpan timeSpan)
		{
			_filterTimeSpan = timeSpan;
			filterItems();  
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
			modelsToShow.ForEach(m=> _showOnlymodels.Add(m));
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
