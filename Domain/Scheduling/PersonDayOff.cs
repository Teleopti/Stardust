using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;
using System;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Class describing an PersonDayOff
    /// </summary>
    public class PersonDayOff : AggregateRootWithBusinessUnit, 
                                IPersonDayOff,
                                IExportToAnotherScenario
    {
        private IPerson _person;
        private IScenario _scenario;
        private readonly DayOff _dayOff;
        private TimeZoneInfo _usedTimeZone;

        /// <summary>
        /// Creates a new instance of PersonAbsence
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="dayOff">The day off.</param>
        /// <param name="date">The date.</param>
        public PersonDayOff(IPerson person, IScenario scenario, IDayOffTemplate dayOff, DateOnly date)
            : this(person, scenario, dayOff, date, person.PermissionInformation.DefaultTimeZone())
        {
        }

        public PersonDayOff(IPerson person, IScenario scenario, IDayOffTemplate dayOff, DateOnly date, TimeZoneInfo toTimeZone)
        {

            _person = person;
            _scenario = scenario;
            _usedTimeZone = toTimeZone;
            DateTime anchorDateTime = TimeZoneHelper.ConvertToUtc(date.Date.Add(dayOff.Anchor), toTimeZone);
            _dayOff = new DayOff(anchorDateTime, dayOff.TargetLength, dayOff.Flexibility, dayOff.Description, dayOff.DisplayColor, dayOff.PayrollCode);
        }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="scenario">The scenario.</param>
        /// <param name="dayOff">The day off.</param>
        /// <param name="date">The date.</param>
        /// <param name="fromTimeZone">From time zone.</param>

        public PersonDayOff(IPerson person, IScenario scenario, IDayOff dayOff, DateOnly date, TimeZoneInfo fromTimeZone)
        {

            _person = person;
            _scenario = scenario;
            _usedTimeZone = person.PermissionInformation.DefaultTimeZone();
            DateTime anchorDateTime = TimeZoneHelper.ConvertToUtc(date.Date.Add(dayOff.AnchorLocal(fromTimeZone).TimeOfDay),
                                                                  _usedTimeZone);

            _dayOff = new DayOff(anchorDateTime, dayOff.TargetLength, dayOff.Flexibility, dayOff.Description, dayOff.DisplayColor, dayOff.PayrollCode);
        }


        /// <summary>
        /// Constructor for NHibernate
        /// </summary>
        protected PersonDayOff()
        {
        }

        #region Properties

        /// <summary>
        /// Gets the Person
        /// </summary>
        public virtual IPerson Person
        {
            get { return _person; }
        }

        /// <summary>
        /// Gets the Scenario
        /// </summary>
        public virtual IScenario Scenario
        {
            get { return _scenario; }
        }

        /// <summary>
        /// Gets the Period.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-02-06
        /// </remarks>
        public virtual DateTimePeriod Period
        {
            get
            {
                return new DateTimePeriod(_dayOff.Anchor, _dayOff.Anchor.AddTicks(1));
            }
        }

        /// <summary>
        /// Information about the Day Off
        /// </summary>
        public virtual IDayOff DayOff
        {
            get { return _dayOff; }
        }

        #endregion


        public virtual bool BelongsToScenario(IScenario scenario)
        {
            return Scenario.Equals(scenario);
        }

        public virtual string FunctionPath
        {
            get { return DefinedRaptorApplicationFunctionPaths.ModifyPersonDayOff; }
        }

        public virtual IPersistableScheduleData CreateTransient()
        {
            return NoneEntityClone();
        }

        public virtual IPersistableScheduleData CloneAndChangeParameters(IScheduleParameters parameters)
        {
            var retObj = (PersonDayOff)NoneEntityClone();
            retObj._scenario = parameters.Scenario;
            retObj._person = parameters.Person;
            return retObj;
        }


        /// <summary>
        /// Compares a PersonDayOff to a DayOffTemplate.
        /// </summary>
        /// <param name="dayOffTemplate">The day off template.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2009-02-06
        /// </remarks>
        public virtual bool CompareToTemplate(IDayOffTemplate dayOffTemplate)
        {
            bool returnValue = false;
            DateTime localDayOffAnchor = TimeZoneHelper.ConvertFromUtc(DayOff.Anchor, _person.PermissionInformation.DefaultTimeZone());
            TimeSpan localDayOffAnchorTimeSpan = localDayOffAnchor.TimeOfDay;
            bool hasSameAncor = dayOffTemplate.Anchor.Equals(localDayOffAnchorTimeSpan);
           
            //Compare flexibility, targetlength and ancor
            if (dayOffTemplate.Flexibility.Equals(DayOff.Flexibility) &&
                dayOffTemplate.TargetLength.Equals(DayOff.TargetLength) &&
                hasSameAncor &&
				string.Compare(dayOffTemplate.PayrollCode, DayOff.PayrollCode, false, CultureInfo.CurrentCulture) == 0)
            {
                returnValue = true;
            }
            return returnValue;
        }
        public virtual bool CompareToTemplateForLocking(IDayOffTemplate dayOffTemplate)
        {
            //Only compare Description
            if (dayOffTemplate.Description.Name == DayOff.Description.Name && dayOffTemplate.Description.ShortName == DayOff.Description.ShortName)
                return true;
            return false;
        }

        #region ICloneableEntity<PersonDayOff> Members


        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2007-11-09
        /// </remarks>
        public virtual object Clone()
        {
            return EntityClone();
        }

        public virtual bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
        {
            return dateAndPeriod.Period().Contains(DayOff.Anchor);
        }

        public virtual bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            DateTimePeriod dateTimePeriod =
               dateOnlyPeriod.ToDateTimePeriod(Person.PermissionInformation.DefaultTimeZone());

            return dateTimePeriod.Contains(DayOff.Anchor);
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public virtual IPersonDayOff NoneEntityClone()
        {
            PersonDayOff retObj = (PersonDayOff)MemberwiseClone();
            retObj.SetId(null);
            return retObj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public virtual IPersonDayOff EntityClone()
        {
            PersonDayOff retObj = (PersonDayOff)MemberwiseClone();
            return retObj;
        }

        #endregion

        public virtual IAggregateRoot MainRoot
        {
            get { return Person; }
        }

        public virtual long Checksum()
        {
            return Crc32.Compute(SerializationHelper.SerializeAsBinary(DayOff.Anchor)) ^
                Crc32.Compute(SerializationHelper.SerializeAsBinary(DayOff.Flexibility)) ^
                Crc32.Compute(SerializationHelper.SerializeAsBinary(DayOff.TargetLength)) ^ 
                Crc32.Compute(SerializationHelper.SerializeAsBinary(Period.EndDateTime.Ticks)) ^ 
                Crc32.Compute(SerializationHelper.SerializeAsBinary(Period.StartDateTime.Ticks));
        }

        public virtual TimeZoneInfo UsedTimeZone
        {
            get { return _usedTimeZone; }
        }
    }
}
