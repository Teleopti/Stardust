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
    /// The stateholder implementation related to person rotations
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Stateholder")]
    public class PersonRotationStateholder : IRotationStateHolder
    {
        private FilteredPeopleHolder _filteredPeopleHolder;
        private readonly WorksheetStateHolder _worksheetStateholder;
        private PersonRotationRepository _prRepository;

        //the private member to store the grid view collection related to person rotation
        private IList<PersonRotationModelChild> _personRotationChildAdapterCollection;

        //readonly collection to hold children person rotations
        private IList<IPersonRotation> _childrenPersonRotationCollection = new List<IPersonRotation>();

        /// <summary>
        /// To hold the state-holder that was passed in from the constructor
        /// </summary>
        public FilteredPeopleHolder FilteredStateHolder
        {
            get { return _filteredPeopleHolder; }
			set { _filteredPeopleHolder = value; }
        }
        /// <summary>
        /// To hold the state-holder that was passed in from the constructor
        /// </summary>
        public WorksheetStateHolder WorksheetStateHolder
        {
            get { return _worksheetStateholder; }
        }

        /// <summary>
        /// 
        /// </summary>
        public PersonRotationRepository RotationRepository
        {
            get
            {
                if (_prRepository == null)
                    _prRepository = new PersonRotationRepository(FilteredStateHolder.GetUnitOfWork);

                return _prRepository;
            }
        }

        /// <summary>
        /// Sets the stateholders
        /// </summary>
        /// <param name="filteredPeopleHolder">An instance of <see cref="FilteredPeopleHolder"/></param>
        /// <param name="worksheetStateHolder">An instance of <see cref="WorksheetStateHolder"/></param>
        public PersonRotationStateholder(FilteredPeopleHolder filteredPeopleHolder, WorksheetStateHolder worksheetStateHolder)
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
            IPerson selectedPerson = FilteredStateHolder.PersonRotationParentAdapterCollection[parentRowIndex].Person;
            int rowIndex =
                FilteredStateHolder.ParentPersonRotationCollection.
                IndexOf(FilteredStateHolder.ParentPersonRotationCollection.FirstOrDefault(p => p.Person.Id == selectedPerson.Id));

            IPersonRotation personRotation = WorksheetStateHolder.GetSamplePersonRotation(selectedPerson);

            if (personRotation != null)
            {

                personRotation.StartDate =
                    PeriodDateService.GetValidPeriodDate(
                        PeriodDateDictionaryBuilder.GetDateOnlyDictionary(FilteredStateHolder, 
                        ViewType.PersonRotationView, selectedPerson), DateOnly.Today);

                FilteredStateHolder.AddNewPersonRotation(personRotation);

                if (FilteredStateHolder.ParentPersonRotationCollection[rowIndex] == null)
                {
                    FilteredStateHolder.ParentPersonRotationCollection[rowIndex] = personRotation;
                }
                FilteredStateHolder.AllPersonRotationCollection.Add(personRotation);
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

            foreach (IPersonRotation pRotation in _childrenPersonRotationCollection)
            {
                if (FilteredStateHolder.AllPersonRotationCollection.Contains(pRotation))
                {
                    FilteredStateHolder.AllPersonRotationCollection.Remove(pRotation);

                    if (pRotation.Id.HasValue)
                        RotationRepository.Remove(pRotation);
                    else
                        FilteredStateHolder.DeleteNewPersonRotation(pRotation);
                }
            }
        }

        /// <summary>
        /// Deletes the person rotation.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-23
        /// </remarks>
        public void DeletePersonRotation(int rowIndex)
        {
            int parentRowIndex = rowIndex;
            IPerson person = FilteredStateHolder.PersonRotationParentAdapterCollection[rowIndex].Person;
            
            IList<IPersonRotation> selectedList = 
                FilteredStateHolder.ParentPersonRotationCollection.Where(p => p.Person.Id == person.Id).ToList();
            if(selectedList.Count == 0) return;

            rowIndex = FilteredStateHolder.ParentPersonRotationCollection.IndexOf(selectedList[0]);

            //lets load the _childPersonRotationCollection as it is to be used in some other methods
            GetChildPersonRotations(parentRowIndex);
            IPersonRotation rotationToBeRemoved = FilteredStateHolder.ParentPersonRotationCollection[rowIndex];

            if (rotationToBeRemoved != null && rotationToBeRemoved.Id.HasValue)
            {
                if (FilteredStateHolder.AllPersonRotationCollection.Contains(rotationToBeRemoved))
                {
                    FilteredStateHolder.AllPersonRotationCollection.Remove(rotationToBeRemoved);

                    RotationRepository.Remove(rotationToBeRemoved);
                }
            }
            else if (rotationToBeRemoved != null && !rotationToBeRemoved.Id.HasValue)
            {
                DeleteNewPersonRotation(rotationToBeRemoved);
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
            PersonRotationRepository prRepository = new PersonRotationRepository(_filteredPeopleHolder.GetUnitOfWork);

            IPersonRotation rotationToBeRemoved = _filteredPeopleHolder.ParentPersonRotationCollection[rowIndex];

            if (rotationToBeRemoved != null && rotationToBeRemoved.Id.HasValue)
            {
                if (_filteredPeopleHolder.AllPersonRotationCollection.Contains(rotationToBeRemoved))
                {
                    _filteredPeopleHolder.AllPersonRotationCollection.Remove(rotationToBeRemoved);

                    prRepository.Remove(rotationToBeRemoved);
                }
            }
            else if (rotationToBeRemoved != null && !rotationToBeRemoved.Id.HasValue)
            {
                RemoveNewPersonRotation(rotationToBeRemoved, _filteredPeopleHolder);
            }
        }

        /// <summary>
        /// Deletes the new person rotation.
        /// </summary>
        /// <param name="rotationToBeRemoved">The rotation to be removed.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-06
        /// </remarks>
        private void DeleteNewPersonRotation(IPersonRotation rotationToBeRemoved)
        {
            if (rotationToBeRemoved != null && !rotationToBeRemoved.Id.HasValue)
            {
                if (FilteredStateHolder.DeleteNewPersonRotation(rotationToBeRemoved))
                {
                    if (FilteredStateHolder.AllPersonRotationCollection.Contains(rotationToBeRemoved))
                    {
                        FilteredStateHolder.AllPersonRotationCollection.Remove(rotationToBeRemoved);
                    }
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
            if (childRowIndex >= _childrenPersonRotationCollection.Count)
                GetChildPersonRotations(parentRowIndex);

            IPersonRotation rotationToBeRemoved = _childrenPersonRotationCollection[childRowIndex];

            if (rotationToBeRemoved != null && rotationToBeRemoved.Id.HasValue)
            {
                _childrenPersonRotationCollection.RemoveAt(childRowIndex);

                if (FilteredStateHolder.AllPersonRotationCollection.Contains(rotationToBeRemoved))
                {
                    FilteredStateHolder.AllPersonRotationCollection.Remove(rotationToBeRemoved);
                    RotationRepository.Remove(rotationToBeRemoved);
                }
                
            }
            else if (rotationToBeRemoved != null && !rotationToBeRemoved.Id.HasValue)
            {
                _childrenPersonRotationCollection.RemoveAt(childRowIndex);

                DeleteNewPersonRotation(rotationToBeRemoved);
            }

            
        }

        /// <summary>
        /// Get all child rotations for the specified parent ro index
        /// </summary>
        /// <param name="rowIndex">The parent row index</param>
        public void GetChildPersonRotations(int rowIndex)
        {
            IPerson person = FilteredStateHolder.PersonRotationParentAdapterCollection[rowIndex].Person;
            bool canBold = FilteredStateHolder.PersonRotationParentAdapterCollection[rowIndex].CanBold;

            IPersonRotation selectedItem =
                FilteredStateHolder.ParentPersonRotationCollection.FirstOrDefault(p => p.Person.Equals(person));

            //the selected item can be null from the _parentPersonRotationCollection in instances where 
            //there is no PersonRotation associated with the person
            if (selectedItem != null)
            {
                WorksheetStateHolder.CurrentRotationChildName = FilteredStateHolder.CommonNameDescription.BuildFor(person);

                _childrenPersonRotationCollection =
                    FilteredStateHolder.AllPersonRotationCollection.Where(a => a.Person.Equals(selectedItem.Person)).ToList();

                _childrenPersonRotationCollection = _childrenPersonRotationCollection.OrderByDescending(n2 => n2.StartDate).ToList();
                _personRotationChildAdapterCollection = new List<PersonRotationModelChild>();

                bool isFirstItem = true;
                foreach (IPersonRotation pRotation in _childrenPersonRotationCollection)
                {
                    PersonRotationModelChild personRotationModel =
                        new PersonRotationModelChild(selectedItem.Person,FilteredStateHolder.CommonNameDescription);

                    if (isFirstItem)
                    {
                        personRotationModel.PersonFullName =FilteredStateHolder.CommonNameDescription.BuildFor( pRotation.Person);
                        isFirstItem = false;
                    }
                    else
                        personRotationModel.PersonFullName = string.Empty;

                    personRotationModel.PersonRotation = pRotation;
                    
                    if(((pRotation == selectedItem) && canBold) || !pRotation.Id.HasValue)
                    {
                        personRotationModel.CanBold = true;
                    }

                    //if the rotation has not been set, use the default rotation
                    if (pRotation.Rotation == null)
                        personRotationModel.CurrentRotation = FilteredStateHolder.GetDefaultRotation;
                    else
                        personRotationModel.CurrentRotation = pRotation.Rotation;

                    _personRotationChildAdapterCollection.Add(personRotationModel);
                }
            }
            else
            {
                _childrenPersonRotationCollection.Clear();
            }

            //Set the stateholder collections
            WorksheetStateHolder.ChildPersonRotationCollection =
                new ReadOnlyCollection<IPersonRotation>(_childrenPersonRotationCollection);

            WorksheetStateHolder.PersonRotationChildGridData =
                new ReadOnlyCollection<PersonRotationModelChild>(_personRotationChildAdapterCollection);
        }

        public void GetChildPersonRotations(int rowIndex, GridControl grid)
        {
            IPerson person = FilteredStateHolder.PersonRotationParentAdapterCollection[rowIndex].Person;

            bool canBold = FilteredStateHolder.PersonRotationParentAdapterCollection[rowIndex].CanBold;

            IPersonRotation selectedItem = FilteredStateHolder.ParentPersonRotationCollection.FirstOrDefault(p => p.Person.Equals(person));

            ReadOnlyCollection<PersonRotationModelChild> cachedCollection = grid.Tag as 
                ReadOnlyCollection<PersonRotationModelChild>;

            //the selected item can be null from the _parentPersonRotationCollection in instances where 
            //there is no PersonRotation associated with the person
            if (selectedItem != null)
            {
                WorksheetStateHolder.CurrentRotationChildName = person.Name.ToString();

                _childrenPersonRotationCollection =
                    FilteredStateHolder.AllPersonRotationCollection.Where(a => a.Person.Equals(selectedItem.Person)).ToList();

                _childrenPersonRotationCollection = _childrenPersonRotationCollection.OrderByDescending(n2 => n2.StartDate).ToList();

                _personRotationChildAdapterCollection = new List<PersonRotationModelChild>();

                bool isFirstItem = true;
                foreach (IPersonRotation pRotation in _childrenPersonRotationCollection)
                {
                    PersonRotationModelChild personRotationModel =
                        new PersonRotationModelChild(selectedItem.Person,FilteredStateHolder.CommonNameDescription);

                    if (isFirstItem)
                    {
                        personRotationModel.PersonFullName = FilteredStateHolder.CommonNameDescription.BuildFor(pRotation.Person);
                        isFirstItem = false;
                    }
                    else
                        personRotationModel.PersonFullName = string.Empty;

                    personRotationModel.PersonRotation = pRotation;

                    if (pRotation.Id == null)
                    {
                        personRotationModel.CanBold = true;
                    }
                    else
                    {
                        personRotationModel.CanBold = PeopleAdminHelper.IsCanBold(pRotation,
                            cachedCollection);
                    }

                    if (pRotation == selectedItem)
                    {
                        // This is fixed for following secnario :If user click save button and user is changing current 
                        // period in the grid then he expands the grid changes still should be bold.
                        // (Apply only when adapter.Canbold is false)
                        if (!personRotationModel.CanBold)
                        {
                            personRotationModel.CanBold = canBold;
                        }
                    }

                    //if the rotation has not been set, use the default rotation
                    if (pRotation.Rotation == null)
                        personRotationModel.CurrentRotation = FilteredStateHolder.GetDefaultRotation;
                    else
                        personRotationModel.CurrentRotation = pRotation.Rotation;

                    if (pRotation.StartDay == -1)
                        personRotationModel.StartWeek = 1;

                    _personRotationChildAdapterCollection.Add(personRotationModel);
                }
            }
            else
            {
                _childrenPersonRotationCollection.Clear();
            }

            //Set the stateholder collections
            WorksheetStateHolder.ChildPersonRotationCollection =
                new ReadOnlyCollection<IPersonRotation>(_childrenPersonRotationCollection);

            WorksheetStateHolder.PersonRotationChildGridData =
                new ReadOnlyCollection<PersonRotationModelChild>(_personRotationChildAdapterCollection);
        }

        /// <summary>
        /// Resets the parent item upon addition of anew item or when an existing child item is updated
        /// </summary>
        /// <param name="rowIndex">The parent rown index</param>
        public void GetParentPersonRotationWhenAddedOrUpdated(int rowIndex)
        {
            int parentIndex = rowIndex;
            IPerson person = FilteredStateHolder.PersonRotationParentAdapterCollection[parentIndex].Person;
            bool canBold = FilteredStateHolder.PersonRotationParentAdapterCollection[parentIndex].CanBold;
            GridControl grid = FilteredStateHolder.PersonRotationParentAdapterCollection[parentIndex].GridControl;

            IPersonRotation selectedPersonRotation =
                FilteredStateHolder.ParentPersonRotationCollection.FirstOrDefault(p => p.Person.Equals(person));

            if (selectedPersonRotation == null) return;

            rowIndex = FilteredStateHolder.ParentPersonRotationCollection.IndexOf(selectedPersonRotation);

            GetChildPersonRotations(parentIndex);
            PersonRotationModelParent personPeriodModelParent;

            IPersonRotation currentPersonRotation = FilteredStateHolder.GetCurrentPersonRotation(_childrenPersonRotationCollection);

            if (currentPersonRotation != null)
            {
                personPeriodModelParent =
                new PersonRotationModelParent(currentPersonRotation.Person,FilteredStateHolder.CommonNameDescription);

                if (currentPersonRotation.Id == null || canBold)
                {
                    personPeriodModelParent.CanBold = true;
                }
                else
                {
                    ReadOnlyCollection<PersonRotationModelChild> cachedCollection = null;

                    if (grid != null)
                    {
                        cachedCollection = grid.Tag as
                            ReadOnlyCollection<PersonRotationModelChild>;
                    }

                    personPeriodModelParent.CanBold = PeopleAdminHelper.IsCanBold(currentPersonRotation,
                    cachedCollection);

                }

                FilteredStateHolder.ParentPersonRotationCollection[rowIndex] = currentPersonRotation;

                personPeriodModelParent.PersonRotation = currentPersonRotation;
                personPeriodModelParent.CurrentRotation = currentPersonRotation.Rotation;
                personPeriodModelParent.RotationCount = _childrenPersonRotationCollection.Count == 1
                                                                      ? 0
                                                                      : _childrenPersonRotationCollection.Count;


            }
            else
            {
                personPeriodModelParent = new PersonRotationModelParent(selectedPersonRotation.Person,FilteredStateHolder.CommonNameDescription);
                personPeriodModelParent.PersonRotation = currentPersonRotation;
                personPeriodModelParent.RotationCount = _childrenPersonRotationCollection.Count + 1;

                FilteredStateHolder.ParentPersonRotationCollection[rowIndex] = WorksheetStateHolder.GetSamplePersonRotation(selectedPersonRotation.Person);
            }
            FilteredStateHolder.PersonRotationParentAdapterCollection[parentIndex] = personPeriodModelParent;
            personPeriodModelParent.GridControl = grid;
        }

        /// <summary>
        /// Resets the the parent item when a item is deleted
        /// </summary>
        /// <param name="rowIndex"></param>
        public void GetParentPersonRotationWhenDeleted(int rowIndex)
        {
            IPerson person = FilteredStateHolder.PersonRotationParentAdapterCollection[rowIndex].Person;

            IPersonRotation selectedPersonRotation =
                FilteredStateHolder.ParentPersonRotationCollection.FirstOrDefault(p => p.Person.Equals(person));

            if (selectedPersonRotation == null) return;
            int index = FilteredStateHolder.ParentPersonRotationCollection.IndexOf(selectedPersonRotation);

            PersonRotationModelParent personRotationModelParent;

            GetChildPersonRotations(rowIndex);

            IPersonRotation currentPersonRotation = FilteredStateHolder.GetCurrentPersonRotation(_childrenPersonRotationCollection);

            if (currentPersonRotation != null)
            {
                personRotationModelParent = new PersonRotationModelParent(currentPersonRotation.Person,FilteredStateHolder.CommonNameDescription);

                personRotationModelParent.PersonRotation = currentPersonRotation;

                if(currentPersonRotation.Id == null)
                    personRotationModelParent.CanBold = true;

                personRotationModelParent.FromDate = currentPersonRotation.StartDate;
                personRotationModelParent.CurrentRotation = currentPersonRotation.Rotation;
                personRotationModelParent.RotationCount = _childrenPersonRotationCollection.Count > 1 ? _childrenPersonRotationCollection.Count : 0;

                FilteredStateHolder.ParentPersonRotationCollection[index] = currentPersonRotation;
            }
            else
            {
                personRotationModelParent = new PersonRotationModelParent(selectedPersonRotation.Person,FilteredStateHolder.CommonNameDescription);

                personRotationModelParent.PersonRotation = currentPersonRotation;
                personRotationModelParent.RotationCount = _childrenPersonRotationCollection.Count > 0 ? 2 : 0;

                FilteredStateHolder.ParentPersonRotationCollection[index] = WorksheetStateHolder.GetSamplePersonRotation(selectedPersonRotation.Person);
            }

            FilteredStateHolder.PersonRotationParentAdapterCollection[rowIndex] = personRotationModelParent;
        }

        /// <summary>
        /// Sets the children person rotation collection.
        /// </summary>
        /// <param name="childrenPersonRotationCollection">The children person rotation collection.</param>
        /// <remarks>
        public void SetChildrenPersonRotationCollection(object childrenPersonRotationCollection)
        {
            IList<PersonRotationModelChild> childs =
                childrenPersonRotationCollection as IList<PersonRotationModelChild>;
            if (childs != null)
            {
                _childrenPersonRotationCollection.Clear();
                foreach (PersonRotationModelChild child in childs)
                {
                    _childrenPersonRotationCollection.Add(child.PersonRotation);
                }
            }
        }

        /// <summary>
        /// Gets the current entity.
        /// </summary>
        /// <param name="grid">The grid.</param>
        /// <param name="gridInCellColumnIndex">Index of the grid in cell column.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/19/2009
        /// </remarks>
        public object GetCurrentEntity(GridControl grid, int gridInCellColumnIndex)
        {
            IRotation rotation = null;

            if (grid.Model.SelectedRanges.Count > PeopleAdminConstants.GridSelectedRangesCountBoundary)
            {
                GridRangeInfo gridRangeInfo = grid.Model.SelectedRanges[0];

                int selectedIndex = gridRangeInfo.Top;

                if (gridRangeInfo.Top <= 0)
                    return null;

                if (!_filteredPeopleHolder.PersonRotationParentAdapterCollection[selectedIndex - 1].ExpandState)
                {
                    // get Parent rotation now
                    rotation = _filteredPeopleHolder.PersonRotationParentAdapterCollection[selectedIndex - 1].CurrentRotation;
                }
                else
                {
                    // get from child rotation 
                    CellEmbeddedGrid childGrid = grid[selectedIndex, gridInCellColumnIndex].Control as CellEmbeddedGrid;

                    if (childGrid != null && childGrid.Model.SelectedRanges.Count > 0)
                    {

                        GridRangeInfo childGridRangeInfo = childGrid.Model.SelectedRanges[0];

                        ReadOnlyCollection<PersonRotationModelChild> personRotationChildCollection =
                            childGrid.Tag as ReadOnlyCollection<PersonRotationModelChild>;

                        if (personRotationChildCollection!=null)
                            rotation = personRotationChildCollection[childGridRangeInfo.Top - 1].CurrentRotation;
                    }

                }
            }

            return rotation;
        }

        /// <summary>
        /// Removes the new person rotation.
        /// </summary>
        /// <param name="rotationToBeRemoved">The rotation to be removed.</param>
        /// <param name="filteredPeopleHolder">The filtered people holder.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-11-06
        /// </remarks>
        private static void RemoveNewPersonRotation(IPersonRotation rotationToBeRemoved, 
            FilteredPeopleHolder filteredPeopleHolder)
        {
            if (rotationToBeRemoved != null && !rotationToBeRemoved.Id.HasValue)
            {
                if (filteredPeopleHolder.AllPersonRotationCollection.Contains(rotationToBeRemoved))
                {
                    filteredPeopleHolder.AllPersonRotationCollection.Remove(rotationToBeRemoved);
                    filteredPeopleHolder.DeleteNewPersonRotation(rotationToBeRemoved);
                }
            }
        }
    }
}
