using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IMatrixData
	{
		IScheduleDayData this[DateOnly key] { get; }
		IScheduleMatrixPro Matrix { get; }
		ReadOnlyCollection<IScheduleDayData> ScheduleDayDataCollection { get; }
		void Store(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions);
	    bool ContainsKey(DateOnly key);
	    bool TryGetValue(DateOnly key, out IScheduleDayData value);
		int TargetDaysOff { get; }
	}

	public class MatrixData : IMatrixData
	{
		private IScheduleMatrixPro _matrix;
		private readonly IScheduleDayDataMapper _scheduleDayDataMapper;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		protected IDictionary<DateOnly, IScheduleDayData> ScheduleDayDataDictionary = new Dictionary<DateOnly, IScheduleDayData>();

		public MatrixData(IScheduleDayDataMapper scheduleDayDataMapper)
		{
			_scheduleDayDataMapper = scheduleDayDataMapper;
		}

		public IScheduleDayData this[DateOnly key]
		{
			get { return ScheduleDayDataDictionary[key]; }
		}

        public bool ContainsKey(DateOnly key)
        {
            return ScheduleDayDataDictionary.ContainsKey(key);
        }

		public bool TryGetValue(DateOnly key, out IScheduleDayData value)
		{
			return ScheduleDayDataDictionary.TryGetValue(key, out value);
		}

		public int TargetDaysOff { get; protected set; }

		public IScheduleMatrixPro Matrix
		{
			get { return _matrix; }
		}

		public ReadOnlyCollection<IScheduleDayData> ScheduleDayDataCollection
		{
			get { return new ReadOnlyCollection<IScheduleDayData>(new List<IScheduleDayData>(ScheduleDayDataDictionary.Values)); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public virtual void Store(IScheduleMatrixPro matrix, ISchedulingOptions schedulingOptions)
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