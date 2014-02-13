using System;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public struct PeriodResourceDetail
	{
		private double _resource;

		public PeriodResourceDetail(double count, double resource) : this()
		{
			Count = count;
			Resource = resource;
		}

		public double Resource
		{
			get { return _resource; }
			set
			{
				_resource = Math.Round(value, 5);
			}
		}

		public double Count { get; set; }
	}
}