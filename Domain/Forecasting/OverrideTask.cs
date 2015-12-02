using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class OverrideTask : IOverrideTask
	{
		private readonly double? _overrideTasks;
		private readonly TimeSpan? _overrideAverageTaskTime;
		private readonly TimeSpan? _overrideAverageAfterTaskTime;

		public OverrideTask(double? tasks, TimeSpan? averageTaskTime, TimeSpan? averageAfterTaskTime)
		{
			_overrideTasks = tasks;
			_overrideAverageTaskTime = averageTaskTime;
			_overrideAverageAfterTaskTime = averageAfterTaskTime;
		}

		public OverrideTask()
		{
		}

		public bool Equals(IOverrideTask other)
		{
			if (other == null)
				return false;

			return other.OverrideAverageTaskTime == _overrideAverageTaskTime &&
					 other.OverrideAverageAfterTaskTime == _overrideAverageAfterTaskTime &&
					 other.OverrideTasks == _overrideTasks;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is OverrideTask))
				return false;
			return Equals((OverrideTask)obj);
		}

		public override int GetHashCode()
		{
			return (GetType().FullName + "|" +
				 _overrideAverageAfterTaskTime + "|" +
				 _overrideAverageTaskTime + "|" +
				 _overrideTasks).GetHashCode();
		}

		public double? OverrideTasks
		{
			get { return _overrideTasks; }
		}

		public TimeSpan? OverrideAverageTaskTime
		{
			get { return _overrideAverageTaskTime; }
		}

		public TimeSpan? OverrideAverageAfterTaskTime
		{
			get { return _overrideAverageAfterTaskTime; }
		}
	}
}