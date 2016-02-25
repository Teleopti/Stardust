using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;
using IAnalyticsPersonPeriodRepository = Teleopti.Ccc.Domain.Repositories.IAnalyticsPersonPeriodRepository;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
    [UseOnToggle(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162)]
    public class PersonPeriodAnalyticsUpdater :
        IHandleEvent<PersonCollectionChangedEvent>,
        IRunOnServiceBus
    {
        private readonly IPersonRepository _personRepository;
        private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;

        public PersonPeriodAnalyticsUpdater(IPersonRepository personRepository, IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository)
        {
            _personRepository = personRepository;
            _analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
        }

        public void Handle(PersonCollectionChangedEvent @event)
        {
            Console.WriteLine("Handle PersonCollectionChangedEvent");

            // Check if new person
            IAnalyticsPersonPeriod t = new AnalyticsPersonPeriod();

            foreach (var personCodeGuid in @event.PersonIdCollection)
            {
                Console.WriteLine(personCodeGuid.ToString());
                var person = _personRepository.FindPeople(new Guid[] { personCodeGuid }).First();
                Console.WriteLine(person.Name + " " + person.Email);
                
                var test = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid);

                Console.WriteLine("Person periods: " + person.PersonPeriodCollection.Count());

                if (test.IsEmpty() && person.PersonPeriodCollection.Any())
                {
                    Console.WriteLine("The person '{0}' does not exists yet in analytics.", person.Name);

                    foreach (var personPeriod in person.PersonPeriodCollection)
                    {
                        _analyticsPersonPeriodRepository.AddPersonPeriod(new AnalyticsPersonPeriod()
                        {
                            PersonCode = personCodeGuid,
                            ValidFromDate = personPeriod.Period.StartDate.Date,
                            ValidToDate = personPeriod.Period.EndDate.Date,
                            ValidFromDateId = -1,
                            ValidToDateId = -1,
                            ValidFromIntervalId = -1,
                            ValidToIntervalId = -1,
                            PersonPeriodCode = personPeriod.Id.GetValueOrDefault(),
                            PersonName = person.Name.ToString(),
                            FirstName = person.Name.FirstName,
                            LastName = person.Name.LastName,
                            EmploymentNumber = person.EmploymentNumber,
                            EmploymentTypeCode = -1,
                            EmploymentTypeName = "Not Defined?",
                            ContractCode = personPeriod.PersonContract.Contract.Id.GetValueOrDefault(),
                            ContractName = personPeriod.PersonContract.Contract.Description.Name,
                            ParttimeCode = personPeriod.PersonContract.PartTimePercentage.Id.GetValueOrDefault(),
                            ParttimePercentage = personPeriod.PersonContract.PartTimePercentage.Percentage.ToString(),
                            TeamId = -1,
                            TeamCode = personPeriod.Team.Id.GetValueOrDefault(),
                            TeamName = personPeriod.Team.SiteAndTeam,
                            SiteId = -1,
                            SiteCode = personPeriod.Team.Site.Id.GetValueOrDefault(),
                            SiteName = personPeriod.Team.Site.Description.Name,
                            BusinessUnitId = -1,
                            BusinessUnitCode = personPeriod.Team.BusinessUnitExplicit.Id.GetValueOrDefault(),
                            BusinessUnitName = personPeriod.Team.BusinessUnitExplicit.Name,
                            SkillsetId = -1,
                            Email = person.Email,
                            Note = person.Note,
                            EmploymentStartDate = personPeriod.Period.StartDate.Date,
                            EmploymentEndDate = personPeriod.Period.EndDate.Date,
                            TimeZoneId = -1,
                            IsAgent = person.IsAgent(new DateOnly()),
                            IsUser = false,
                            DatasourceId = 1,
                            InsertDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            DatasourceUpdateDate = person.UpdatedOn.GetValueOrDefault(),
                            ToBeDeleted = false,
                            WindowsDomain = "",
                            WindowsUsername = "",
                            ValidToDateIdMaxDate = -1,
                            ValidToIntervalIdMaxDate = -1,
                            ValidFromDateIdLocal = -1,
                            ValidToDateIdLocal = -1,
                            ValidFromDateLocal = DateTime.Now,
                            ValidToDateLocal = DateTime.Now
                        });
                    }
                }
            }
        }
    }

    internal class AnalyticsPersonPeriod : IAnalyticsPersonPeriod
    {
        public int PersonId { get; set; }
        public Guid PersonCode { get; set; }
        public DateTime ValidFromDate { get; set; }
        public DateTime ValidToDate { get; set; }
        public int ValidFromDateId { get; set; }
        public int ValidFromIntervalId { get; set; }
        public int ValidToDateId { get; set; }
        public int ValidToIntervalId { get; set; }
        public Guid PersonPeriodCode { get; set; }
        public string PersonName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmploymentNumber { get; set; }
        public int EmploymentTypeCode { get; set; }
        public string EmploymentTypeName { get; set; }
        public Guid ContractCode { get; set; }
        public string ContractName { get; set; }
        public Guid ParttimeCode { get; set; }
        public string ParttimePercentage { get; set; }
        public int TeamId { get; set; }
        public Guid TeamCode { get; set; }
        public string TeamName { get; set; }
        public int SiteId { get; set; }
        public Guid SiteCode { get; set; }
        public string SiteName { get; set; }
        public int BusinessUnitId { get; set; }
        public Guid BusinessUnitCode { get; set; }
        public string BusinessUnitName { get; set; }
        public int SkillsetId { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public DateTime EmploymentStartDate { get; set; }
        public DateTime EmploymentEndDate { get; set; }
        public int TimeZoneId { get; set; }
        public bool IsAgent { get; set; }
        public bool IsUser { get; set; }
        public int DatasourceId { get; set; }
        public DateTime InsertDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime DatasourceUpdateDate { get; set; }
        public bool ToBeDeleted { get; set; }
        public string WindowsDomain { get; set; }
        public string WindowsUsername { get; set; }
        public int ValidToDateIdMaxDate { get; set; }
        public int ValidToIntervalIdMaxDate { get; set; }
        public int ValidFromDateIdLocal { get; set; }
        public int ValidToDateIdLocal { get; set; }
        public DateTime ValidFromDateLocal { get; set; }
        public DateTime ValidToDateLocal { get; set; }
    }
}
