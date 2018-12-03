using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			get => _resource;
			set
			{
				InParameter.ValueMustBePositive(nameof(Resource), value);
				_resource = value;
			}
		}

		public double Count
		{
			get => _count;
			set
			{
				InParameter.ValueMustBePositive(nameof(Resource), value);
				_count = value;
			}
		}
	}


	public struct InnerPeriodResourceDetail
	{
		private double _resource;
		private double _count;

		public InnerPeriodResourceDetail(double count, double resource, SkillEffiencyResource[] effiencyResources, DateTimePeriod[] fractionPeriods)
			: this()
		{
			Count = count;
			Resource = resource;
			EffiencyResources = effiencyResources;
			FractionPeriods = fractionPeriods;
		}

		public double Resource
		{
			get => _resource;
			private set
			{
				InParameter.ValueMustBePositive(nameof(Resource), value);
				_resource = value;
			}
		}

		public double Count
		{
			get => _count;
			private set
			{
				InParameter.ValueMustBePositive(nameof(Count), value);
				_count = value;
			}
		}

		public DateTimePeriod[] FractionPeriods { get; }

		public SkillEffiencyResource[] EffiencyResources { get; }
	}

	public struct SkillEffiencyResource
	{
		public SkillEffiencyResource(Guid skill, double resource) : this()
		{
			Resource = resource;
			Skill = skill;
		}

		public Guid Skill { get; }
		public double Resource { get; }
	}
}