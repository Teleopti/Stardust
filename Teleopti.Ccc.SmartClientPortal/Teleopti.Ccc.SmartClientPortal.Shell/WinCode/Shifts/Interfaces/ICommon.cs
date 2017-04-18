using System.Collections.ObjectModel;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface ICommon<T>
    {
        ReadOnlyCollection<T> ModelCollection { get; }

        void SetModelCollection(ReadOnlyCollection<T> models);

        void AddToModelCollection(T model);

        void RemoveFromCollection(T model);

        void ClearModelCollection();
    }
}
