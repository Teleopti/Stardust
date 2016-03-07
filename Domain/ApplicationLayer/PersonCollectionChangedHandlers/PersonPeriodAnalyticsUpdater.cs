using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
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
			// Check if new person
			IAnalyticsPersonPeriod t = new AnalyticsPersonPeriod();

			foreach (var personCodeGuid in @event.PersonIdCollection.Distinct())
			{
				var person = _personRepository.FindPeople(new Guid[] { personCodeGuid }).FirstOrDefault();
				if (person == null)
				{
					return;
				}

				var personPeriodsInAnalytics = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid);

				PersonPeriodFilter filter = new PersonPeriodFilter(
					_analyticsPersonPeriodRepository.MinDate().DateDate,
					_analyticsPersonPeriodRepository.MaxDate().DateDate);

				PersonPeriodTransformer transformer = 
					new PersonPeriodTransformer(_personRepository, _analyticsPersonPeriodRepository);

				// Check if person period already exists in database
				foreach (var personPeriod in filter.GetFiltered(person.PersonPeriodCollection))
				{
					var existingPeriod = personPeriodsInAnalytics.FirstOrDefault(a => a.PersonPeriodCode.Equals(personPeriod.Id.Value));

					if (existingPeriod != null)
					{
						Console.WriteLine("Update person period for {0}", person.Name.ToString());

						// Update
						var updatedAnalyticsPersonPeriod = transformer.Transform(person, personPeriod);

						// Keep windows domain and username.
						updatedAnalyticsPersonPeriod.WindowsUsername = existingPeriod.WindowsUsername;
						updatedAnalyticsPersonPeriod.WindowsDomain = existingPeriod.WindowsDomain;

						_analyticsPersonPeriodRepository.UpdatePersonPeriod(updatedAnalyticsPersonPeriod);
					}
					else
					{
						Console.WriteLine("Insert new person period for {0}", person.Name.ToString());

						// Insert
						var newAnalyticsPersonPeriod = transformer.Transform(person, personPeriod);
						_analyticsPersonPeriodRepository.AddPersonPeriod(newAnalyticsPersonPeriod);
					}
					
				}
			}
		}

		
	}

	public class AnalyticsPersonPeriod : IAnalyticsPersonPeriod
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
		public int? EmploymentTypeCode { get; set; }
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
		public int? SkillsetId { get; set; }
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
