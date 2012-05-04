using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	/// <summary>
	/// Validates that the activity has specified length
	/// </summary>
	public class ActivityTimeLimiter : WorkShiftLimiter
	{
		private IActivity _activity;
		private TimeSpan _timeLimit;
		private OperatorLimiter _timeLimitOperator;

		protected ActivityTimeLimiter() { }

		public ActivityTimeLimiter(IActivity activity, TimeSpan timeLimit, OperatorLimiter timeLimitOperator)
		{
			_activity = activity;
			_timeLimit = timeLimit;
			_timeLimitOperator = timeLimitOperator;
		}

		public virtual TimeSpan TimeLimit
		{
			get { return _timeLimit; }
			set { _timeLimit = value; }
		}

		public virtual OperatorLimiter TimeLimitOperator
		{
			get { return _timeLimitOperator; }
			set { _timeLimitOperator = value; }
		}

		public virtual IActivity Activity
		{
			get { return _activity; }
			set { _activity = value; }
		}

		public override bool IsValidAtStart(IWorkShift shift, IList<IWorkShiftExtender> extenders)
		{
			var totalExtenderTime = TimeSpan.Zero;
			var containsActivity = false;

			foreach (var extender in extenders.Where(extender => extender.ExtendWithActivity.Equals(_activity)))
			{
				containsActivity = true;
				totalExtenderTime += extender.ExtendMaximum();
			}

			if (containsActivity)
			{
				if (_timeLimitOperator == OperatorLimiter.GreaterThen)
				{
					if (totalExtenderTime <= _timeLimit)
						return false;
				}

				if (_timeLimitOperator == OperatorLimiter.GreaterThenEquals)
				{
					if (totalExtenderTime < _timeLimit)
						return false;
				}
			}

			return true;
		}


		public override bool IsValidAtEnd(IVisualLayerCollection endProjection)
		{
			var layers = endProjection.FilterLayers(_activity);

			if (layers.Any())
			{
				foreach (var layerTime in layers.Select(layer => layer.Period.ElapsedTime()))
				{
					bool ok;
					switch (TimeLimitOperator)
					{
						case OperatorLimiter.Equals:
							ok = layerTime == _timeLimit;
							break;
						case OperatorLimiter.LessThen:
							ok = layerTime < _timeLimit;
							break;
						case OperatorLimiter.GreaterThen:
							ok = layerTime > _timeLimit;
							break;
						case OperatorLimiter.LessThenEquals:
							ok = layerTime <= _timeLimit;
							break;
						case OperatorLimiter.GreaterThenEquals:
							ok = layerTime >= _timeLimit;
							break;
						default:
							throw new NotImplementedException("Unknown operator limiter: " + TimeLimitOperator);
					}

					if (!ok)
						return false;
				}

				return true;
			}

			return _timeLimitOperator == OperatorLimiter.LessThenEquals || _timeLimitOperator == OperatorLimiter.LessThen;
		}

		public override IWorkShiftLimiter NoneEntityClone()
		{
			var retobj = (IWorkShiftLimiter)MemberwiseClone();
			retobj.SetId(null);
			retobj.SetParent(null);
			return retobj;
		}


		public override IWorkShiftLimiter EntityClone()
		{
			return (IWorkShiftLimiter)MemberwiseClone();
		}

		public override object Clone()
		{
			return EntityClone();
		}
	}
}
