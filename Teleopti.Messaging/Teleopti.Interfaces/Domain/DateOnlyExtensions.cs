namespace Teleopti.Interfaces.Domain
{
	public static class DateOnlyExtensions
	{
		public static DateOnly ValidDateOnly(this DateOnly value)
		{
			var minDateOnly = new DateOnly(DateHelper.MinSmallDateTime);
			var maxDateOnly = new DateOnly(DateHelper.MaxSmallDateTime);

			var toReturn = value;
			if (toReturn < minDateOnly)
			{
				toReturn = minDateOnly;
			}
			if (toReturn > maxDateOnly)
			{
				toReturn = maxDateOnly;
			}
			return toReturn;
		}
	}
}