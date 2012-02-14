using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.PersonPicker
{
    /// <summary>
    /// Holds a collection of selectable persons
    /// </summary>
    /// <remarks>
    /// For filtering and grouping different views of Persons
    /// Created by: henrika
    /// Created date: 2009-06-09
    /// </remarks>
    public class PersonPickerViewModel
    {
        public IRepositoryFactory RepFactory { get; private set; }

        /// <summary>
        /// Gets or sets the load all command.
        /// </summary>
        /// <value>The load all command.</value>
        /// <remarks>
        /// Loads all people from repository
        /// Created by: henrika
        /// Created date: 2009-06-09
        /// </remarks>
        public CommandModel LoadAllCommand { get; private set; }
        public ObservableCollection<SelectablePersonViewModel> People { get; private set; }

        public PersonPickerViewModel():this(new RepositoryFactory(), UnitOfWorkFactory.Current)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxLoadAll")]
        public PersonPickerViewModel(IRepositoryFactory factory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            People = new ObservableCollection<SelectablePersonViewModel>();
            RepFactory = factory;
            LoadAllCommand = CommandModelFactory.CreateRepositoryCommandModel(LoadAllFromRepository, unitOfWorkFactory, "xxLoadAll");
        }

        private void LoadAllFromRepository(IUnitOfWork obj)
        {
            ClearAndAddToPeople(RepFactory.CreatePersonRepository(obj).LoadAll());
        }

        /// <summary>
        /// Sets the peoplecollection
        /// </summary>
        /// <param name="persons">The persons.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-09
        /// </remarks>
        public void SetPeople(IList<IPerson> persons)
        {
            ClearAndAddToPeople(persons);
        }

        private void ClearAndAddToPeople(IList<IPerson> peopleToAdd)
        {
            People.Clear();
            peopleToAdd.ForEach(p => People.Add(new SelectablePersonViewModel(p)));
        }
    }
}
