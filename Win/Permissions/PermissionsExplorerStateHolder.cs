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
            get { return _unitOfWork ?? (_unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork()); }
        }

        public FixedCapacityStack<ApplicationRoleAdapter> CopyPasteStack
        {
            get { return _copyPasteStack; }
        }

        public PermissionsExplorerStateHolder()
        {
            _copyPasteStack = new FixedCapacityStack<ApplicationRoleAdapter>(1024);
        }

        public void AddOrUpdateApplicationRole(IApplicationRole applicationRole)
        {
            IApplicationRoleRepository applicationRoleRepository = new ApplicationRoleRepository(UnitOfWork);
            applicationRoleRepository.Add(applicationRole);
        }

        public void DeleteApplicationRole(IApplicationRole applicationRole)
        {
            if (applicationRole != null)
            {
                IApplicationRoleRepository applicationRoleRepository = new ApplicationRoleRepository(UnitOfWork);
                applicationRoleRepository.Remove(applicationRole);
            }
        }

        public void AddOrUpdateAvailableData(IAvailableData availableData)
        {
            IAvailableDataRepository availableDataRepository = new AvailableDataRepository(UnitOfWork);
            availableDataRepository.Add(availableData);
        }

        public void DeleteAvailableData(IAvailableData availableData)
        {
            if (availableData != null)
            {
                IAvailableDataRepository availableDataRepository = new AvailableDataRepository(UnitOfWork);
                availableDataRepository.Remove(availableData);
            }
        }

        public void PersistAll()
        {
            try
            {
                using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
                {
                    UnitOfWork.PersistAll();
                    uow.PersistAll();
                }
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

        public void AddAndSaveAvailableData(IAvailableData availableData)
        {
            if (availableData != null)
            {
                IAvailableDataRepository repository = new AvailableDataRepository(UnitOfWork);
                repository.Add(availableData);
            }
        }

        public void QueueDirtyPeopleCollection()
        {
            IPersonRepository personRepository = new PersonRepository(new ThisUnitOfWork(UnitOfWork));

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

        public void MarkAsDirty(Guid? id)
        {
            IEnumerable<PersonAdapter> personAdapters = _personAdapterCollection.Where(pa => pa.Person.Id == id);

            foreach (PersonAdapter adapter in personAdapters)
            {
                adapter.IsDirty = true;
                break;
            }
        }

        public void AssignPersonInPermissionsDataDictionary(IApplicationRole role, ICollection<IPersonInRole> personCollection)
        {
            if (personCollection != null && personCollection.Count > 0)
            {
                if (_permissionsDataDictionary.ContainsKey(role))
                {
                    foreach (IPersonInRole person in personCollection)
                    {
                        _permissionsDataDictionary[role].AddPersonToCollection(person);
                    }
                }
            }
        }

        public void RemovePersonInPermissionsDataDictionary(IApplicationRole role, IPersonInRole person)
        {
            if (_permissionsDataDictionary.ContainsKey(role))
            {
                _permissionsDataDictionary[role].RemovePersonFromCollection(person);
            }
        }

        public ICollection<IPersonInRole> GetPersonInPermissionsDataDictionary(IApplicationRole role)
        {
            if (_permissionsDataDictionary.ContainsKey(role))
            {
                return _permissionsDataDictionary[role].PersonCollection;
            }

            return null;
        }

        public IAvailableData GetAvailableDataInPermissionsDataDictionary(IApplicationRole role)
        {
            if (_permissionsDataDictionary.ContainsKey(role))
            {
                return _permissionsDataDictionary[role].AvailableData;
            }

            return null;
        }

        public IPerson IsPersonInTheList(Guid? id)
        {
            IEnumerable<PersonAdapter> personAdapters = _personAdapterCollection.Where(pa => pa.Person.Id == id);

            foreach (PersonAdapter adapter in personAdapters)
            {
                return adapter.Person;
            }

            return null;
        }

        public void SaveApplicationRole(IApplicationRole role)
        {
            if (role != null)
            {
                IApplicationRoleRepository repository = new ApplicationRoleRepository(UnitOfWork);
                repository.Add(role);
            }
        }

        public void AddRoleToPermissionsDataDictionary(IApplicationRole role, PermissionsDataHolder permissionsDataHolder)
        {
            if (!_permissionsDataDictionary.ContainsKey(role))
            {
                _permissionsDataDictionary.Add(role, permissionsDataHolder);
            }
        }

        public void RemoveRoleFromPermissionsDataDictionary(IApplicationRole role)
        {
            if (_permissionsDataDictionary.ContainsKey(role))
            {
                _permissionsDataDictionary.Remove(role);
            }
        }

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

        public void AddPersonAdapter(PersonAdapter personAdapter)
        {
            if (_personAdapterCollection.Count == 0)
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

        public bool CanPaste()
        {
            return _copyPasteStack.Count > 0;
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
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
    }
}
