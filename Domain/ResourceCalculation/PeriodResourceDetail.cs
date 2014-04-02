using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public struct PeriodResourceDetail
	{
		private double _resource;
		private double _count;

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
				var roundedValue = Math.Round(value, 5);
				InParameter.ValueMustBePositive("Resource", roundedValue);
				_resource = roundedValue;
			}
		}

		public double Count
		{
			get { return _count; }
			set
			{
				var roundedValue = Math.Round(value, 5);
				InParameter.ValueMustBePositive("Resource", roundedValue);
				_count = roundedValue;
			}
		}
	}
}