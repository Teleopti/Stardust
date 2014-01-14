using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Extracts the contained person in the schedule part or in the general schedule period contained in the
    /// schedule part and calculates the actual schedule period
    /// </summary>
    public class SchedulePartExtractor : ISchedulePartExtractor
    {
        private readonly IVirtualSchedulePeriod _schedulePeriod;
        private readonly DateOnlyPeriod _actualSchedulePeriod;
        private readonly IPerson _person;
        private readonly IScheduleDay _schedulePart;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedSchedulePeriodData"/> class.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        public SchedulePartExtractor(IScheduleDay schedulePart)
        {
            InParameter.NotNull("schedulePart", schedulePart);

            _schedulePart = schedulePart;

            IPerson person = schedulePart.Person;
            if (person == null)
                throw new ArgumentException("The Schedule Part must have a valid person.", "schedulePart");

            _person = person;

            DateTimePeriod validPeriod = schedulePart.Period;
            DateTime scheduleDayUtc = validPeriod.StartDateTime;

            TimeZoneInfo timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
            DateOnly startDate = new DateOnly(TimeZoneHelper.ConvertFromUtc(scheduleDayUtc, timeZoneInfo));

            _schedulePeriod = schedulePart.Person.VirtualSchedulePeriod(startDate); // this method takes care of the terminal date too
            if (!_schedulePeriod.IsValid)
                return;

            _actualSchedulePeriod = _schedulePeriod.DateOnlyPeriod;
            //DateOnlyPeriod? scheduleDatePeriod = _schedulePeriod.DateOnlyPeriod; // this method also takes care of the terminal date

            //_actualSchedulePeriod = scheduleDatePeriod.Value; // later that is used as effective period
            
        }

        public IVirtualSchedulePeriod SchedulePeriod
        {
            get { return _schedulePeriod; }
        }

        public DateOnlyPeriod ActualSchedulePeriod
        {
            get { return _actualSchedulePeriod;  }
        }

        public IPerson Person
        {
            get { return _person; }
        }

        public IScheduleDay SchedulePart
        {
            get { return _schedulePart; }
        }

        public override int GetHashCode()
        {
            return Person.GetHashCode() ^ SchedulePeriod.GetHashCode() ^ ActualSchedulePeriod.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var casted = obj as ISchedulePartExtractor;
            if(casted==null)
                return false;
            return casted.Person.Equals(Person) && 
                   casted.SchedulePeriod.Equals(SchedulePeriod) &&
                   casted.ActualSchedulePeriod.Equals(ActualSchedulePeriod);
        }
    }
}