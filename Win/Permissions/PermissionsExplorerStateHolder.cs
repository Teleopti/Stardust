using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Permissions
{
    /// <summary>
    /// Permissions State Holder to hold Permissions Explorer instance data 
    /// </summary>
    /// <remarks>
    /// Created by: Muhamad Risath
    /// Created date: 11/17/2008
    /// </remarks>
    public class PermissionsExplorerStateHolder : IDisposable
    {
        private IUnitOfWork _unitOfWork;
        private readonly FixedCapacityStack<ApplicationRoleAdapter> _copyPasteStack;
        private readonly Dictionary<IApplicationRole, PermissionsDataHolder> _permissionsDataDictionary = new Dictionary<IApplicationRole, PermissionsDataHolder>();
        private readonly ICollection<PersonAdapter> _personAdapterCollection = new List<PersonAdapter>();

        public IUnitOfWork UnitOfWork
        {
            get
            {
                if (_unitOfWork == null)
                {
                    _unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork();
                }
                return _unitOfWork;
            }
        }

        public FixedCapacityStack<ApplicationRoleAdapter> CopyPasteStack
        {
            get { return _copyPasteStack; }
        }

        /// <summary>
        /// Gets the person adapter collection.
        /// </summary>
        /// <value>The person adapter collection.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public ICollection<PersonAdapter> PersonAdapterCollection
        {
            get
            {
                return _personAdapterCollection;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_unitOfWork != null)
                {
                    _unitOfWork.Dispose();
                    _unitOfWork = null;
                }
                _copyPasteStack.Clear();
            }
        }

        public PermissionsExplorerStateHolder()
        {
            _copyPasteStack = new FixedCapacityStack<ApplicationRoleAdapter>(1024);
        }

        /// <summary>
        /// Adds the or update application role.
        /// </summary>
        /// <param name="applicationRole">The application role.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public void AddOrUpdateApplicationRole(IApplicationRole applicationRole)
        {
            IApplicationRoleRepository applicationRoleRepository = new ApplicationRoleRepository(UnitOfWork);
            applicationRoleRepository.Add(applicationRole);
        }

        /// <summary>
        /// Deletes the application role.
        /// </summary>
        /// <param name="applicationRole">The application role.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public void DeleteApplicationRole(IApplicationRole applicationRole)
        {
            if (applicationRole != null)
            {
                IApplicationRoleRepository applicationRoleRepository = new ApplicationRoleRepository(UnitOfWork);
                applicationRoleRepository.Remove(applicationRole);
            }
        }

        /// <summary>
        /// Adds the or update available data.
        /// </summary>
        /// <param name="availableData">The available data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public void AddOrUpdateAvailableData(IAvailableData availableData)
        {
            IAvailableDataRepository availableDataRepository = new AvailableDataRepository(UnitOfWork);
            availableDataRepository.Add(availableData);
        }

        /// <summary>
        /// Deletes the available data.
        /// </summary>
        /// <param name="availableData">The available data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public void DeleteAvailableData(IAvailableData availableData)
        {
            if (availableData != null)
            {
                IAvailableDataRepository availableDataRepository = new AvailableDataRepository(UnitOfWork);
                availableDataRepository.Remove(availableData);
            }
        }

        /// <summary>
        /// Updates the people with application role.
        /// </summary>
        /// <param name="personCollection">The person collection.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public void UpdatePeopleWithApplicationRole(ICollection<IPerson> personCollection)
        {
            IPersonRepository personRepository = new PersonRepository(UnitOfWork);
            // Queue all selected personCollection on repository.
            personRepository.AddRange(personCollection);
        }

        /// <summary>
        /// Deletes the application role from person.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public void DeleteApplicationRoleFromPerson(IPerson person)
        {
            if (person != null)
            {
                IPersonRepository personRepository = new PersonRepository(UnitOfWork);
                personRepository.Add(person);
            }
        }

        /// <summary>
        /// Persists all.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-09-09
        /// </remarks>
        public void PersistAll()
        {
            //TODO: Check the way the error is handled?
            try
            {
                UnitOfWork.PersistAll();
            }
            catch (OptimisticLockException optimisticLockException)
            {
                string errorText = "Someone else have changed " + "[" + optimisticLockException.EntityName + ":" + optimisticLockException.RootId + "]";
                throw new OptimisticLockException(errorText);
            }
            finally
            {
                _unitOfWork.Dispose();
                _unitOfWork = null;
            }
        }

        /// <summary>
        /// Adds the and save available data.
        /// </summary>
        /// <param name="availableData">The available data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/9/2008
        /// </remarks>
        public void AddAndSaveAvailableData(IAvailableData availableData)
        {
            if (availableData != null)
            {
                IAvailableDataRepository repository = new AvailableDataRepository(UnitOfWork);
                repository.Add(availableData);
            }
        }

        /// <summary>
        /// Determines whether [is lazy loaded] [the specified id].
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>
        /// 	<c>true</c> if [is lazy loaded] [the specified id]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public bool IsLazyLoaded(Guid? id)
        {
            bool result = true;

            IEnumerable<PersonAdapter> personAdapters = _personAdapterCollection.Where(pa => pa.Person.Id == id);

            foreach (PersonAdapter adapter in personAdapters)
            {
                result = adapter.IsLazyLoaded;
                break;
            }

            return result;
        }

        /// <summary>
        /// Sets the lazy loaded.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void SetLazyLoaded(Guid? id)
        {
            IEnumerable<PersonAdapter> personAdapters = _personAdapterCollection.Where(pa => pa.Person.Id == id);

            foreach (PersonAdapter adapter in personAdapters)
            {
                adapter.IsLazyLoaded = false;
                break;
            }
        }

        /// <summary>
        /// Queues the dirty people collection.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void QueueDirtyPeopleCollection()
        {
            IPersonRepository personRepository = new PersonRepository(UnitOfWork);

            foreach (PersonAdapter adapter in _personAdapterCollection)
            {
                if (adapter.IsDirty)
                {
                    // Queue all selected personCollection on repository.
                    personRepository.Add(adapter.Person);
                    adapter.IsDirty = false;
                }
            }
        }

        /// <summary>
        /// Marks as dirty.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void MarkAsDirty(Guid? id)
        {
            IEnumerable<PersonAdapter> personAdapters = _personAdapterCollection.Where(pa => pa.Person.Id == id);

            foreach (PersonAdapter adapter in personAdapters)
            {
                adapter.IsDirty = true;
                break;
            }
        }

        /// <summary>
        /// Assigns the person in permissions data dictionary.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="personCollection">The person collection.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void AssignPersonInPermissionsDataDictionary(IApplicationRole role, ICollection<IPerson> personCollection)
        {
            if (personCollection != null && personCollection.Count > 0)
            {
                if (_permissionsDataDictionary.ContainsKey(role))
                {
                    foreach (IPerson person in personCollection)
                    {
                        _permissionsDataDictionary[role].AddPersonToCollection(person);
                    }
                }
            }
        }

        /// <summary>
        /// Removes the person in permissions data dictionary.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void RemovePersonInPermissionsDataDictionary(IApplicationRole role, IPerson person)
        {
            if (_permissionsDataDictionary.ContainsKey(role))
            {
                _permissionsDataDictionary[role].RemovePersonFromCollection(person);
            }
        }

        /// <summary>
        /// Gets the person in permissions data dictionary.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public ICollection<IPerson> GetPersonInPermissionsDataDictionary(IApplicationRole role)
        {
            if (_permissionsDataDictionary.ContainsKey(role))
            {
                var builtInUsers = from p in _permissionsDataDictionary[role].PersonCollection
                                   where p.BuiltIn
                                   select p;
                foreach (IPerson builtInUser in builtInUsers.ToList())
                {
                    _permissionsDataDictionary[role].PersonCollection.Remove(builtInUser);
                }
                return _permissionsDataDictionary[role].PersonCollection;
            }

            return null;
        }

        /// <summary>
        /// Gets the available data in permissions data dictionary.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public IAvailableData GetAvailableDataInPermissionsDataDictionary(IApplicationRole role)
        {
            if (_permissionsDataDictionary.ContainsKey(role))
            {
                return _permissionsDataDictionary[role].AvailableData;
            }

            return null;
        }

        /// <summary>
        /// Determines whether [is person in the list] [the specified id].
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public IPerson IsPersonInTheList(Guid? id)
        {
            IEnumerable<PersonAdapter> personAdapters = _personAdapterCollection.Where(pa => pa.Person.Id == id);

            foreach (PersonAdapter adapter in personAdapters)
            {
                return adapter.Person;
            }

            return null;
        }

        /// <summary>
        /// Saves the application role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void SaveApplicationRole(IApplicationRole role)
        {
            if (role != null)
            {
                IApplicationRoleRepository repository = new ApplicationRoleRepository(UnitOfWork);
                repository.Add(role);
            }
        }

        /// <summary>
        /// Adds the role to permissions data dictionary.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="permissionsDataHolder">The permissions data holder.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void AddRoleToPermissionsDataDictionary(IApplicationRole role, PermissionsDataHolder permissionsDataHolder)
        {
            if (!_permissionsDataDictionary.ContainsKey(role))
            {
                _permissionsDataDictionary.Add(role, permissionsDataHolder);
            }
        }

        /// <summary>
        /// Removes the role from permissions data dictionary.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void RemoveRoleFromPermissionsDataDictionary(IApplicationRole role)
        {
            if (_permissionsDataDictionary.ContainsKey(role))
            {
                _permissionsDataDictionary.Remove(role);
            }
        }

        /// <summary>
        /// Assigns the available data in permissions data dictionary.
        /// </summary>
        /// <param name="availableData">The available data.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void AssignAvailableDataInPermissionsDataDictionary(IAvailableData availableData)
        {
            if (availableData != null)
            {
                if (_permissionsDataDictionary.ContainsKey(availableData.ApplicationRole))
                {
                    _permissionsDataDictionary[availableData.ApplicationRole].AvailableData = availableData;
                }
            }
        }

        /// <summary>
        /// Adds the person adapter.
        /// </summary>
        /// <param name="personAdapter">The person adapter.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void AddPersonAdapter(PersonAdapter personAdapter)
        {
            if (_personAdapterCollection != null && _personAdapterCollection.Count == 0)
            {
                _personAdapterCollection.Add(personAdapter);
            }
            else
            {
                IPerson person = IsPersonInTheList(personAdapter.Person.Id);

                if (person == null)
                {
                    _personAdapterCollection.Add(personAdapter);
                }
            }
        }

        /// <summary>
        /// Adds to person adapter collection.
        /// </summary>
        /// <param name="personCollection">The person collection.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 11/17/2008
        /// </remarks>
        public void AddToPersonAdapterCollection(ICollection<IPerson> personCollection)
        {
            foreach (IPerson person in personCollection)
            {
                AddPersonAdapter(new PersonAdapter(person));
            }
        }

        public bool CanPaste()
        {
            return _copyPasteStack.Count > 0;
        }

        public void Reassociate(IUnitOfWork theUnit)
        {
            _unitOfWork = theUnit;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        public void CopySitesAndTeam(IAvailableData sourceAvailableData, IAvailableData availableDataToSave)
        {
            InParameter.NotNull("availableDataToSave", availableDataToSave);

            AvailableDataRepository repository = new AvailableDataRepository(UnitOfWork);
            IAvailableData availableData = repository.LoadAllCollectionsInAvailableData(sourceAvailableData);

            foreach (ITeam team in availableData.AvailableTeams)
            {
                availableDataToSave.AddAvailableTeam(team);
            }

            foreach (ISite site in availableData.AvailableSites)
            {
                availableDataToSave.AddAvailableSite(site);
                foreach (ITeam team in site.TeamCollection)
                {
                    availableDataToSave.AddAvailableTeam(team);
                }
            }
            
        }
    }
}
