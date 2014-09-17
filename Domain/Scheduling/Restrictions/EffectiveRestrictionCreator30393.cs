using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class EffectiveRestrictionCreator30393 : EffectiveRestrictionCreator
	{
		public EffectiveRestrictionCreator30393(IRestrictionExtractor extractor) : base(extractor)
		{
		}

		public override IEffectiveRestriction GetEffectiveRestriction(IScheduleDay part, ISchedulingOptions options)
		{
			var ret = base.GetEffectiveRestriction(part, options);

			if (ret == null)
				return null;

			if (options.UseStudentAvailability && ret.NotAvailable)
				ret.DayOffTemplate = options.DayOffTemplate;

			return ret;
		}
	}
}