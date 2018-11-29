using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeAvailableCheckItem
	{
		public ShiftTradeAvailableCheckItem(DateOnly dateOnly, IPerson personFrom, IPerson personTo)
		{
			DateOnly = dateOnly;
			PersonFrom = personFrom;
			PersonTo = personTo;
		}

		public IPerson PersonFrom { get; private set; }
		public IPerson PersonTo { get; private set; }
		public DateOnly DateOnly { get; private set; }

		public override bool Equals(object obj)
		{
			return obj is ShiftTradeAvailableCheckItem other && (PersonFrom.Equals(other.PersonFrom) &&
																 PersonTo.Equals(other.PersonTo) &&
																 DateOnly.Equals(other.DateOnly));
		}

		public override int GetHashCode()
		{
			return PersonFrom.GetHashCode() ^
			       PersonTo.GetHashCode() ^
			       DateOnly.GetHashCode();
		}
	}
}