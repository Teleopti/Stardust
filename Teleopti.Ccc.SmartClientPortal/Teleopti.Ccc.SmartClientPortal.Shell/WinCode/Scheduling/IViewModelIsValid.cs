namespace Teleopti.Ccc.WinCode.Scheduling
{
    /// <summary>
    /// Info for validation
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-08-30
    /// </remarks>
    public interface IViewModelIsValid
    {
        bool IsValid { get; }
        string InvalidMessage { get; }
    }
}