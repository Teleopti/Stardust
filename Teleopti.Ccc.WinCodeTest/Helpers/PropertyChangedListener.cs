using System.Collections.Generic;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCodeTest.Helpers
{
    /// <summary>
    /// Helper for listening to PropertyChanges
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class PropertyChangedListener : Stack<string>
    {
        public PropertyChangedListener ListenTo(INotifyPropertyChanged target)
        {
            target.PropertyChanged += ((sender, e) => { Push(e.PropertyName); });
            return this;
        }

        public bool HasFired(string property)
        {
            return Contains(property) || Contains(string.Empty);
        }

        public bool HasOnlyFired(string property)
        {
            return (HasFired(property) && Count == 1);
        }

    }
}
