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
using Teleopti.Ccc.WinCode.Converters;

namespace Teleopti.Ccc.WpfControls.Controls.Intraday
{
    /// <summary>
    /// ListView sort, copy and an optional custom horizontal scrollbar
    /// Resources\ListViewDictionary.xaml must be included
    /// </summary>
    public class ListViewPresenter
    {
        private readonly ListView _listView;
        private GridViewColumnHeader _lastHeaderClicked;
        private ListSortDirection _lastDirection;
        private ScrollBar _customHorizontalScrollBar;

        public ListViewPresenter(ListView listView)
        {
            _listView = listView;
            _listView.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy,(CopyExecuted),(CopyCanExecute)));
            //_listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(header_Click));
        }

        void CopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _listView.SelectedItems.Count > 0;
        }

        void CopyExecuted(object sender, ExecutedRoutedEventArgs e)
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

        public event EventHandler<EventArgs> SortChanged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void header_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked == null)
            {
                // Not a header header click
                return;
            }
            Binding binding = headerClicked.Column.DisplayMemberBinding as Binding;
            if (binding == null)
            {
                return;
            }

            if (headerClicked != _lastHeaderClicked)
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }

            if (_lastHeaderClicked != null)
            {
                _lastHeaderClicked.Column.HeaderTemplate = null;
            }
            if (direction == ListSortDirection.Ascending)
            {
                headerClicked.Column.HeaderTemplate = _listView.FindResource("headerTemplateArrowUp") as DataTemplate;
            }
            else
            {
                headerClicked.Column.HeaderTemplate = _listView.FindResource("headerTemplateArrowDown") as DataTemplate;
            }

            string path = binding.Path.Path;
            IValueConverter converter = binding.Converter;
            Sort(path, direction, converter);

            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;

            if (SortChanged != null)
            {
                SortChanged(this, EventArgs.Empty);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void Sort(string sortBy, ListSortDirection direction, IValueConverter converter)
        {
            _listView.Items.SortDescriptions.Clear();
            if (converter != null)
            {
                // Sort with converter
                ListCollectionView view = CollectionViewSource.GetDefaultView(_listView.ItemsSource) as ListCollectionView;
                view.CustomSort = new ConverterComparer(direction, sortBy, converter);
            }
            else
            {
                SortDescription sd = new SortDescription(sortBy, direction);
                _listView.Items.SortDescriptions.Add(sd);
            }
        }

        public void Sort()
        {
            CollectionViewSource.GetDefaultView(_listView.ItemsSource).Refresh();
        }

        public GridViewColumn SortColumn { get { return _lastHeaderClicked == null ? null : _lastHeaderClicked.Column; } }

        public ScrollBar CustomHorizontalScrollBar 
        {
            get { return _customHorizontalScrollBar; }
            set
            {
                _customHorizontalScrollBar = value;
                _listView.LayoutUpdated += new EventHandler(CustomHorizontalScrollBarLayoutUpdated);
            }
        }

        void CustomHorizontalScrollBarLayoutUpdated(object sender, EventArgs e)
        {
            // Auto margin for the custom scroll bar
            if (VisualTreeHelper.GetChildrenCount(_listView) == 0)
            {
                return;
            }
            Decorator border = VisualTreeHelper.GetChild(_listView, 0) as Decorator;
            ScrollViewer scroll = border.Child as ScrollViewer;
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