namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
    public interface ILockable
    {
        void Lock();
        void Release();
        bool IsLocked { get; }
    }
}
