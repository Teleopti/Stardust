using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixData
	{
		IScheduleMatrixPro Matrix { get; }
		ReadOnlyCollection<IScheduleDayData> ScheduleDayDataCollection { get; }
		void Store(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions);
	    bool TryGetValue(DateOnly key, out IScheduleDayData value);
		int TargetDaysOff { get; }
	}

	public class MatrixData : IMatrixData
	{
		private IScheduleMatrixPro _matrix;
		private readonly IScheduleDayDataMapper _scheduleDayDataMapper;
		protected IDictionary<DateOnly, IScheduleDayData> ScheduleDayDataDictionary = new Dictionary<DateOnly, IScheduleDayData>();

		public MatrixData(IScheduleDayDataMapper scheduleDayDataMapper)
		{
			_scheduleDayDataMapper = scheduleDayDataMapper;
		}

		public bool TryGetValue(DateOnly key, out IScheduleDayData value)
		{
			return ScheduleDayDataDictionary.TryGetValue(key, out value);
		}

		public int TargetDaysOff { get; protected set; }

		public IScheduleMatrixPro Matrix => _matrix;

		public ReadOnlyCollection<IScheduleDayData> ScheduleDayDataCollection => new ReadOnlyCollection<IScheduleDayData>(new List<IScheduleDayData>(ScheduleDayDataDictionary.Values));

		public virtual void Store(IScheduleMatrixPro matrix, SchedulingOptions schedulingOptions)
		{
			_matrix = matrix;
			TargetDaysOff = _matrix.SchedulePeriod.DaysOff();
			foreach (var scheduleDayPro in _matrix.FullWeeksPeriodDays)
			{
				IScheduleDayData toAdd = _scheduleDayDataMapper.Map(scheduleDayPro, schedulingOptions);
				ScheduleDayDataDictionary.Add(toAdd.DateOnly, toAdd);
			}
		}
	}
}