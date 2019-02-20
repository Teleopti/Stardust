using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class PeopleLoader : IPeopleLoader
    {
        private ICollection<IPerson> _peopleInOrg;
        private readonly IPersonRepository _personRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ISelectedEntitiesForPeriod _selectedEntitiesForPeriod;
        private readonly ISkillRepository _skillRepository;

        public PeopleLoader(IPersonRepository personRepository, IContractRepository contractRepository, 
            ISelectedEntitiesForPeriod selectedEntitiesForPeriod, ISkillRepository skillRepository)
        {
            _personRepository = personRepository;
            _contractRepository = contractRepository;
            _selectedEntitiesForPeriod = selectedEntitiesForPeriod;
            _skillRepository = skillRepository;
        }

        public void Initialize(ISchedulerStateHolder stateHolder)
        {
            filterPersonsInOrg(stateHolder);
        }

        public ICollection<IPerson> PeopleInOrg()
        {
            return _peopleInOrg ?? (_peopleInOrg =
                    _personRepository.FindAllAgents(_selectedEntitiesForPeriod.SelectedPeriod, true));
        }

        private void filterPersonsInOrg(ISchedulerStateHolder stateHolder) 
        {
#pragma warning disable 618
            using(_contractRepository.UnitOfWork.DisableFilter(QueryFilter.Deleted))
#pragma warning restore 618
            {
                _contractRepository.FindAllContractByDescription();
                // same uow
                _skillRepository.LoadAll();
            }
            
            bool isTeam = false;
            bool isPerson = false;

            var selectedEntities = _selectedEntitiesForPeriod.SelectedEntities;
            var firstSelectedEntity = selectedEntities.FirstOrDefault();
            if (firstSelectedEntity!=null)
            {
                isTeam = firstSelectedEntity is ITeam;
                isPerson = firstSelectedEntity is IPerson;
            }

            foreach (IPerson person in PeopleInOrg())
            {
                if (isPerson)
                {
                    if (selectedEntities.Contains(person))
						stateHolder.ChoosenAgents.Add(person);
                }
                if (isTeam)
                {
                    foreach (var team in PersonTeams(person))
                    {
                        if (selectedEntities.Contains(team))
                        {
							stateHolder.ChoosenAgents.Add(person);
                            break;
                        }
                    }
                }

				foreach (var optionalColumnValue in person.OptionalColumnValueCollection)
				{
					if(!LazyLoadingManager.IsInitialized(optionalColumnValue.ReferenceObject))
						LazyLoadingManager.Initialize(optionalColumnValue.ReferenceObject);
				}
            }
			stateHolder.SchedulingResultState.LoadedAgents = _peopleInOrg;
			stateHolder.ResetFilteredPersons();
        }

        private ICollection<ITeam> PersonTeams(IPerson person)
        {
            ICollection<ITeam> ret = new HashSet<ITeam>();
            DateOnlyPeriod dateOnlyPeriod =
                _selectedEntitiesForPeriod.SelectedPeriod;
            IList<IPersonPeriod> periods = person.PersonPeriods(dateOnlyPeriod);
            foreach (var personPeriod in periods)
            {
                ret.Add(personPeriod.Team);
            }
            return ret;
        }
    }

    public interface ISelectedEntitiesForPeriod
    {
        IEnumerable<IEntity> SelectedEntities { get; }
        DateOnlyPeriod SelectedPeriod { get; }
    }

    public class IntradaySelectedEntitiesForPeriod : ISelectedEntitiesForPeriod
    {
        private readonly IntradayMainModel _model;

        public IntradaySelectedEntitiesForPeriod(IntradayMainModel model)
        {
            _model = model;
        }

        public IEnumerable<IEntity> SelectedEntities
        {
            get { return _model.EntityCollection; }
        }

        public DateOnlyPeriod SelectedPeriod
        {
            get { return _model.Period; }
        }
    }

    public class SelectedEntitiesForPeriod : ISelectedEntitiesForPeriod
    {
        private readonly IEnumerable<IEntity> _selectedEntities;
        private readonly DateOnlyPeriod _period;

        public SelectedEntitiesForPeriod(IEnumerable<IEntity> selectedEntities, DateOnlyPeriod period)
        {
            _selectedEntities = selectedEntities;
            _period = period;
        }

        public IEnumerable<IEntity> SelectedEntities
        {
            get { return _selectedEntities; }
        }

        public DateOnlyPeriod SelectedPeriod
        {
            get { return _period; }
        }
    }
}
