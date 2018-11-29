using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	/// <summary>
	/// Payload layer class containing Activity
	/// </summary>
	public class ActivityLayer : Layer<IActivity>, IActivityRestrictableVisualLayer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ActivityLayer"/> class.
		/// </summary>
		/// <param name="activity">The activity.</param>
		/// <param name="period">The period.</param>
		public ActivityLayer(IActivity activity, DateTimePeriod period)
			: base(activity, period)
		{
			InParameter.EnsureNoSecondsInPeriod(period);
		}

		public override DateTimePeriod Period
		{
			get { return base.Period; }
			set
			{
				InParameter.EnsureNoSecondsInPeriod(value);
				base.Period = value;
			}
		}

		public virtual Guid ActivityId => Payload.Id.Value;
	}
}