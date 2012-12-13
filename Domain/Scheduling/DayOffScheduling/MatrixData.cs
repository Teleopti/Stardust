using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixData
	{
		IScheduleDayData this[DateOnly key] { get; }
		IScheduleMatrixPro Matrix { get; }
		ReadOnlyCollection<IScheduleDayData> ScheduleDayDatas { get; }
	}

	public class MatrixData : IMatrixData
	{
		private readonly IScheduleMatrixPro _matrix;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly IDictionary<DateOnly, IScheduleDayData> _scheduleDayDatas;

		public MatrixData(IScheduleMatrixPro matrix, IEffectiveRestrictionCreator effectiveRestrictionCreator, ISchedulingOptions schedulingOptions)
		{
			_matrix = matrix;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulingOptions = schedulingOptions;
			initialize();
		}

		public IScheduleDayData this[DateOnly key]
		{
			get { return _scheduleDayDatas[key]; }
		}

		public IScheduleMatrixPro Matrix
		{
			get { return _matrix; }
		}

		public ReadOnlyCollection<IScheduleDayData> ScheduleDayDatas
		{
			get { return new ReadOnlyCollection<IScheduleDayData>(new List<IScheduleDayData>(_scheduleDayDatas.Values)); }
		}

		private void initialize()
		{
			foreach (var scheduleDayPro in _matrix.EffectivePeriodDays)
			{
				IScheduleDayData toAdd = new ScheduleDayData(scheduleDayPro.Day);
				IScheduleDay scheduleDay = scheduleDayPro.DaySchedulePart();
				toAdd.IsScheduled = scheduleDay.IsScheduled();
				toAdd.IsDayOff = scheduleDay.SignificantPart() == SchedulePartView.DayOff;
				toAdd.IsContractDayOff = scheduleDay.SignificantPart() == SchedulePartView.ContractDayOff;
				IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(scheduleDay,
				                                                                                                  _schedulingOptions);
				toAdd.HaveRestriction = effectiveRestriction.IsRestriction;
				_scheduleDayDatas.Add(toAdd.Date, toAdd);
			}
		}
	}
}