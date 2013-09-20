using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.SimpleSample.Model;
using Teleopti.Ccc.Sdk.SimpleSample.Repositories;

namespace Teleopti.Ccc.Sdk.SimpleSample.ViewModel
{
    public class FindAllPeopleCommand : ICommand
    {
        private readonly PeopleViewModel _peopleViewModel;
        private readonly PersonRepository _personRepository;
        private readonly ContractRepository _contractRepository;
        private readonly PartTimePercentageRepository _partTimePercentageRepository;
        private readonly ContractScheduleRepository _contractScheduleRepository;
        private readonly SkillRepository _skillRepository;
        private readonly BusinessHierarchyRepository _businessHierarchyRepository;
        private readonly PersonPeriodRepository _personPeriodRepository;

        public FindAllPeopleCommand(PeopleViewModel peopleViewModel, PersonRepository personRepository, ContractRepository contractRepository, PartTimePercentageRepository partTimePercentageRepository, ContractScheduleRepository contractScheduleRepository, SkillRepository skillRepository, BusinessHierarchyRepository businessHierarchyRepository, PersonPeriodRepository personPeriodRepository)
        {
            _peopleViewModel = peopleViewModel;
            _personRepository = personRepository;
            _contractRepository = contractRepository;
            _partTimePercentageRepository = partTimePercentageRepository;
            _contractScheduleRepository = contractScheduleRepository;
            _skillRepository = skillRepository;
            _businessHierarchyRepository = businessHierarchyRepository;
            _personPeriodRepository = personPeriodRepository;
            _peopleViewModel.PropertyChanged += _peopleViewModel_PropertyChanged;
        }

        private void _peopleViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }


        public void Execute(object parameter)
        {
            var startDate = new DateOnlyDto(_peopleViewModel.StartDate.Year, _peopleViewModel.StartDate.Month,
                                            _peopleViewModel.StartDate.Day);
            var endDate = new DateOnlyDto(_peopleViewModel.EndDate.Year, _peopleViewModel.EndDate.Month,
                                            _peopleViewModel.EndDate.Day);

            initializeRepositories(startDate, endDate);

            _peopleViewModel.FoundPeople.Clear();
            var businessUnit = _businessHierarchyRepository.GetBusinessUnit();
            var personDetailContainers = _personPeriodRepository.GetPersonPeriods();
            foreach (var personDetailContainer in personDetailContainers)
            {
                var personId = personDetailContainer.PersonPeriod.PersonId;

                personDetailContainer.Person = _personRepository.GetById(personId);

                if (personDetailContainer.Person == null) continue;

                personDetailContainer.Contract = _contractRepository.GetById(personDetailContainer.PersonPeriod.ContractId).Description;
                personDetailContainer.ContractSchedule = _contractScheduleRepository.GetById(personDetailContainer.PersonPeriod.ContractScheduleId).Description;

                var partTimePercentage = _partTimePercentageRepository.GetById(personDetailContainer.PersonPeriod.PartTimePercentageId);
                if (partTimePercentage != null)
                {
                    personDetailContainer.PartTimePercentageName = partTimePercentage.Description;
                    personDetailContainer.PartTimePercentageValue = partTimePercentage.Percentage.ToString("P2");
                }

                var site = _businessHierarchyRepository.GetSiteByTeam(personDetailContainer.PersonPeriod.Team.Id.GetValueOrDefault());
                personDetailContainer.Site = site.Site;

                _peopleViewModel.FoundPeople.Add(new PersonDetailModel
                {
                    Team = personDetailContainer.PersonPeriod.Team.Description,
                    StartPeriod =
                        personDetailContainer.PersonPeriod.StartDate.DateTime,
                    EndPeriod =
                        personDetailContainer.PersonSkillPeriod.DateTo.DateTime,
                    FirstName = personDetailContainer.Person.FirstName,
                    LastName = personDetailContainer.Person.LastName,
                    Skills =
                        string.Join(", ",
                                    personDetailContainer.PersonSkillPeriod.
                                        SkillCollection.Select(
                                            s => _skillRepository.GetById(s).Name)),
                    ExternalLogOns =
                        string.Join(", ",
                                    personDetailContainer.PersonPeriod.
                                        ExternalLogOn.Select(
                                            e => e.AcdLogOnOriginalId)),
                    Note = personDetailContainer.PersonPeriod.Note,
                    Contract = personDetailContainer.Contract,
                    ContractSchedule = personDetailContainer.ContractSchedule,
                    PartTimePercentageName =
                        personDetailContainer.PartTimePercentageName,
                    PartTimePercentageValue =
                        personDetailContainer.PartTimePercentageValue,
                    Site = personDetailContainer.Site.Name,
                    BusinessUnit = businessUnit.Name,
                    IsDeleted = personDetailContainer.Person.IsDeleted,
                    LeavingDate = (personDetailContainer.Person.TerminationDate != null) ? personDetailContainer.Person.TerminationDate.DateTime : (DateTime?)null,
                });
            }

           _peopleViewModel.ResultCountVisible = Visibility.Visible;
        }

        private void initializeRepositories(DateOnlyDto startDate, DateOnlyDto endDate)
        {
            _contractRepository.Initialize();
            _personRepository.Initialize();
            _contractScheduleRepository.Initialize();
            _partTimePercentageRepository.Initialize();
            _skillRepository.Initialize();
            _personPeriodRepository.Initialize(startDate, endDate);
            _businessHierarchyRepository.Initialize();
        }

        public bool CanExecute(object parameter)
        {
            return _peopleViewModel.StartDate <= _peopleViewModel.EndDate;
        }

        public event EventHandler CanExecuteChanged;
    }
}