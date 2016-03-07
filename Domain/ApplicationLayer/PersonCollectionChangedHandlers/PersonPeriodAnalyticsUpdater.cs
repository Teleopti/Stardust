using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
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
			var personPeriodFilter = new PersonPeriodFilter(
						_analyticsPersonPeriodRepository.MinDate().DateDate,
						_analyticsPersonPeriodRepository.MaxDate().DateDate);

			var persons = _personRepository.FindPeople(@event.PersonIdCollection.Distinct());

			var transformer = new PersonPeriodTransformer(_analyticsPersonPeriodRepository);

			foreach (var personCodeGuid in @event.PersonIdCollection.Distinct())
			{
				// Check if deleted person => all person periods in analytics should be deleted
				if (!persons.Any(a => a.Id.Equals(personCodeGuid)))
				{
					var personPeriodsInAnalyticsToBeDeleted = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid);
					foreach (var analyticsPersonPeriodToBeDeleted in personPeriodsInAnalyticsToBeDeleted)
					{
						_analyticsPersonPeriodRepository.DeletePersonPeriod(analyticsPersonPeriodToBeDeleted);
					}
					continue;
				}

				var person = persons.First(a => a.Id.Equals(personCodeGuid));
				if (person == null)
				{
					continue;
				}

				var personPeriodsInAnalytics = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid);

				foreach (var personPeriod in personPeriodFilter.GetFiltered(person.PersonPeriodCollection))
				{
					// Check if person period already exists in database
					var existingPeriod = personPeriodsInAnalytics.FirstOrDefault(a => a.PersonPeriodCode.Equals(personPeriod.Id.Value));

					if (existingPeriod != null)
					{
						Console.WriteLine("Update person period for {0}", person.Name.ToString());

						// Update
						var updatedAnalyticsPersonPeriod = transformer.Transform(person, personPeriod);

						// Keep windows domain and username until external login information is availble from service bus
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

				// Check deleted person periods
				foreach (var analyticsPersonPeriod in personPeriodsInAnalytics)
				{
					if (!person.PersonPeriodCollection.Any(a => a.Id.Equals(analyticsPersonPeriod.PersonPeriodCode)))
					{
						_analyticsPersonPeriodRepository.DeletePersonPeriod(analyticsPersonPeriod);
					}
				}
			}
		}
	}
}
