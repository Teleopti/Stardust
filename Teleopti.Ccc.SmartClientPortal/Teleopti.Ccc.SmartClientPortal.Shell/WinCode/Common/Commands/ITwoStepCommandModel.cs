using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Common.Commands
{
    /// <summary>
    /// Command that uses two steps.
    /// First, the user must set the commands edit-mode to true (by setting the property or calling the EditCommand)
    /// Then Execute the normal command, cancel or manually reset the edit-property
    /// </summary>
    /// <remarks>
    /// Sample of usage:
    /// Editing or adding  a item: first set it to editmode, edit properties, press ok or cancel.
    /// Created by: henrika
    /// Created date: 2009-08-25
    /// </remarks>
    public interface ITwoStepCommandModel : INotifyPropertyChanged
    {
        bool EditMode { get; set; }
        CommandModel EditCommandModel { get; }
        CommandModel CancelEditCommandModel { get; }
    }

}
