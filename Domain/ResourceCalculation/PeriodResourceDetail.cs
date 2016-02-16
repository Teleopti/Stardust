using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public struct PeriodResourceDetail
	{
		private double _resource;
		private int _count;

		public PeriodResourceDetail(int count, double resource) : this()
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

		public int Count
		{
			get { return _count; }
			set
			{
				InParameter.ValueMustBePositive("Count", value);
				_count = value;
			}
		}
	}


	public struct InnerPeriodResourceDetail
	{
		private double _resource;
		private int _count;
		private readonly SkillEffiencyResource[] _effiencyResources;
		private readonly DateTimePeriod[] _fractionPeriods;

		public InnerPeriodResourceDetail(int count, double resource, SkillEffiencyResource[] effiencyResources, DateTimePeriod[] fractionPeriods)
			: this()
		{
			Count = count;
			Resource = resource;
			_effiencyResources = effiencyResources;
			_fractionPeriods = fractionPeriods;
		}

		public double Resource
		{
			get { return _resource; }
			private set
			{
				var roundedValue = Math.Round(value, 5);
				InParameter.ValueMustBePositive("Resource", roundedValue);
				_resource = roundedValue;
			}
		}

		public int Count
		{
			get { return _count; }
			private set
			{
				InParameter.ValueMustBePositive("Count", value);
				_count = value;
			}
		}

		public DateTimePeriod[] FractionPeriods
		{
			get { return _fractionPeriods; }
		}

		public SkillEffiencyResource[] EffiencyResources
		{
			get { return _effiencyResources; }
		}
	}

	public struct SkillEffiencyResource
	{
		public SkillEffiencyResource(Guid skill, double resource) : this()
		{
			Resource = resource;
			Skill = skill;
		}

		public Guid Skill { get; private set; }
		public double Resource { get; private set; }
	}
}