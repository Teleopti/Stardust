using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.Scheduling.RestrictionSummary;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.AgentRestrictions
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IAgentRestrictionsDetailModel
	{
		void LoadDetails(IScheduleMatrixPro scheduleMatrixPro, IRestrictionExtractor restrictionExtractor, RestrictionSchedulingOptions schedulingOptions,  IAgentRestrictionsDetailEffectiveRestrictionExtractor effectiveRestrictionExtractor, TimeSpan periodTarget, IPreferenceNightRestChecker preferenceNightRestChecker);
		Dictionary<int, IPreferenceCellData> DetailData();
	}

	public class AgentRestrictionsDetailModel : IAgentRestrictionsDetailModel
	{
		private Dictionary<int, IPreferenceCellData> _detailData;
		private readonly DateTimePeriod _loadedPeriod;
		private readonly object _lock = new object();

		public AgentRestrictionsDetailModel(DateTimePeriod loadedPeriod)
		{
			_detailData = new Dictionary<int, IPreferenceCellData>();
			_loadedPeriod = loadedPeriod;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "5"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void LoadDetails(IScheduleMatrixPro scheduleMatrixPro, IRestrictionExtractor restrictionExtractor, RestrictionSchedulingOptions schedulingOptions, IAgentRestrictionsDetailEffectiveRestrictionExtractor effectiveRestrictionExtractor, TimeSpan periodTarget, IPreferenceNightRestChecker preferenceNightRestChecker)
		{
			lock (_lock)
			{
				_detailData = new Dictionary<int, IPreferenceCellData>();
				//var dates = DetailDates(scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.StartDate.Date,
				//                        scheduleMatrixPro.SchedulePeriod.DateOnlyPeriod.EndDate.Date);

				var counter = 0;

				foreach (var dateOnly in scheduleMatrixPro.OuterWeeksPeriodDictionary.Keys)
				{
					var data = new PreferenceCellData();
					effectiveRestrictionExtractor.Extract(scheduleMatrixPro, data, dateOnly, _loadedPeriod, periodTarget);
					_detailData.Add(counter, data);

					counter++;
				}

				preferenceNightRestChecker.CheckNightlyRest(_detailData);
			}
		}

		public Dictionary<int, IPreferenceCellData> DetailData()
		{
			return _detailData;
		}
	}
}
