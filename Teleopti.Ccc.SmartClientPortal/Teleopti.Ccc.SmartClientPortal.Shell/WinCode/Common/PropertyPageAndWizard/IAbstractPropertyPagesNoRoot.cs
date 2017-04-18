using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard
{
    public interface IAbstractPropertyPagesNoRoot<T>
    {
        IPropertyPageNoRoot<T> FirstPage { get; }

        IPropertyPageNoRoot<T> CurrentPage { get; set; }

        IPropertyPageNoRoot<T> PreviousPage();

        IPropertyPageNoRoot<T> NextPage();

        IPropertyPageNoRoot<T> ShowPage(IPropertyPageNoRoot<T> propertyPage);

        bool IsOnFirst();

        bool IsOnLast();

        string WindowText { get; }

        ReadOnlyCollection<IPropertyPageNoRoot<T>> Pages { get; }
        
        Form Owner { get; set; }

        Size MinimumSize { get; }

        bool ModeCreateNew { get; }

        string[] GetPageNames();

        string Name { get; }
        
        void Save();

        event EventHandler PageListChanged;
    }
}
