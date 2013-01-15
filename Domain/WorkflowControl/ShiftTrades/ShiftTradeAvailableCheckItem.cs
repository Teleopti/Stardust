using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{
	public class ShiftTradeAvailableCheckItem
	{
		public IPerson PersonFrom { get; set; }
		public IPerson PersonTo { get; set; }
		public DateOnly DateOnly { get; set; }

		public override bool Equals(object obj)
		{
			var other = obj as ShiftTradeAvailableCheckItem;
			if (other == null)
				return false;

			return PersonFrom.Equals(other.PersonFrom) &&
				PersonTo.Equals(other.PersonTo) &&
				DateOnly.Equals(other.DateOnly);
		}

		public override int GetHashCode()
		{
			return PersonFrom.GetHashCode() ^
			       PersonTo.GetHashCode() ^
			       DateOnly.GetHashCode();
		}
	}
}