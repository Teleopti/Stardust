using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

        public SchedulePartExtractor(IScheduleDay schedulePart)
        {
            InParameter.NotNull(nameof(schedulePart), schedulePart);

            _schedulePart = schedulePart;

            IPerson person = schedulePart.Person;
            if (person == null)
                throw new ArgumentException("The Schedule Part must have a valid person.", nameof(schedulePart));

            _person = person;

            DateOnly startDate = _schedulePart.DateOnlyAsPeriod.DateOnly;

            _schedulePeriod = schedulePart.Person.VirtualSchedulePeriod(startDate); // this method takes care of the terminal date too
            if (!_schedulePeriod.IsValid)
                return;

            _actualSchedulePeriod = _schedulePeriod.DateOnlyPeriod;
        }

        public IVirtualSchedulePeriod SchedulePeriod => _schedulePeriod;

	    public DateOnlyPeriod ActualSchedulePeriod => _actualSchedulePeriod;

	    public IPerson Person => _person;

	    public IScheduleDay SchedulePart => _schedulePart;

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