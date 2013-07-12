using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
    public class PeopleLoaderForTeamLeaderMode : IPeopleLoader
    {
        private ICollection<IPerson> _peopleInOrg;
        private readonly IUnitOfWork _unitOfWork;
    	private readonly ISchedulerStateHolder _schedulerStateHolder;
        private readonly ISelectedEntitiesForPeriod _selectedEntitiesForPeriod;
    	private readonly IRepositoryFactory _repositoryFactory;

    	public PeopleLoaderForTeamLeaderMode(IUnitOfWork unitOfWork, ISchedulerStateHolder schedulerStateHolder, ISelectedEntitiesForPeriod selectedEntitiesForPeriod, IRepositoryFactory repositoryFactory)
        {
            _unitOfWork = unitOfWork;
        	_schedulerStateHolder = schedulerStateHolder;
            _selectedEntitiesForPeriod = selectedEntitiesForPeriod;
        	_repositoryFactory = repositoryFactory;
        }

        public ISchedulerStateHolder Initialize()
        {
			using (_unitOfWork.DisableFilter(QueryFilter.Deleted))
            {
                _repositoryFactory.CreateContractRepository(_unitOfWork).FindAllContractByDescription();
				_repositoryFactory.CreateSkillRepository(_unitOfWork).LoadAll();
				_repositoryFactory.CreatePartTimePercentageRepository(_unitOfWork).LoadAll();
            	_repositoryFactory.CreateRuleSetBagRepository(_unitOfWork).LoadAllWithRuleSets();
            	_repositoryFactory.CreateWorkShiftRuleSetRepository(_unitOfWork).FindAllWithLimitersAndExtenders();
            	_repositoryFactory.CreateWorkflowControlSetRepository(_unitOfWork).LoadAll();
            	_repositoryFactory.CreateSiteRepository(_unitOfWork).LoadAll();
            	_repositoryFactory.CreateTeamRepository(_unitOfWork).LoadAll();
            }

            foreach (IPerson person in peopleInOrg())
            {
                if (person.TerminalDate == null || person.TerminalDate >= _selectedEntitiesForPeriod.SelectedPeriod.StartDate)
                _schedulerStateHolder.AllPermittedPersons.Add(person);
            }

            _schedulerStateHolder.SchedulingResultState.PersonsInOrganization = _schedulerStateHolder.AllPermittedPersons;
            _schedulerStateHolder.ResetFilteredPersons();

            return _schedulerStateHolder;
        }

        private IEnumerable<IPerson> peopleInOrg()
        {
            return _peopleInOrg ?? loadPeople();
        }

        private ICollection<IPerson> loadPeople()
        {
            _peopleInOrg = new List<IPerson>();

            bool isTeam = false;
            bool isPerson = false;

            var selectedEntities = _selectedEntitiesForPeriod.SelectedEntities;
            var firstSelectedEntity = selectedEntities.FirstOrDefault();
            if (firstSelectedEntity != null)
            {
                isTeam = firstSelectedEntity is ITeam;
                isPerson = firstSelectedEntity is IPerson;
            }

            if(isTeam)
            {
                foreach (var entity in selectedEntities)
                {
                    var team = (ITeam) entity;
					ICollection<IPerson> teamMembers =
                        _repositoryFactory.CreatePersonRepository(_unitOfWork)
                        .FindPeopleBelongTeamWithSchedulePeriod(team, _selectedEntitiesForPeriod.SelectedPeriod);

                    foreach (var teamMember in teamMembers)
                    {
						if (!_peopleInOrg.Contains(teamMember)) 
							_peopleInOrg.Add(teamMember);
                    }
                }
            }

            if(isPerson)
            {
                IList<IPerson> personList = new List<IPerson>();
                foreach (var entity in selectedEntities)
                {
                    var person = (IPerson) entity;
                    personList.Add(person);
                }

				_peopleInOrg =
                    _repositoryFactory.CreatePersonRepository(_unitOfWork)
                    .FindPeople(personList);
            }

            return _peopleInOrg;
        }
    }
}