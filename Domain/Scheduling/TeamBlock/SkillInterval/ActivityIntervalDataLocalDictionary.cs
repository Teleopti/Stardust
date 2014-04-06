using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface IActivityIntervalDataLocalDictionary
	{
		void Store(IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> skillIntervalDataDictionary);
		bool TryGetSkillIntervalData(IActivity activity, DateTime localDateTime, out ISkillIntervalData skillIntervalData);
		IEnumerable<IActivity> Keys { get; }
		TimeZoneInfo TimeZoneInfo { get; }
		IDictionary<DateTime, ISkillIntervalData> SkillIntervalDataDicFor(IActivity activity);
	}

	public class ActivityIntervalDataLocalDictionary : IActivityIntervalDataLocalDictionary
	{
		private IDictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>> _localDictionary;
		private TimeZoneInfo _timeZoneInfo;

		public void Store(IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> skillIntervalDataDictionary)
		{
			_localDictionary = new Dictionary<IActivity, IDictionary<DateTime, ISkillIntervalData>>();
			_timeZoneInfo = TimeZoneGuard.Instance.TimeZone;
			foreach (var activity in skillIntervalDataDictionary.Keys)
			{
				var localDataDicForActivity = new Dictionary<DateTime, ISkillIntervalData>();
				foreach (var skillIntervalData in skillIntervalDataDictionary[activity].Values)
				{
					var periodUtc = skillIntervalData.Period;
					var localStart = DateTime.SpecifyKind(periodUtc.StartDateTimeLocal(_timeZoneInfo), DateTimeKind.Utc);
					var localEnd = DateTime.SpecifyKind(periodUtc.EndDateTimeLocal(_timeZoneInfo), DateTimeKind.Utc);
					var localPeriod = new DateTimePeriod(localStart, localEnd);
					var intervalData = new SkillIntervalData(localPeriod, skillIntervalData.ForecastedDemand,
						skillIntervalData.CurrentDemand, skillIntervalData.CurrentHeads, skillIntervalData.MinimumHeads,
						skillIntervalData.MaximumHeads);
					localDataDicForActivity.Add(localStart, intervalData);
				}
				_localDictionary.Add(activity, localDataDicForActivity);
			}
		}

		public bool TryGetSkillIntervalData(IActivity activity, DateTime localDateTime, out ISkillIntervalData skillIntervalData)
		{
			IDictionary<DateTime, ISkillIntervalData> localDataDicForActivity;
			skillIntervalData = null;
			if (!_localDictionary.TryGetValue(activity, out localDataDicForActivity))
				return false;
			if (!localDataDicForActivity.TryGetValue(localDateTime, out skillIntervalData))
				return false;

			return true;
		}

		public IEnumerable<IActivity> Keys
		{
			get { return _localDictionary.Keys; }
		}

		public IDictionary<DateTime, ISkillIntervalData> SkillIntervalDataDicFor(IActivity activity)
		{
			return _localDictionary[activity];
		}

		public TimeZoneInfo TimeZoneInfo
		{
			get { return _timeZoneInfo; }
		}
	}
}