using System.Collections.Generic;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard
{

    public interface IPropertyPageNoRoot<T>
    {
        void Populate(T stateObj);

        bool Depopulate(T stateObj);

        void SetEditMode();

        string PageName { get; }

        ICollection<string> ErrorMessages { get; }

        void Dispose();
    }
}