using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class PersonWeek
	{
		private readonly IPerson _person;
		private readonly DateOnlyPeriod _week;

		public PersonWeek(IPerson person, DateOnlyPeriod week)
		{
			_person = person;
			_week = week;
		}

		public IPerson Person => _person;

		public DateOnlyPeriod Week => _week;

		public override int GetHashCode()
		{
			return _person.GetHashCode() ^ _week.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is PersonWeek other && (((_person == null && other._person == null) ||
												(_person != null && _person.Equals(other._person))) &&
											   _week == other._week);
		}
	}
}