using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IScheduleRestrictionExtractor
	{
        IEffectiveRestriction Extract(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions, TimeZoneInfo timeZone, ITeamBlockInfo teamBlockInfo);

		IEffectiveRestriction ExtractForOnePersonOneBlock(IList<DateOnly> dateOnlyList,
														  IList<IScheduleMatrixPro> matrixList,
		                                                  ISchedulingOptions schedulingOptions, TimeZoneInfo timeZone);

		IEffectiveRestriction ExtractForOneTeamOneDay(DateOnly dateOnly, IList<IScheduleMatrixPro> matrixList,
		                                              ISchedulingOptions schedulingOptions, TimeZoneInfo timeZone);
	}

    public class InternalScheduleDay
    {
        public Lazy<SchedulePartView> SignificantPart { get; set; }
        public Lazy<IVisualLayerCollection> Projection { get; set; }
        public Lazy<IPersonAssignment> Assignment { get; set; }
        public Lazy<IEditableShift> EditorShift { get; set; }
    }

    public class ScheduleRestrictionExtractor : IScheduleRestrictionExtractor
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;
        private readonly INightlyRestRestrictionForTeamBlock _nightlyRestRestrictionForTeamBlock;

		public ScheduleRestrictionExtractor(IScheduleDayEquator scheduleDayEquator, INightlyRestRestrictionForTeamBlock nightlyRestRestrictionForTeamBlock)
		{
		    _scheduleDayEquator = scheduleDayEquator;
		    _nightlyRestRestrictionForTeamBlock = nightlyRestRestrictionForTeamBlock;
		}
        
		public IEffectiveRestriction Extract(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList,
                                             ISchedulingOptions schedulingOptions, TimeZoneInfo timeZone, ITeamBlockInfo teamBlockInfo)
		{
			IEffectiveRestriction restriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                             new EndTimeLimitation(),
			                                                             new WorkTimeLimitation(), null, null, null,
			                                                             new List<IActivityRestriction>());
			if (dateOnlyList == null)
				return restriction;

            var scheduleDays = internalScheduleDays(matrixList, dateOnlyList).ToArray();

            if (schedulingOptions.BlockSameShift)
			{
				var sameShiftRestriction = extractSameShift(scheduleDays);
				if (sameShiftRestriction == null) return null;
				restriction = restriction.Combine(sameShiftRestriction);
			}
			if ((schedulingOptions.UseBlock && schedulingOptions.BlockSameStartTime) || (schedulingOptions.UseTeam && schedulingOptions.TeamSameStartTime))
			{
				var sameStartRestriction = extractSameStartTime(scheduleDays, timeZone);
				if (sameStartRestriction == null) return null;
				restriction = restriction.Combine(sameStartRestriction);
			}
			if ((schedulingOptions.UseBlock && schedulingOptions.BlockSameEndTime) || (schedulingOptions.UseTeam && schedulingOptions.TeamSameEndTime))
			{
				var sameEndRestriction = extractSameEndTime(scheduleDays, timeZone);
				if (sameEndRestriction == null) return null;
				restriction = restriction.Combine(sameEndRestriction);
			}
			if ((schedulingOptions.UseBlock && schedulingOptions.BlockSameShiftCategory) || (schedulingOptions.UseTeam && schedulingOptions.TeamSameShiftCategory))
			{
				var sameShiftCategory = extractSameShiftCategory(scheduleDays);
				if (sameShiftCategory == null) return null;
				restriction = restriction.Combine(sameShiftCategory);
			}
            
            restriction =
                restriction.Combine(_nightlyRestRestrictionForTeamBlock.AggregatedNightlyRestRestriction(teamBlockInfo));
            
			return restriction;
		}

		public IEffectiveRestriction ExtractForOnePersonOneBlock(IList<DateOnly> dateOnlyList,
																 IList<IScheduleMatrixPro> matrixList,
		                                                         ISchedulingOptions schedulingOptions, TimeZoneInfo timeZone)
		{
			IEffectiveRestriction restriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                             new EndTimeLimitation(),
			                                                             new WorkTimeLimitation(), null, null, null,
			                                                             new List<IActivityRestriction>());
			if (dateOnlyList == null)
				return restriction;

			if (!schedulingOptions.UseBlock) 
				return restriction;

            var scheduleDays = internalScheduleDays(matrixList, dateOnlyList).ToArray();

            if (schedulingOptions.BlockSameShift)
			{
				var sameShiftRestriction = extractSameShift(scheduleDays);
				if (sameShiftRestriction == null) return null;
				restriction = restriction.Combine(sameShiftRestriction);
			}
			if (schedulingOptions.BlockSameStartTime)
			{
				var sameStartRestriction = extractSameStartTime(scheduleDays, timeZone);
				if (sameStartRestriction == null) return null;
				restriction = restriction.Combine(sameStartRestriction);
			}
			if (schedulingOptions.BlockSameEndTime)
			{
				var sameEndRestriction = extractSameEndTime(scheduleDays, timeZone);
				if (sameEndRestriction == null) return null;
				restriction = restriction.Combine(sameEndRestriction);
			}
			if (schedulingOptions.BlockSameShiftCategory)
			{
				var sameShiftCategory = extractSameShiftCategory(scheduleDays);
				if (sameShiftCategory == null) return null;
				restriction = restriction.Combine(sameShiftCategory);
			}
			return restriction;
		}

		public IEffectiveRestriction ExtractForOneTeamOneDay(DateOnly dateOnly, IList<IScheduleMatrixPro> matrixList,
		                                                     ISchedulingOptions schedulingOptions, TimeZoneInfo timeZone)
		{
			IEffectiveRestriction restriction = new EffectiveRestriction(new StartTimeLimitation(),
			                                                             new EndTimeLimitation(),
			                                                             new WorkTimeLimitation(), null, null, null,
			                                                             new List<IActivityRestriction>());
			var dateOnlyList = new List<DateOnly> {dateOnly};

			if (!schedulingOptions.UseTeam ) return restriction;

		    var scheduleDays = internalScheduleDays(matrixList, dateOnlyList).ToArray();

			if (schedulingOptions.TeamSameStartTime)
			{
				var sameStartRestriction = extractSameStartTime(scheduleDays, timeZone);
				if (sameStartRestriction == null) return null;
				restriction = restriction.Combine(sameStartRestriction);
			}
			if (schedulingOptions.TeamSameEndTime)
			{
				var sameEndRestriction = extractSameEndTime(scheduleDays, timeZone);
				if (sameEndRestriction == null) return null;
				restriction = restriction.Combine(sameEndRestriction);
			}
			if (schedulingOptions.TeamSameShiftCategory)
			{
				var sameShiftCategory = extractSameShiftCategory(scheduleDays);
				if (sameShiftCategory == null) return null;
				restriction = restriction.Combine(sameShiftCategory);
			}
			return restriction;
		}

        private static IEnumerable<InternalScheduleDay> internalScheduleDays(IList<IScheduleMatrixPro> matrixList, IList<DateOnly> dateOnlyList)
        {
            var scheduleDays =
                matrixList.SelectMany(m => dateOnlyList.Select(d =>
                {
                    var x = m.GetScheduleDayByKey(d);
                    return x != null ? x.DaySchedulePart() : null;
                }))
                    .Where(s => s != null)
                    .Select(
                        s =>
                        {
                            return new InternalScheduleDay
                            {
                                SignificantPart = new Lazy<SchedulePartView>(s.SignificantPart),
                                Projection = new Lazy<IVisualLayerCollection>(() => s.ProjectionService().CreateProjection()),
                                Assignment = new Lazy<IPersonAssignment>(() => s.PersonAssignment()),
                                EditorShift = new Lazy<IEditableShift>(s.GetEditorShift)
                            };
                        });
            return scheduleDays;
        }

        private IEffectiveRestriction extractSameShift(IEnumerable<InternalScheduleDay> dateOnlyList)
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
            foreach (var schedulePart in dateOnlyList)
            {
                if (schedulePart.SignificantPart.Value == SchedulePartView.MainShift)
                {
                    var mainShift = schedulePart.EditorShift.Value;
                    if (mainShift == null) continue;
                    if (restriction.CommonMainShift == null)
                    {
                        restriction.CommonMainShift = mainShift;
                    }
                    else
                    {
                        if (!_scheduleDayEquator.MainShiftBasicEquals(mainShift, restriction.CommonMainShift))
                            return null;
                    }
                }
            }
            return restriction;
		}

		private static IEffectiveRestriction extractSameShiftCategory(IEnumerable<InternalScheduleDay> dateOnlyList)
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
		    foreach (var schedulePart in dateOnlyList)
		    {
		        if (schedulePart.SignificantPart.Value == SchedulePartView.MainShift)
		        {
		            var assignment = schedulePart.Assignment.Value;
		            if (assignment == null) continue;
		            var shiftCategory = assignment.ShiftCategory;
		            if (shiftCategory == null)
		                continue;
		            if (restriction.ShiftCategory == null)
		            {
		                restriction.ShiftCategory = shiftCategory;
		            }
		            else
		            {
		                if (restriction.ShiftCategory != shiftCategory)
		                    return null;
		            }
		        }
		    }
		    return restriction;
		}

		private static IEffectiveRestriction extractSameStartTime(IEnumerable<InternalScheduleDay> dateOnlyList, TimeZoneInfo timeZone)
		{
			var startTimeLimitation = new StartTimeLimitation();
		    foreach (var schedulePart in dateOnlyList)
		    {
		        var period = schedulePart.Projection.Value.Period();
		        if (period == null) continue;
		        if (startTimeLimitation.StartTime == null && startTimeLimitation.EndTime == null)
		        {
		            var timePeriod = period.Value.TimePeriod(timeZone);
		            startTimeLimitation = new StartTimeLimitation(timePeriod.StartTime, timePeriod.StartTime);
		        }
		        else
		        {
		            var timePeriod = period.Value.TimePeriod(timeZone);
		            if (startTimeLimitation.StartTime != timePeriod.StartTime ||
		                startTimeLimitation.EndTime != timePeriod.StartTime)
		                return null;
		        }
		    }
		    var restriction = new EffectiveRestriction(startTimeLimitation,
			                                           new EndTimeLimitation(),
			                                           new WorkTimeLimitation(), null, null, null,
			                                           new List<IActivityRestriction>());
			return restriction;
		}

		private static IEffectiveRestriction extractSameEndTime(IEnumerable<InternalScheduleDay> dateOnlyList, TimeZoneInfo timeZone)
		{
			var endTimeLimitation = new EndTimeLimitation();
		    foreach (var schedulePart in dateOnlyList)
		    {
		        var period = schedulePart.Projection.Value.Period();
		        if (period == null) continue;
		        if (endTimeLimitation.StartTime == null && endTimeLimitation.EndTime == null)
		        {
		            var timePeriod = period.Value.TimePeriod(timeZone);
		            endTimeLimitation = new EndTimeLimitation(timePeriod.EndTime, timePeriod.EndTime);
		        }
		        else
		        {
		            var timePeriod = period.Value.TimePeriod(timeZone);
		            if (endTimeLimitation.StartTime != timePeriod.EndTime || endTimeLimitation.EndTime != timePeriod.EndTime)
		                return null;
		        }
		    }
		    var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   endTimeLimitation,
			                                           new WorkTimeLimitation(), null, null, null,
			                                           new List<IActivityRestriction>());
			return restriction;
		}
	}
}
