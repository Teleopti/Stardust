using System;
using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Common.Clipboard
{
    /// <summary>
    /// PasteBehavior explains via PasteOption the pastebehavior in the grid.
    /// Merge-behavior merges connecting cells by pasting one long absence etc. on the first cell that
    /// stretches over the following selected cells
    public class MergePasteBehavior : PasteBehavior, IPasteBehavior
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "ret")]
        public IList<T> DoPaste<T>(GridControl gridControl, ClipHandler<T> clipHandler, IGridPasteAction<T> gridPasteAction, GridRangeInfoList rangeList)
        {
            //Will only do an mergepaste if SchedulePart
            IList<T> pasteList = new List<T>();
            T pasteResult;

            if (clipHandler.ClipList.Count > 0)
            {
                Clip<T> clip = clipHandler.ClipList[0];
                IScheduleDay part = clip.ClipValue as IScheduleDay;
                if (part != null)
                {
                    foreach (GridRangeInfo range in rangeList)
                    {
                        Clip<T> extendedClip = new Clip<T>(clip.RowOffset, clip.ColOffset, (T)ExtendAbsence(part, range.Right - range.Left));

                        for (int row = range.Top; row <= range.Bottom; row += clipHandler.RowSpan())
                        {
                            //bool ret = gridPasteAction.Paste(gridControl, extendedClip,
                            //                                 row + extendedClip.RowOffset,
                            //                                range.Left + extendedClip.ColOffset);

                            pasteResult = gridPasteAction.Paste(gridControl, extendedClip,
                                                             row + extendedClip.RowOffset,
                                                            range.Left + extendedClip.ColOffset);

                            if (pasteResult != null)
                                pasteList.Add(pasteResult);
                        }
                    }
                }
            }

            return pasteList;
        }


        #region helpers


        protected static IScheduleDay ExtendAbsence(IScheduleDay part, int days)
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
				var newPeriod = oldPeriod.ChangeEndTime(TimeSpan.FromDays(days));
				IAbsenceLayer newLayer = new AbsenceLayer(oldLayer.Payload, newPeriod);
				IPersonAbsence newPersonAbsence = new PersonAbsence(personAbsence.Person, part.Scenario, newLayer);
				part.Add(newPersonAbsence);
            }
            return part;
        }

        #endregion

    }
    
}
