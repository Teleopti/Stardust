using Syncfusion.Windows.Forms.Grid;
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
											Clip<T> reducedClip = new Clip<T>(clip.RowOffset, clip.ColOffset, (T)ReducedAbsence(part));
											//IScheduleDay dest = gridControl[row + reducedClip.RowOffset, col + reducedClip.ColOffset].CellValue as IScheduleDay;
											// skip the Paste if there already is a FullDayAbsence
											//if (!IsFullDayAbsence(dest))
											//{
											pasteResult = gridPasteAction.Paste(gridControl, reducedClip, row + reducedClip.RowOffset, col + reducedClip.ColOffset);
											if (pasteResult != null)
												pasteList.Add(pasteResult);
											//}
	
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

        //check if we have a full day absence
        protected static bool IsFullDayAbsence(IScheduleDay part)
        {
            return (part != null && (part.SignificantPart() == SchedulePartView.FullDayAbsence || part.SignificantPart() == SchedulePartView.ContractDayOff));
        }

        //reduce absence period to one day
        protected static IScheduleDay ReducedAbsence(IScheduleDay part)
        {
            foreach (IPersonAbsence personAbsence in part.PersonAbsenceCollection())
            {
                TimeSpan diff = personAbsence.Layer.Period.StartDateTime.Subtract(personAbsence.Layer.Period.EndDateTime);
                personAbsence.Layer.ChangeLayerPeriodEnd(diff.Add(TimeSpan.FromDays(1)));
                diff = part.Period.StartDateTime.Subtract(personAbsence.Layer.Period.StartDateTime);
                personAbsence.Layer.MoveLayer(diff);
            }

	        var ass = part.AssignmentHighZOrder();
			if(ass != null) part.Remove(ass);
	
            return part;
        }	
    }
}
