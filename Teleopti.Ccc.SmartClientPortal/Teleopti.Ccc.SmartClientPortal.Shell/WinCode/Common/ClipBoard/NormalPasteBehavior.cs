using System;
using System.Collections.Generic;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard
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
	        var specialPasteBehaviour = new SpecialPasteBehaviour();

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
										var part = clip.ClipValue as IScheduleDay;

										T pasteResult;

										if (isSchedulerSpecialPaste(gridPasteAction))
										{
											var reducedPart = specialPasteBehaviour.DoPaste(part);

											Clip<T> reducedClip = new Clip<T>(clip.RowOffset, clip.ColOffset, (T)reducedPart);

											pasteResult = gridPasteAction.Paste(gridControl, reducedClip, row + reducedClip.RowOffset,
												col + reducedClip.ColOffset);
											if (pasteResult != null)
												pasteList.Add(pasteResult);
											continue;
										}

										if(IsFullDayAbsence(part))
										{

											var reducedPart = ReducedAbsence(part);
											if (!gridPasteAction.PasteOptions.MainShift)
											{
												var ass = reducedPart.PersonAssignment();
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

	    private static bool isSchedulerSpecialPaste<T>(IGridPasteAction<T> gridPasteAction)
	    {
		    return (!gridPasteAction.PasteOptions.Default
					&& (gridPasteAction.PasteOptions.MainShift
						|| gridPasteAction.PasteOptions.PersonalShifts
						|| gridPasteAction.PasteOptions.ShiftAsOvertime
						|| gridPasteAction.PasteOptions.Preference
						|| gridPasteAction.PasteOptions.DayOff
						|| gridPasteAction.PasteOptions.Overtime
						|| gridPasteAction.PasteOptions.OvertimeAvailability
						|| gridPasteAction.PasteOptions.StudentAvailability
						|| gridPasteAction.PasteOptions.Absences == PasteAction.Add ));
	    }

	    protected static void AdjustFullDayAbsenceNextDay<T>(T part)
		{
			var destination = part as IScheduleDay;
			destination?.AdjustFullDayAbsenceNextDay(destination);
		}

        //check if we have a full day absence
        protected static bool IsFullDayAbsence(IScheduleDay part)
        {
	        if (part == null) return false;

	        var significantPart = part.SignificantPart();
	        return significantPart == SchedulePartView.FullDayAbsence || significantPart == SchedulePartView.ContractDayOff;
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
				var payLoad = personAbsence.Layer.Payload;
				var oldAbsencePeriod = personAbsence.Layer.Period;

				var newAbsencePeriod = calculateNewAbsencePeriod(part, oldAbsencePeriod);

				IAbsenceLayer newLayer = new AbsenceLayer(payLoad, newAbsencePeriod);
				IPersonAbsence newPersonAbsence = new PersonAbsence(personAbsence.Person, part.Scenario, newLayer);
				part.Add(newPersonAbsence);
            }

            return part;
        }

	    private static DateTimePeriod calculateNewAbsencePeriod(IScheduleDay part, DateTimePeriod oldPeriod)
	    {
		    var oldLenght = oldPeriod.StartDateTime.Subtract(oldPeriod.EndDateTime);
		    var shortedLength = oldLenght.Add(TimeSpan.FromDays(1));
		    var newPeriod = oldPeriod.ChangeEndTime(shortedLength);
		    var newLenght = part.Period.StartDateTime.Subtract(newPeriod.StartDateTime);
		    newPeriod = newPeriod.MovePeriod(newLenght);
		    return newPeriod;
	    }
    }
}
