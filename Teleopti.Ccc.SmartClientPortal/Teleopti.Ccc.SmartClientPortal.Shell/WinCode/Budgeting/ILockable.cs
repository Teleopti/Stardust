namespace Teleopti.Ccc.WinCode.Budgeting
{
    public interface ILockable
    {
        void Lock();
        void Release();
        bool IsLocked { get; }
    }
}
