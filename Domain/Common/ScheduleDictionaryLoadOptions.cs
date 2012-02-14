using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class ScheduleDictionaryLoadOptions : IScheduleDictionaryLoadOptions
    {
        public bool LoadRestrictions { get; set; }
        public bool LoadNotes { get; set; }

        public ScheduleDictionaryLoadOptions(bool loadRestrictions, bool loadNotes)
        {
            LoadRestrictions = loadRestrictions;
            LoadNotes = loadNotes;
        }   
    }
}
