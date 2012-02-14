using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Compars the Name object. Used for name and person sorting.
    /// </summary>
    public sealed class NameComparer : IComparer<Name>
    {
        private readonly CultureInfo _cultureInfo;

        public NameComparer() : this(CultureInfo.CurrentCulture)
        {
        }

        public NameComparer(CultureInfo cultureInfo)
        {
            _cultureInfo = cultureInfo;
        }

        #region IComparer<Name> Members

        public int Compare(Name x, Name y)
        {
            if (x.LastName == y.LastName)
                return string.Compare(x.FirstName, y.FirstName, true, _cultureInfo);
            else
                return string.Compare(x.LastName, y.LastName, true, _cultureInfo);
        }

        #endregion
    }
}
