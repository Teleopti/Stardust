using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public class PeopleLoaderForTeamLeaderMode : IPeopleLoader
    {
        private readonly Lazy<ICollection<IPerson>> _peopleInOrg;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISelectedEntitiesForPeriod _selectedEntitiesForPeriod;
    	private readonly IRepositoryFactory _repositoryFactory;

    	public PeopleLoaderForTeamLeaderMode(IUnitOfWork unitOfWork, ISelectedEntitiesForPeriod selectedEntitiesForPeriod, IRepositoryFactory repositoryFactory)
        {
            _unitOfWork = unitOfWork;
            _selectedEntitiesForPeriod = selectedEntitiesForPeriod;
        	_repositoryFactory = repositoryFactory;
			_peopleInOrg = new Lazy<ICollection<IPerson>>(loadPeople);
        }

        public void Initialize(ISchedulerStateHolder stateHolder)
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
				_repositoryFactory.CreateBudgetGroupRepository(_unitOfWork).LoadAll();
			}

            foreach (IPerson person in _peopleInOrg.Value)
            {
                if (person.TerminalDate == null || person.TerminalDate >= _selectedEntitiesForPeriod.SelectedPeriod.StartDate)
                stateHolder.ChoosenAgents.Add(person);
            }

            stateHolder.SchedulingResultState.LoadedAgents = stateHolder.ChoosenAgents;
            stateHolder.ResetFilteredPersons();
        }

        private ICollection<IPerson> loadPeople()
        {
            var peopleInOrg = new List<IPerson>();
            var peopleInTeam = new Dictionary<Guid, IPerson>();

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
						if (!peopleInTeam.ContainsKey(teamMember.Id.GetValueOrDefault()))
							peopleInTeam.Add(teamMember.Id.GetValueOrDefault(), teamMember);
                    }
                }
	            peopleInOrg = peopleInTeam.Values.ToList();
            }

            if(isPerson)
            {
                IList<IPerson> personList = selectedEntities.OfType<IPerson>().ToList();

	            peopleInOrg =
		            _repositoryFactory.CreatePersonRepository(_unitOfWork)
		                              .FindPeople(personList).ToList();
            }

            return peopleInOrg;
        }
    }
}