using Teleopti.Interfaces.Domain;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Common.Commands;

namespace Teleopti.Ccc.WinCode.Scheduling
{
    public interface INotesEditorViewModel
    {
        /// <summary>
        /// Get, set SchedulePart
        /// </summary>
        IScheduleDay SchedulePart { get; }
        /// <summary>
        /// Load ScheduelPart
        /// </summary>
        /// <param name="schedulePart"></param>
        void Load(IScheduleDay schedulePart);
        /// <summary>
        /// Get, set ScheduleNote
        /// </summary>
        string ScheduleNote { get; set; }
        /// <summary>
        /// Get ChangedCommand
        /// </summary>
        ICommand ChangedCommand { get; }
        /// <summary>
        /// Get DeleteCommandModel
        /// </summary>
        CommandModel DeleteCommandModel { get; }
    }
}
