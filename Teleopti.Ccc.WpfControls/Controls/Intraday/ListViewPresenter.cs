using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.WpfControls.Controls.Intraday
{
    /// <summary>
    /// ListView sort, copy and an optional custom horizontal scrollbar
    /// Resources\ListViewDictionary.xaml must be included
    /// </summary>
    public class ListViewPresenter
    {
        private readonly ListView _listView;
        private ScrollBar _customHorizontalScrollBar;

        public ListViewPresenter(ListView listView)
        {
            _listView = listView;
            _listView.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy,(copyExecuted),(copyCanExecute)));
        }

        void copyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _listView.SelectedItems.Count > 0;
        }

        void copyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            PropertyReflector reflector = new PropertyReflector();
            StringBuilder text = new StringBuilder();
            foreach (object adapter in _listView.SelectedItems)
            {
                GridViewColumnCollection columns = (_listView.View as GridView).Columns;
                foreach (GridViewColumn column in columns)
                {
                    Binding binding = column.DisplayMemberBinding as Binding;
                    if (binding == null)
                    {
                        continue;
                    }
                    string path = binding.Path.Path;
                    object value = reflector.GetValue(adapter, path);
                    IValueConverter converter = binding.Converter;
                    if (converter != null)
                    {
                        // Convert
                        value = converter.Convert(value, null, null, null);
                    }
                    string format = binding.StringFormat;
                    if (format != null)
                    {
                        // Format
                        value = string.Format(CultureInfo.CurrentCulture, "{0:" + format + "}", value);
                    }
                    text.Append(value);
                    text.Append("\t");
                }
                text.AppendLine();
            }
            Clipboard.SetText(text.ToString());
        }

        public IEnumerable<object> ColumnValues(GridViewColumn column)
        {
            PropertyReflector reflector = new PropertyReflector();
            foreach (object adapter in _listView.Items)
            {
                Binding binding = column.DisplayMemberBinding as Binding;
                if (binding == null)
                {
                    continue;
                }
                string path = binding.Path.Path;
                object value = reflector.GetValue(adapter, path);
                IValueConverter converter = binding.Converter;
                if (converter != null)
                {
                    // Convert
                    value = converter.Convert(value, null, null, null);
                }

                yield return value;
            }
        }

        public ScrollBar CustomHorizontalScrollBar 
        {
            get { return _customHorizontalScrollBar; }
            set
            {
                _customHorizontalScrollBar = value;
                _listView.LayoutUpdated += new EventHandler(customHorizontalScrollBarLayoutUpdated);
            }
        }

        void customHorizontalScrollBarLayoutUpdated(object sender, EventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(_listView) == 0)
            {
                return;
            }
            var border = VisualTreeHelper.GetChild(_listView, 0) as Decorator;
            var scroll = border.Child as ScrollViewer;
            if (scroll.ScrollableHeight == 0)
            {
                CustomHorizontalScrollBar.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                CustomHorizontalScrollBar.Margin = new Thickness(0, 0, SystemParameters.ScrollWidth, 0);
            }

        }
    }
}