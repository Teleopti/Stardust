using System.ComponentModel;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        protected void TriggerNotifyPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

        public event PropertyChangedEventHandler PropertyChanged;

        public void EnablePropertyChangedInvocation()
        {
            var propertyChangedHandler = PropertyChanged;
            if (propertyChangedHandler == null) return;
            var invocationList = propertyChangedHandler.GetInvocationList();
            foreach (var invocationTarget in invocationList)
            {
                var lockable = invocationTarget.Target as ILockable;
                if (IsLockableAndLocked(lockable))
                {
                    lockable.Release();
                }
            }
        }

        private static bool IsLockableAndLocked(ILockable lockable)
        {
            return lockable != null && lockable.IsLocked;
        }

        public void DisablePropertyChangedInvocation()
        {
            var propertyChangedHandler = PropertyChanged;
            if (propertyChangedHandler==null) return;
            var invocationList = propertyChangedHandler.GetInvocationList();
            foreach (var invocationTarget in invocationList)
            {
                var lockable = invocationTarget.Target as ILockable;
                if (IsLockableAndNotLocked(lockable))
                {
                    lockable.Lock();
                }
            }
        }

        private static bool IsLockableAndNotLocked(ILockable lockable)
        {
            return lockable != null && !lockable.IsLocked;
        }
    }
}