namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Used by DialogComposer to handle interaction between Code and Gui
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-26
    /// </remarks>
    public interface IDialogResult
    {
        bool Result { get; set; }
        string Title { get;}

        bool CanOk { get; }
    }
}