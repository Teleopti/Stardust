using System;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using Teleopti.Ccc.Sdk.Common.Contracts;
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
            if (handler!=null)
            {
                handler.Invoke(this,EventArgs.Empty);
            }
        }

        public void Execute(object parameter)
        {
            var startDate = new DateOnlyDto(_peopleViewModel.StartDate.Year, _peopleViewModel.StartDate.Month,
                                            _peopleViewModel.StartDate.Day);
            var endDate = new DateOnlyDto(_peopleViewModel.EndDate.Year, _peopleViewModel.EndDate.Month,
                                            _peopleViewModel.EndDate.Day);

            ITeleoptiOrganizationService organisationService = initializeRepositories(startDate, endDate);

            var periods = _personPeriodRepository.AllPersonPeriodDetails();
            var personDetailContainers = periods.Select(p => new PersonDetailContainer { PersonPeriod = p }).ToList();

            foreach (var personDetailContainer in personDetailContainers)
            {
                var personId = personDetailContainer.PersonPeriod.PersonId;
                personDetailContainer.Person = _personRepository.GetById(personId);

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
            }

            var skillPeriods =
                organisationService.GetPersonSkillPeriodsForPersons(
                    personDetailContainers.Select(p => p.Person).Where(p => p != null).ToArray(), startDate, endDate);
            foreach (var personDetailContainer in personDetailContainers)
            {
                var personId = personDetailContainer.PersonPeriod.PersonId;
                var periodStartDate = personDetailContainer.PersonPeriod.StartDate.DateTime;
                personDetailContainer.PersonSkillPeriod =
                    skillPeriods.FirstOrDefault(
                        s => personId.Equals(s.PersonId) && s.DateFrom.DateTime == periodStartDate);
            }

            _peopleViewModel.FoundPeople.Clear();

            foreach (var personDetailContainer in personDetailContainers.Where(p => p.Person!=null))
            {
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
                                                         IsDeleted = personDetailContainer.Person.IsDeleted,
                                                         LeavingDate = (personDetailContainer.Person.TerminationDate!=null) ? personDetailContainer.Person.TerminationDate.DateTime : (DateTime?)null,
                                                     });
            }

            _peopleViewModel.ResultCountVisible = Visibility.Visible;
        }

        private ITeleoptiOrganizationService initializeRepositories(DateOnlyDto startDate, DateOnlyDto endDate)
        {
            var organisationService = new ChannelFactory<ITeleoptiOrganizationService>(typeof(ITeleoptiOrganizationService).Name).CreateChannel();
            var forecastingService = new ChannelFactory<ITeleoptiForecastingService>(typeof(ITeleoptiForecastingService).Name).CreateChannel();
            
            _contractRepository.Initialize(organisationService);
            _personRepository.Initialize(organisationService);
            _contractScheduleRepository.Initialize(organisationService);
            _partTimePercentageRepository.Initialize(organisationService);
            _skillRepository.Initialize(forecastingService);
            _personPeriodRepository.Initialize(organisationService,startDate,endDate);
            _businessHierarchyRepository.Initialize(organisationService);

            return organisationService;
        }

        private class PersonDetailContainer
        {
            public PersonDto Person { get; set; }
            public PersonPeriodDetailDto PersonPeriod { get; set; }
            public PersonSkillPeriodDto PersonSkillPeriod { get; set; }
            public string Contract { get; set; }
            public string ContractSchedule { get; set; }
            public string PartTimePercentageName { get; set; }
            public string PartTimePercentageValue { get; set; }

            public SiteModel Site { get; set; }
        }

        public bool CanExecute(object parameter)
        {
            return _peopleViewModel.StartDate <= _peopleViewModel.EndDate;
        }

        public event EventHandler CanExecuteChanged;
    }
}