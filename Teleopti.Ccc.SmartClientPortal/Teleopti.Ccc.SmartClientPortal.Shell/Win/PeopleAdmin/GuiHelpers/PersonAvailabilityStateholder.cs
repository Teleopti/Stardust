using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers
{
    /// <summary>
    /// The stateholder implementation related to person availability
    /// </summary>
    class PersonAvailabilityStateholder : IRotationStateHolder
    {
        /// <summary>
        /// The filtered people stateholder 
        /// </summary>
        private FilteredPeopleHolder _filteredPeopleHolder;

        /// <summary>
        /// The worksheet stateholder 
        /// </summary>
        private WorksheetStateHolder _worksheetStateholder;

        /// <summary>
        /// The person availability repository
        /// </summary>
        private PersonAvailabilityRepository _paRepository;

        /// <summary>
        /// The private member to store the grid view collection related to person rotation
        /// </summary>
        private IList<PersonAvailabilityModelChild> _personAvailabilityChildAdapterCollection;

        /// <summary>
        /// Readonly collection to hold children person rotations
        /// </summary>
        private IList<IPersonAvailability> _childrenPersonAvailabilityCollection = new List<IPersonAvailability>();
		
        /// <summary>
        /// To hold the state-holder that was passed in from the constructor
        /// </summary>
        public WorksheetStateHolder WorksheetStateHolder
        {
            get { return _worksheetStateholder; }
        }
		
        public PersonAvailabilityRepository AvailabilityRepository => _paRepository ??
																	  (_paRepository = PersonAvailabilityRepository.DONT_USE_CTOR(FilteredStateHolder.GetUnitOfWork));

	    /// <summary>
        /// Sets the stateholders
        /// </summary>
        /// <param name="filteredPeopleHolder">An instance of <see cref="FilteredPeopleHolder"/></param>
        /// <param name="worksheetStateHolder">An instance of <see cref="WorksheetStateHolder"/></param>
        public PersonAvailabilityStateholder(FilteredPeopleHolder filteredPeopleHolder, WorksheetStateHolder worksheetStateHolder)
        {
            _filteredPeopleHolder = filteredPeopleHolder;
            _worksheetStateholder = worksheetStateHolder;
        }
		
        /// <summary>
        /// Adds a person rotation to the specified parent row
        /// </summary>
        /// <param name="parentRowIndex"></param>
        public void AddPersonRotation(int parentRowIndex)
        {
            IPerson selectedPerson = FilteredStateHolder.PersonAvailabilityParentAdapterCollection[parentRowIndex].Person;
            int rowIndex;
            
            var selectedItem =
                FilteredStateHolder.ParentPersonAvailabilityCollection.FirstOrDefault(p => p.Person.Id == selectedPerson.Id);
            if(selectedItem != null)
                rowIndex = FilteredStateHolder.ParentPersonAvailabilityCollection.IndexOf(selectedItem);
            else return;

            IPersonAvailability personRotation = WorksheetStateHolder.GetSamplePersonAvailability(selectedPerson);
            
            if (personRotation != null)
            {
                personRotation.StartDate =
                    PeriodDateService.GetValidPeriodDate(
                        PeriodDateDictionaryBuilder.GetDateOnlyDictionary(FilteredStateHolder,
                        ViewType.PersonAvailabilityView, selectedPerson), DateOnly.Today);

                FilteredStateHolder.AddNewPersonAvailability(personRotation);

                if (FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex] == null)
                {
                    FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex] = personRotation;
                }
                FilteredStateHolder.AllPersonAvailabilityCollection.Add(personRotation);
            }
        }

        /// <summary>
        /// Deletes all person rotations in the specified parent row index
        /// </summary>
        /// <param name="parentRowIndex">The parent row index who's children are to be deleted</param>
        public void DeleteAllChildPersonRotations(int parentRowIndex)
        {
            //lets load the _childPersonRotationCollection as it is to be used in some other methods
            GetChildPersonRotations(parentRowIndex);

            foreach (IPersonAvailability pAvail in _childrenPersonAvailabilityCollection)
            {
                if (FilteredStateHolder.AllPersonAvailabilityCollection.Contains(pAvail))
                {
                    FilteredStateHolder.AllPersonAvailabilityCollection.Remove(pAvail);

                    if (pAvail.Id != null)
                        AvailabilityRepository.Remove(pAvail);
                    else
                        FilteredStateHolder.DeleteNewPersonAvailability(pAvail);
                }
            }
        }

        /// <summary>
        /// Deletes person from the specified parent row and in the specified child index
        /// </summary>
        /// <param name="parentRowIndex">The parent row index</param>
        /// <param name="childRowIndex">The child row index</param>
        public void DeletePersonRotation(int parentRowIndex, int childRowIndex)
        {
            //Load the child person rotations for the current parent
            if (childRowIndex >= _childrenPersonAvailabilityCollection.Count)
            GetChildPersonRotations(parentRowIndex);

            IPersonAvailability availabilityToBeRemoved = _childrenPersonAvailabilityCollection[childRowIndex];

            if (availabilityToBeRemoved != null && availabilityToBeRemoved.Id != null)
            {
                _childrenPersonAvailabilityCollection.RemoveAt(childRowIndex);

                if (FilteredStateHolder.AllPersonAvailabilityCollection.Contains(availabilityToBeRemoved))
                {
                    FilteredStateHolder.AllPersonAvailabilityCollection.Remove(availabilityToBeRemoved);
                }

                AvailabilityRepository.Remove(availabilityToBeRemoved);
            }
            else if (availabilityToBeRemoved != null && availabilityToBeRemoved.Id == null)
            {
                _childrenPersonAvailabilityCollection.RemoveAt(childRowIndex);

                DeleteNewPersonAvailability(availabilityToBeRemoved);
            }
        }
		
        /// <summary>
        /// Deletes the new person availability.
        /// </summary>
        /// <param name="availabilityToBeRemoved">The availability to be removed.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-06
        /// </remarks>
        private void DeleteNewPersonAvailability(IPersonAvailability availabilityToBeRemoved)
        {
            if (availabilityToBeRemoved != null && availabilityToBeRemoved.Id == null)
            {
                if (FilteredStateHolder.DeleteNewPersonAvailability(availabilityToBeRemoved))
                {
                    if (FilteredStateHolder.AllPersonAvailabilityCollection.Contains(availabilityToBeRemoved))
                    {
                        FilteredStateHolder.AllPersonAvailabilityCollection.Remove(availabilityToBeRemoved);
                    }
                }
            }
        }
		
        /// <summary>
        /// Deletes person from the specified parent row 
        /// </summary>
        /// <param name="rowIndex">The parent row index</param>
        public void DeletePersonRotation(int parentRowIndex)
        {
            IPerson person = FilteredStateHolder.PersonAvailabilityParentAdapterCollection[parentRowIndex].Person;
            int rowIndex =
                FilteredStateHolder.ParentPersonAvailabilityCollection.
                IndexOf(FilteredStateHolder.ParentPersonAvailabilityCollection.FirstOrDefault(p => p.Person.Id == person.Id))
            ;

            //lets load the _childPersonRotationCollection as it is to be used in some other methods
            GetChildPersonRotations(parentRowIndex);
            IPersonAvailability availabilityToBeRemoved = FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex];

            if (availabilityToBeRemoved != null && availabilityToBeRemoved.Id != null)
            {
                if (FilteredStateHolder.AllPersonAvailabilityCollection.Contains(availabilityToBeRemoved))
                {
                    FilteredStateHolder.AllPersonAvailabilityCollection.Remove(availabilityToBeRemoved);
                }

                AvailabilityRepository.Remove(availabilityToBeRemoved);
            }
            else if (availabilityToBeRemoved != null && availabilityToBeRemoved.Id == null)
            {
                DeleteNewPersonAvailability(availabilityToBeRemoved);
            }
           
        }

        /// <summary>
        /// Deletes person from the specified parent row
        /// </summary>
        /// <param name="rowIndex">The parent row index</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-23
        /// </remarks>
        public void DeleteAllPersonRotation(int rowIndex)
        {
            //lets load the _childPersonRotationCollection as it is to be used in some other methods
            GetChildPersonRotations(rowIndex);
            PersonAvailabilityRepository prRepository = PersonAvailabilityRepository.DONT_USE_CTOR(_filteredPeopleHolder.GetUnitOfWork);

            IPersonAvailability availabilityToBeRemoved = _filteredPeopleHolder.ParentPersonAvailabilityCollection[rowIndex];

            if (availabilityToBeRemoved != null && availabilityToBeRemoved.Id != null)
            {
                if (_filteredPeopleHolder.AllPersonAvailabilityCollection.Contains(availabilityToBeRemoved))
                {
                    _filteredPeopleHolder.AllPersonAvailabilityCollection.Remove(availabilityToBeRemoved);
                    prRepository.Remove(availabilityToBeRemoved);
                }
                              
            }
            else if (availabilityToBeRemoved != null && availabilityToBeRemoved.Id == null)
            {
                RemoveNewPersonAvailability(availabilityToBeRemoved, _filteredPeopleHolder);
            } 
        }

        /// <summary>
        /// Removes the new person availability.
        /// </summary>
        /// <param name="availabilityToBeRemoved">The availability to be removed.</param>
        /// <param name="filteredPeopleHolder">The filtered people holder.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-23
        /// </remarks>
        private static void RemoveNewPersonAvailability(IPersonAvailability availabilityToBeRemoved, FilteredPeopleHolder filteredPeopleHolder)
        {
            if (availabilityToBeRemoved != null && availabilityToBeRemoved.Id == null)
            {
                if (filteredPeopleHolder.AllPersonAvailabilityCollection.Contains(availabilityToBeRemoved))
                {
                    filteredPeopleHolder.AllPersonAvailabilityCollection.Remove(availabilityToBeRemoved);
                    filteredPeopleHolder.DeleteNewPersonAvailability(availabilityToBeRemoved);
                }
            }
        }

        /// <summary>
        /// Get all child rotations for the specified parent ro index
        /// </summary>
        /// <param name="parentRowIndex">The parent row index</param>
        public void GetChildPersonRotations(int parentRowIndex)
        {
	        var parent = FilteredStateHolder.PersonAvailabilityParentAdapterCollection[parentRowIndex];
	        var person = parent.Person;
            var canBold = parent.CanBold;
			
            var selectedItem = FilteredStateHolder.ParentPersonAvailabilityCollection.First(p => p.Person.Id == person.Id);

            //the selected item can be null from the _parentPersonRotationCollection in instances where 
            //there is no PersonRotation associated with the person
            if (selectedItem != null)
            {
                //TODO:Check whethere this need to be duplicated
                WorksheetStateHolder.CurrentRotationChildName =FilteredStateHolder.CommonNameDescription.BuildFor( selectedItem.Person);   //.Name.ToString(); // FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex].Person.Name.ToString();

                _childrenPersonAvailabilityCollection =
                    FilteredStateHolder.AllPersonAvailabilityCollection.Where(a => a.Person.Id == selectedItem.Person.Id).ToList();

                _childrenPersonAvailabilityCollection = _childrenPersonAvailabilityCollection.OrderByDescending(n2 => n2.StartDate).ToList();

                _personAvailabilityChildAdapterCollection = new List<PersonAvailabilityModelChild>();

                bool isFirstItem = true;
                foreach (IPersonAvailability pAvailability in _childrenPersonAvailabilityCollection)
                {
                    PersonAvailabilityModelChild personAvailabilityAdapter =
                        new PersonAvailabilityModelChild(selectedItem.Person, pAvailability,FilteredStateHolder.CommonNameDescription);

                    if (isFirstItem)
                    {
                        personAvailabilityAdapter.PersonFullName =FilteredStateHolder.CommonNameDescription.BuildFor( pAvailability.Person);
                        isFirstItem = false;
                    }
                    else
                        personAvailabilityAdapter.PersonFullName = string.Empty;

                    personAvailabilityAdapter.PersonRotation = pAvailability;

                    if (((pAvailability == selectedItem) && canBold) || pAvailability.Id == null)
                    {
                        personAvailabilityAdapter.CanBold = true;
                    }

                    //if the rotation has not been set, use the default rotation
                    if (pAvailability.Availability == null)
                        personAvailabilityAdapter.CurrentRotation = FilteredStateHolder.GetDefaultAvailability;
                    else
                        personAvailabilityAdapter.CurrentRotation = pAvailability.Availability;

                    _personAvailabilityChildAdapterCollection.Add(personAvailabilityAdapter);
                }
            }
            else
            {
                _childrenPersonAvailabilityCollection.Clear();
            }

            //Set the stateholder collections
            WorksheetStateHolder.ChildPersonAvailabilityCollection =
                new ReadOnlyCollection<IPersonAvailability>(_childrenPersonAvailabilityCollection);

            WorksheetStateHolder.PersonAvailabilityChildGridData =
                new ReadOnlyCollection<PersonAvailabilityModelChild>(_personAvailabilityChildAdapterCollection);
        }

        /// <summary>
        /// Gets the child person rotations.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <param name="grid">The grid.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-17
        /// </remarks>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-17
        /// </remarks>
        public void GetChildPersonRotations(int rowIndex, GridControl grid)
        {
	        var parent = FilteredStateHolder.PersonAvailabilityParentAdapterCollection[rowIndex];
	        var person = parent.Person;
            var canBold = parent.CanBold;
			
            IPersonAvailability selectedItem = FilteredStateHolder.ParentPersonAvailabilityCollection.First(p => p.Person.Id == person.Id);

            ReadOnlyCollection<PersonAvailabilityModelChild> cachedCollection = grid.Tag as
                ReadOnlyCollection<PersonAvailabilityModelChild>;

            //the selected item can be null from the _parentPersonRotationCollection in instances where 
            //there is no PersonRotation associated with the person
            if (selectedItem != null)
            {
                //TODO:Check whethere this need to be duplicated
                WorksheetStateHolder.CurrentRotationChildName = FilteredStateHolder.CommonNameDescription.BuildFor(selectedItem.Person); // FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex].Person.Name.ToString();

                _childrenPersonAvailabilityCollection =
                    FilteredStateHolder.AllPersonAvailabilityCollection.Where(a => a.Person.Id == selectedItem.Person.Id).ToList();

                _childrenPersonAvailabilityCollection = _childrenPersonAvailabilityCollection.OrderByDescending(n2 => n2.StartDate).ToList();

                _personAvailabilityChildAdapterCollection = new List<PersonAvailabilityModelChild>();

                bool isFirstItem = true;
                foreach (IPersonAvailability pAvailability in _childrenPersonAvailabilityCollection)
                {
                    PersonAvailabilityModelChild personAvailabilityAdapter =
                        new PersonAvailabilityModelChild(selectedItem.Person, pAvailability, FilteredStateHolder.CommonNameDescription);

                    if (isFirstItem)
                    {
                        personAvailabilityAdapter.PersonFullName = FilteredStateHolder.CommonNameDescription.BuildFor(pAvailability.Person);
                        isFirstItem = false;
                    }
                    else
                        personAvailabilityAdapter.PersonFullName = string.Empty;

                    personAvailabilityAdapter.PersonRotation = pAvailability;

                    if (pAvailability.Id == null)
                    {
                        personAvailabilityAdapter.CanBold = true;
                    }
                    else
                    {
                        personAvailabilityAdapter.CanBold = PeopleAdminHelper.IsCanBold(pAvailability,
                            cachedCollection);
                    }

                    if (pAvailability == selectedItem)
                    {
                        // This is fixed for following secnario :If user click save button and user is changing current 
                        // period in the grid then he expands the grid changes still should be bold.
                        // (Apply only when adapter.Canbold is false)
                        if (!personAvailabilityAdapter.CanBold)
                        {
                            personAvailabilityAdapter.CanBold = canBold;
                        }
                    }

                    //if the rotation has not been set, use the default rotation
                    if (pAvailability.Availability == null)
                        personAvailabilityAdapter.CurrentRotation = FilteredStateHolder.GetDefaultAvailability;
                    else
                        personAvailabilityAdapter.CurrentRotation = pAvailability.Availability;

                    _personAvailabilityChildAdapterCollection.Add(personAvailabilityAdapter);
                }
            }
            else
            {
                _childrenPersonAvailabilityCollection.Clear();
            }

            //Set the stateholder collections
            WorksheetStateHolder.ChildPersonAvailabilityCollection =
                new ReadOnlyCollection<IPersonAvailability>(_childrenPersonAvailabilityCollection);

            WorksheetStateHolder.PersonAvailabilityChildGridData =
                new ReadOnlyCollection<PersonAvailabilityModelChild>(_personAvailabilityChildAdapterCollection);
        }

        /// <summary>
        /// Resets the parent item upon addition of anew item or when an existing child item is updated
        /// </summary>
        /// <param name="parentRowIndex">The parent rown index</param>
        public void GetParentPersonRotationWhenAddedOrUpdated(int parentRowIndex)
        {
	        var parent = FilteredStateHolder.PersonAvailabilityParentAdapterCollection[parentRowIndex];
	        var person = parent.Person;
            var canBold = parent.CanBold;
            var grid = parent.GridControl;
			
            var selectedPersonRotation = FilteredStateHolder.ParentPersonAvailabilityCollection.FirstOrDefault(p => p.Person.Id == person.Id);
            if (selectedPersonRotation == null) return;

            int rowIndex = FilteredStateHolder.ParentPersonAvailabilityCollection.IndexOf(selectedPersonRotation);

            PersonAvailabilityModelParent personAvailabilityAdapterParent;

            GetChildPersonRotations(parentRowIndex);
            IPersonAvailability currentPersonAvailability = FilteredStateHolder.GetCurrentPersonAvailability(_childrenPersonAvailabilityCollection);

            if (currentPersonAvailability != null)
            {
                personAvailabilityAdapterParent =
                new PersonAvailabilityModelParent(currentPersonAvailability.Person, currentPersonAvailability, FilteredStateHolder.CommonNameDescription);

                if (currentPersonAvailability.Id == null || canBold)
                {
                    personAvailabilityAdapterParent.CanBold = true;
                }
                else
                {
                    ReadOnlyCollection<PersonAvailabilityModelChild> cachedCollection = null;

                    if (grid != null)
                    {
                        cachedCollection = grid.Tag as
                            ReadOnlyCollection<PersonAvailabilityModelChild>;
                    }

                    personAvailabilityAdapterParent.CanBold = PeopleAdminHelper.IsCanBold(currentPersonAvailability,
                    cachedCollection);
                    
                }

                FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex] = currentPersonAvailability;

                personAvailabilityAdapterParent.PersonRotation = currentPersonAvailability;
                personAvailabilityAdapterParent.FromDate = currentPersonAvailability.StartDate;
                personAvailabilityAdapterParent.CurrentRotation = currentPersonAvailability.Availability;
                personAvailabilityAdapterParent.RotationCount = _childrenPersonAvailabilityCollection.Count == 1
                                                                      ? 0
                                                                      : _childrenPersonAvailabilityCollection.Count;


            }
            else
            {
                personAvailabilityAdapterParent = new PersonAvailabilityModelParent(selectedPersonRotation.Person, null, FilteredStateHolder.CommonNameDescription);

                personAvailabilityAdapterParent.PersonRotation = currentPersonAvailability;
                personAvailabilityAdapterParent.RotationCount = _childrenPersonAvailabilityCollection.Count + 1;

                FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex] = WorksheetStateHolder.GetSamplePersonAvailability(selectedPersonRotation.Person);
            }
            FilteredStateHolder.PersonAvailabilityParentAdapterCollection[parentRowIndex] = personAvailabilityAdapterParent;
        }

        /// <summary>
        /// Resets the the parent item when a item is deleted
        /// </summary>
        /// <param name="rowIndex"></param>
        public void GetParentPersonRotationWhenDeleted(int parentRowIndex)
        {
            var person = FilteredStateHolder.PersonAvailabilityParentAdapterCollection[parentRowIndex].Person;
            int rowIndex =
                FilteredStateHolder.ParentPersonAvailabilityCollection.
                IndexOf(FilteredStateHolder.ParentPersonAvailabilityCollection.FirstOrDefault(p => p.Person.Id == person.Id));

            IPersonAvailability selectedPersonRotation = FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex];
            PersonAvailabilityModelParent personAvailabilityAdapterParent;

            GetChildPersonRotations(parentRowIndex);
            IPersonAvailability currentPersonAvailability = FilteredStateHolder.GetCurrentPersonAvailability(_childrenPersonAvailabilityCollection);

            if (currentPersonAvailability != null)
            {
                FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex] = currentPersonAvailability;

                personAvailabilityAdapterParent =
                    new PersonAvailabilityModelParent(currentPersonAvailability.Person,
                                                                currentPersonAvailability,FilteredStateHolder.CommonNameDescription);

                if (currentPersonAvailability.Id == null)
                    personAvailabilityAdapterParent.CanBold = true;

                personAvailabilityAdapterParent.PersonRotation = currentPersonAvailability;
                personAvailabilityAdapterParent.FromDate = currentPersonAvailability.StartDate;
                personAvailabilityAdapterParent.CurrentRotation = currentPersonAvailability.Availability;
                personAvailabilityAdapterParent.RotationCount =
                    _childrenPersonAvailabilityCollection.Count > 1 ? _childrenPersonAvailabilityCollection.Count : 0;

                FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex] = currentPersonAvailability;

            }
            else
            {
                personAvailabilityAdapterParent =
                    new PersonAvailabilityModelParent(selectedPersonRotation.Person, currentPersonAvailability, FilteredStateHolder.CommonNameDescription);

                personAvailabilityAdapterParent.PersonRotation = currentPersonAvailability;
                personAvailabilityAdapterParent.RotationCount = _childrenPersonAvailabilityCollection.Count > 0 ? 2 : 0;

                FilteredStateHolder.ParentPersonAvailabilityCollection[rowIndex] = WorksheetStateHolder.GetSamplePersonAvailability(selectedPersonRotation.Person);
            }

            FilteredStateHolder.PersonAvailabilityParentAdapterCollection[parentRowIndex] = personAvailabilityAdapterParent;
        }

        /// <summary>
        /// Sets the children person rotation collection.
        /// </summary>
        /// <param name="childrenPersonAvailabilityCollection">The children person availability collection.</param>
        /// <remarks>
        public void SetChildrenPersonRotationCollection(object childrenPersonAvailabilityCollection)
        {
            IList<PersonAvailabilityModelChild> childs =
               childrenPersonAvailabilityCollection as IList<PersonAvailabilityModelChild>;
            if (childs != null)
            {
                _childrenPersonAvailabilityCollection.Clear();
                foreach (PersonAvailabilityModelChild child in childs)
                {
                    _childrenPersonAvailabilityCollection.Add(child.PersonRotation);
                }
            }
        }

        /// <summary>
        /// Gets the current entity.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="gridinCellColumnIndex">Index of the gridin cell column.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/19/2009
        /// </remarks>
        public object GetCurrentEntity(GridControl grid, int gridinCellColumnIndex)
        {
            IAvailabilityRotation availability = null;

            if (grid.Model.SelectedRanges.Count > PeopleAdminConstants.GridSelectedRangesCountBoundary)
            {
                GridRangeInfo gridRangeInfo = grid.Model.SelectedRanges[0];

                int selectedIndex = gridRangeInfo.Top;

                if (gridRangeInfo.Top <= 0)
                    return null;

	            var parentAdapter = _filteredPeopleHolder.PersonAvailabilityParentAdapterCollection[selectedIndex - 1];
	            if (!parentAdapter.ExpandState)
                {
                    // get Parent rotation now
                    availability = parentAdapter.CurrentRotation;
                }
                else
                {
                    // get from child rotation 
                    CellEmbeddedGrid childGrid = grid[selectedIndex, gridinCellColumnIndex].Control as CellEmbeddedGrid;

                    if (childGrid != null && childGrid.Model.SelectedRanges.Count > 0)
                    {

                        GridRangeInfo childGridRangeInfo = childGrid.Model.SelectedRanges[0];

                        ReadOnlyCollection<PersonAvailabilityModelChild> personAvailabilityChildCollection =
                            childGrid.Tag as ReadOnlyCollection<PersonAvailabilityModelChild>;

                        availability = personAvailabilityChildCollection[childGridRangeInfo.Top - 1].CurrentRotation;
                    }

                }
            }

            return availability;
        }

		public FilteredPeopleHolder FilteredStateHolder
    	{
			get { return _filteredPeopleHolder; }
			set { _filteredPeopleHolder = value; }
    	}
    }
}
