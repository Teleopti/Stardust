using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.Views;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers
{
    public static class PeriodDateDictionaryBuilder
    {

        /// <summary>
        /// Gets the date only dictionary.
        /// </summary>
        /// <param name="stateHolder">The state holder.</param>
        /// <param name="viewType">Type of the view.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2/10/2009
        /// </remarks>
        public static ICollection<DateOnly> GetDateOnlyDictionary(FilteredPeopleHolder stateHolder, 
            ViewType viewType, IPerson person)
        {
            ICollection<DateOnly> dictionary = new HashSet<DateOnly>();

            switch (viewType)
            {
                case ViewType.PeoplePeriodView:
                    {
                        foreach (IPersonPeriod personPeriod in person.PersonPeriodCollection)
                        {
                            dictionary.Add(personPeriod.StartDate);
                        }

                        break;
                    }
                case ViewType.SchedulePeriodView:
                    {
                        foreach (ISchedulePeriod schedulePeriod in person.PersonSchedulePeriodCollection)
                        {
                            dictionary.Add(schedulePeriod.DateFrom);
                        }

                        break;
                    }
                case ViewType.PersonalAccountGridView:
                    {
                        foreach (IAccount personAccount in stateHolder.AllAccounts[person].AllPersonAccounts())
                        {
                            dictionary.Add(personAccount.StartDate);
                        }

                        break;
                    }
                case ViewType.PersonRotationView:
                    {
                        foreach (IPersonRotation rotation in GetPersonRotationCollection(person, stateHolder))
                        {
                            dictionary.Add(rotation.StartDate);
                        }
                        break;
                    }
                case ViewType.PersonAvailabilityView:
                    {
                        foreach (IPersonAvailability availability in GetPersonAvailabilityCollection(person, stateHolder))
                        {
                            dictionary.Add(availability.StartDate);
                        }
                        break;

                    }
            }

            return dictionary;
        }


        /// <summary>
        /// Gets the person rotation collection.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="stateHolder">The state holder.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2/10/2009
        /// </remarks>
        private static IList<IPersonRotation> GetPersonRotationCollection(IPerson person, FilteredPeopleHolder stateHolder)
        {
            if (stateHolder == null) return null;

            IList<IPersonRotation> personRotationCollection = new List<IPersonRotation>();

            if (person != null)
            {
                personRotationCollection = stateHolder.AllPersonRotationCollection.Where
                    (a => a.Person.Id == person.Id).ToList();
            }

            return personRotationCollection;
        }


        /// <summary>
        /// Gets the person availability collection.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="stateHolder">The state holder.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2/10/2009
        /// </remarks>
        private static IList<IPersonAvailability> GetPersonAvailabilityCollection(IPerson person, FilteredPeopleHolder stateHolder)
        {
            if (stateHolder == null) return null;

            IList<IPersonAvailability> personAvailabilityCollection = new List<IPersonAvailability>();

            if (person != null)
            {
                personAvailabilityCollection = stateHolder.AllPersonAvailabilityCollection.Where
                    (a => a.Person.Id == person.Id).ToList();
            }

            return personAvailabilityCollection;
        }



    }



}
