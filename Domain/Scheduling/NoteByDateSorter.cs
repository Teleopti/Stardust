using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class NoteByDateSorter: IComparer<INote>
    {
        public int Compare(INote x, INote y)
        {
            return x.NoteDate.CompareTo(y.NoteDate);
        }
    }
}
