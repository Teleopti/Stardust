using System;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.FakeData
{
    /// <summary>
    /// This class helps specific domain objects creations for test methods in people Admin.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-09-24
    /// </remarks>
    public static class PeopleAdminUtilityTestHelper
    {
        /// <summary>
        /// Creates the person rotation.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="name">The name.</param>
        /// <param name="rotation">The rotaion.</param>
        /// <param name="person">The person.</param>
        /// <param name="startWeek">The start week.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-02
        /// </remarks>
        public static IPersonRotation CreatePersonRotation(DateOnly dateTime, Name name, 
            IRotation rotation, IPerson person, int startWeek)
        {
	        person.WithName(name);
            return new PersonRotation(person, rotation, dateTime, startWeek);
        }

        /// <summary>
        /// Creates the person account day grid view adoptor.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="person">The person.</param>
        /// <param name="balanceIn">The balance in.</param>
        /// <param name="accrued">The accrued.</param>
        /// <param name="extra">The extra.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        public static IPersonAccountModel CreatePersonAccountDayGridViewAdapter(DateOnly date, IPerson person, int balanceIn, int accrued, int extra)
        {
            IAccount account = new AccountDay(date);
            account.BalanceIn = TimeSpan.FromDays(balanceIn);
            account.Accrued = TimeSpan.FromDays(accrued);
            account.Extra= TimeSpan.FromDays(extra);
            var gris = new PersonAccountCollection(person);
            var varulv = new PersonAbsenceAccount(person, new Absence());
            gris.Add(varulv);
            return new PersonAccountModel(null, gris, account, null);
        }

        /// <summary>
        /// Creates the person account time grid view adoptor.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="person">The person.</param>
        /// <param name="balanceIn">The balance in.</param>
        /// <param name="accrued">The accrued.</param>
        /// <param name="extra">The extra.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-08
        /// </remarks>
        public static IPersonAccountModel CreatePersonAccountTimeGridViewAdapter(DateOnly date, IPerson person, TimeSpan balanceIn, TimeSpan accrued, TimeSpan extra)
        {
            IAccount account = new AccountTime(date);
            account.BalanceIn = balanceIn;
            account.Accrued = accrued;
            account.Extra = extra;
            var gris = new PersonAccountCollection(person);
            var varulv = new PersonAbsenceAccount(person, new Absence());
            gris.Add(varulv);
            return new PersonAccountModel(null, gris, account, null);
        }

        /// <summary>
        /// Creates the person availability parent adopter.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="dayCount">The day count.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-15
        /// </remarks>
        public static PersonAvailabilityModelParent CreatePersonAvailabilityParentAdapter(DateOnly dateTime, Name name, 
            Description description, int dayCount)
        {
            // Instantiates the person
            IPerson person = PersonFactory.CreatePerson();
            person.SetName(name);

            // Instantiates the availability
            AvailabilityRotation availability = new AvailabilityRotation(description.Name, dayCount);
            IPersonAvailability personAvailability = new PersonAvailability(person, availability, dateTime);

            // Instantiates the adopter
            return new PersonAvailabilityModelParent(person, personAvailability,null);
        }
    }
}
