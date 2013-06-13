using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	/// <summary>
	/// Payload layer class containing Activity
	/// </summary>
	public class ActivityLayer : Layer<IActivity>, IActivityLayer, IActivityRestrictableVisualLayer
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

		/// <summary>
		/// Used by nhibernate to reconstitute from datasource
		/// </summary>
		protected ActivityLayer()
		{
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

		public virtual IMultiplicatorDefinitionSet DefinitionSet
		{
			get { return null; }
		}

		public virtual Guid ActivityId
		{
			get { return Payload.Id.Value; }
		}
	}
}