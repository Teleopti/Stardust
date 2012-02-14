using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    /// <summary>
    /// Compars the Name object. Used for name and person sorting.
    /// </summary>
    public sealed class PersonComparer : IComparer<IPerson>
    {
        private readonly NameComparer _comparer;

        public PersonComparer() : this(CultureInfo.CurrentCulture)
        {
        }

        public PersonComparer(CultureInfo cultureInfo)
        {
            _comparer = new NameComparer(cultureInfo);
        }

        #region IComparer<Name> Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public int Compare(IPerson x, IPerson y)
        {
            return _comparer.Compare(x.Name, y.Name);
        }

        #endregion
    }
}
