using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Manager.IntegrationTest.WPF.Converters
{
    public class RowNumberConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
                              Type targetType,
                              object parameter,
                              CultureInfo culture)
        {
            var item = values[0];

            DataGrid grid = values[1] as DataGrid;

            if (grid != null)
            {
                int index = grid.Items.IndexOf(item);

                index += 1;

                return index.ToString();
            }

            return null;
        }

        public object[] ConvertBack(object value,
                                    Type[] targetTypes,
                                    object parameter,
                                    CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}