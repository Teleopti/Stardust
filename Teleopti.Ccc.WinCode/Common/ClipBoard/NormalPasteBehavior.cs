using System.Linq;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;
using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Common.Clipboard
{
    /// <summary>
    /// PasteBehavior explains via PasteOption the pastebehavior in the grid. 
    /// Standard pastebehavior is writing each clip in every cell thats selected.
    /// Merge-behavior merges connecting cells by pasting one long absence etc. on the first cell that
    /// stretches over the following selected cells
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-09-04
    /// </remarks>
    public class NormalPasteBehavior : PasteBehavior, IPasteBehavior
    {
        /// <summary>
        /// Todo Fix this, moved from Win, rewrite & test
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gridControl"></param>
        /// <param name="clipHandler"></param>
        /// <param name="gridPasteAction"></param>
        /// <param name="rangeList"></param>
        public IList<T> DoPaste<T>(GridControl gridControl, ClipHandler<T> clipHandler, IGridPasteAction<T> gridPasteAction, GridRangeInfoList rangeList)
        {
            IList<T> pasteList = new List<T>();
			
            if (clipHandler.ClipList.Count > 0)
            {
                foreach (GridRangeInfo range in rangeList)
                {
                    //loop all rows in selection, step with height in clip
                    for (int row = range.Top; row <= range.Bottom; row += clipHandler.RowSpan())
                    {
                        //loop all columns in selection, step with in clip
                        for (int col = range.Left; col <= range.Right; col += clipHandler.ColSpan())
                        {
                            if (row > gridControl.Rows.HeaderCount && col > gridControl.Cols.HeaderCount)
                            {
                                foreach (Clip<T> clip in clipHandler.ClipList)
                                {
                                    //check clip fits inside selected range, rows
                                    if (IsPasteRangeOk(range, gridControl, clip.RowOffset, clip.ColOffset, row, col))
                                    {
                                        IScheduleDay part = clip.ClipValue as IScheduleDay;

                                        T pasteResult;
										if(IsFullDayAbsence(part))
										{

											var reducedPart = ReducedAbsence(part);
											if (!gridPasteAction.PasteOptions.MainShift)
											{
												var ass = reducedPart.AssignmentHighZOrder();
												if (ass != null) reducedPart.Remove(ass);
											}

											Clip<T> reducedClip = new Clip<T>(clip.RowOffset, clip.ColOffset, (T)reducedPart);

											pasteResult = gridPasteAction.Paste(gridControl, reducedClip, row + reducedClip.RowOffset, col + reducedClip.ColOffset);
											if (pasteResult != null)
											{
												pasteList.Add(pasteResult);
												foreach (var item in pasteList)
												{
													AdjustFullDayAbsenceNextDay(item);
												}
												
											}
										}
										else
										{
                                            pasteResult = gridPasteAction.Paste(gridControl, clip, row + clip.RowOffset, col + clip.ColOffset);
                                            if (pasteResult != null)
                                                pasteList.Add(pasteResult);
										}
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return pasteList;
        }

		protected static void AdjustFullDayAbsenceNextDay<T>(T part)
		{
			var destination = part as IScheduleDay;
			if (destination == null) return;

			var assignment = destination.PersonAssignment();
			if (assignment == null) return;

			var dayPeriod = destination.DateOnlyAsPeriod.Period();
			var nextDay = assignment.Period.EndDateTime > dayPeriod.EndDateTime && assignment.Period.StartDateTime < dayPeriod.EndDateTime;

			if (!nextDay) return;

			IList<IPersonAbsence> allAbsences = new List<IPersonAbsence>(destination.PersonAbsenceCollection());
			foreach (IPersonAbsence personAbsence in destination.PersonAbsenceCollection())
			{
				if(personAbsence.Period.StartDateTime < destination.Period.EndDateTime)
					destination.Remove(personAbsence);
			}

			foreach (var personAbsence in allAbsences)
			{
				if(personAbsence.Period.StartDateTime >= destination.Period.EndDateTime) continue;
				var oldLayer = personAbsence.Layer;
				var oldPeriod = oldLayer.Period;

				var diffEnd = oldPeriod.EndDateTime.Subtract(assignment.Period.EndDateTime);
				var diffStart = oldPeriod.StartDateTime.Subtract(assignment.Period.StartDateTime);
				var newPeriod = oldPeriod.ChangeEndTime(-diffEnd);
				newPeriod = newPeriod.ChangeStartTime(-diffStart);

				IAbsenceLayer newLayer = new AbsenceLayer(oldLayer.Payload, newPeriod);
				IPersonAbsence newPersonAbsence = new PersonAbsence(personAbsence.Person, destination.Scenario, newLayer);
				destination.Add(newPersonAbsence);	
			}
		}

        //check if we have a full day absence
        protected static bool IsFullDayAbsence(IScheduleDay part)
        {
            return (part != null && (part.SignificantPart() == SchedulePartView.FullDayAbsence || part.SignificantPart() == SchedulePartView.ContractDayOff));
        }

        //reduce absence period to one day
        protected static IScheduleDay ReducedAbsence(IScheduleDay part)
        {
			IList<IPersonAbsence> allAbsences = new List<IPersonAbsence>(part.PersonAbsenceCollection());
			foreach (IPersonAbsence personAbsence in part.PersonAbsenceCollection())
			{
				part.Remove(personAbsence);
			}
			foreach (IPersonAbsence personAbsence in allAbsences)
            {
	            var oldLayer = personAbsence.Layer;
	            var oldPeriod = oldLayer.Period;
				var diff = oldPeriod.StartDateTime.Subtract(oldPeriod.EndDateTime);
				var newPeriod = oldPeriod.ChangeEndTime(diff.Add(TimeSpan.FromDays(1)));
				diff = part.Period.StartDateTime.Subtract(newPeriod.StartDateTime);
	            newPeriod = newPeriod.MovePeriod(diff);
				IAbsenceLayer newLayer = new AbsenceLayer(oldLayer.Payload, newPeriod);
				IPersonAbsence newPersonAbsence = new PersonAbsence(personAbsence.Person, part.Scenario, newLayer);
				part.Add(newPersonAbsence);
            }

			
	
            return part;
        }
    }
}
