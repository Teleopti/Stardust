using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters
{
    public class ConverterComparer : IComparer
    {
        private readonly int _direction;
        private readonly string _path;
        private readonly IValueConverter _converter;
        private readonly PropertyReflector _reflector = new PropertyReflector();

        public ConverterComparer(ListSortDirection direction, string path, IValueConverter converter)
        {
            _direction = direction == ListSortDirection.Ascending ? 1 : -1;
            _converter = converter;
            _path = path;
        }

        public int Compare(object x, object y)
        {
            object object1 = _reflector.GetValue(x, _path);
            object object2 = _reflector.GetValue(y, _path);
            return _direction * string.Compare(
                (_converter.Convert(object1, null, null, CultureInfo.CurrentUICulture) ?? "").ToString(),
                (_converter.Convert(object2, null, null, CultureInfo.CurrentUICulture) ?? "").ToString(), true, CultureInfo.CurrentUICulture);
        }
    }
}
