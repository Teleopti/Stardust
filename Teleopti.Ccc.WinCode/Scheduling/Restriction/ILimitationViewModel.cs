using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public interface ILimitationViewModel : INotifyPropertyChanged
    {
        ILimitation Limitation { get; }
        string StartTime { get; set; }
        string EndTime { get; set; }
        bool Editable { get; set; }
        bool EditableStartTime { get; }
        bool EditableEndTime { get; }
        bool Enabled { get; set; }
        bool InvalidStatePossible { get; set; }
        bool Invalid { get; }
        new event PropertyChangedEventHandler PropertyChanged;
    }
}
