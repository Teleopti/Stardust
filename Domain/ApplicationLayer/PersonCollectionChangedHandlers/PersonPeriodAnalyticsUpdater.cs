using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162)]
	public class PersonPeriodAnalyticsUpdater :
		IHandleEvent<PersonCollectionChangedEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IRunOnServiceBus
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (PersonPeriodAnalyticsUpdater));
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsSkillRepository _analyticsSkillRepository;
		private readonly IPersonRepository _personRepository;

		public PersonPeriodAnalyticsUpdater(IPersonRepository personRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IAnalyticsSkillRepository analyticsSkillRepository)
		{
			_personRepository = personRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsSkillRepository = analyticsSkillRepository;
		}

		public void Handle(PersonCollectionChangedEvent @event)
		{
			var personPeriodFilter = new PersonPeriodFilter(
				_analyticsPersonPeriodRepository.MinDate().DateDate,
				_analyticsPersonPeriodRepository.MaxDate().DateDate);

			var persons = _personRepository.FindPeople(@event.PersonIdCollection.Distinct());

			var transformer = new PersonPeriodTransformer(_analyticsPersonPeriodRepository, _analyticsSkillRepository);

			foreach (var personCodeGuid in @event.PersonIdCollection.Distinct())
			{
				// Check if person does exists => if not it is deleted and handled by other handle-method
				if (!persons.Any(a => a.Id.Equals(personCodeGuid)))
				{
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
						Logger.DebugFormat("Update person period for {0}", person.Name);

						// Update
						var updatedAnalyticsPersonPeriod = transformer.Transform(person, personPeriod);

						// Keep windows domain and username until external login information is availble from service bus
						updatedAnalyticsPersonPeriod.WindowsUsername = existingPeriod.WindowsUsername;
						updatedAnalyticsPersonPeriod.WindowsDomain = existingPeriod.WindowsDomain;

						_analyticsPersonPeriodRepository.UpdatePersonPeriod(updatedAnalyticsPersonPeriod);
					}
					else
					{
						Logger.DebugFormat("Insert new person period for {0}", person.Name);

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

		public void Handle(PersonDeletedEvent @event)
		{
			Logger.DebugFormat("Removing all person periods with person code {0}", @event.PersonId);
			var personPeriodsInAnalyticsToBeDeleted = _analyticsPersonPeriodRepository.GetPersonPeriods(@event.PersonId);
			foreach (var analyticsPersonPeriodToBeDeleted in personPeriodsInAnalyticsToBeDeleted)
			{
				_analyticsPersonPeriodRepository.DeletePersonPeriod(analyticsPersonPeriodToBeDeleted);
			}
		}
	}
}