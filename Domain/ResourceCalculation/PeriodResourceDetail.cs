namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public struct PeriodResourceDetail
	{
		public PeriodResourceDetail(double count, double resource) : this()
		{
			Count = count;
			Resource = resource;
		}

		public double Resource { get; set; }

		public double Count { get; set; }
	}
}