using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class PublicNoteByDateSorter: IComparer<IPublicNote>
    {
        public int Compare(IPublicNote x, IPublicNote y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            return x.NoteDate.CompareTo(y.NoteDate);
        }
    }
}
