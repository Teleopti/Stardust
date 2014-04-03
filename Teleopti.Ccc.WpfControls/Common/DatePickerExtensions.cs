using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Teleopti.Ccc.WpfControls.Common
{
    
       
    public enum DatePickerIncrementMode
    {
        NoIncrement = 0,
        FullIncrement = 1,
        PartIncrement = 2
    };

    /// <summary>
    /// Winform functionality to wpftoolkit-datepicker
    /// </summary>
    /// <remarks>
    /// Created by: http://github.com/TomDudfield/DatePickerExtensions
    /// Created date: 2010-09-03
    /// usage sample: DatePicker DatepickerExtensions:DatePickerExtensions.IncrementMode="FullIncrement"
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    public class DatePickerExtensions
    {
        private enum VerticalDirection { Up, Down }
        private enum HorizontalDirection { Left, Right }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyPropertyKey LongDateSelectorPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("LongDateSelector",
                                                        typeof(DateElementSelector),
                                                        typeof(DatePicker),
                                                        new PropertyMetadata(null));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyPropertyKey ShortDateSelectorPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ShortDateSelector",
                                                        typeof(DateElementSelector),
                                                        typeof(DatePicker),
                                                        new PropertyMetadata(null));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static DependencyProperty IncrementModeProperty =
            DependencyProperty.RegisterAttached("IncrementMode",
                                                typeof(DatePickerIncrementMode),
                                                typeof(DatePicker),
                                                new PropertyMetadata(DatePickerIncrementMode.NoIncrement, OnIncrementModeChanged));

        public static void SetIncrementMode(DependencyObject element, DatePickerIncrementMode value)
        {
            element.SetValue(IncrementModeProperty, value);
        }

        public static DatePickerIncrementMode GetIncrementMode(DependencyObject element)
        {
            return (DatePickerIncrementMode)element.GetValue(IncrementModeProperty);
        }

        private static void OnIncrementModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DatePicker datePicker = d as DatePicker;

            if (datePicker == null)
                return;

            //longDateSelector below is removed because long date format (as US format) can't be handled correctly /Maria
            //DateElementSelector longDateSelector = datePicker.GetValue(LongDateSelectorPropertyKey.DependencyProperty) as DateElementSelector;
            DateElementSelector shortDateSelector = datePicker.GetValue(ShortDateSelectorPropertyKey.DependencyProperty) as DateElementSelector;

            //if (longDateSelector == null)
            //    datePicker.SetValue(LongDateSelectorPropertyKey, new DateElementSelector(DatePickerFormat.Long));

            if (shortDateSelector == null)
                datePicker.SetValue(ShortDateSelectorPropertyKey, new DateElementSelector(DatePickerFormat.Short));

            datePicker.PreviewKeyDown -= OnPreviewKeyDown;
            datePicker.PreviewKeyDown += OnPreviewKeyDown;
            datePicker.PreviewMouseWheel -= OnPreviewMouseWheel;
            datePicker.PreviewMouseWheel += OnPreviewMouseWheel;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;

            if (datePicker == null)
                return;

            DateElementSelector selector;

            if (e.Key == Key.T && Keyboard.Modifiers == ModifierKeys.Control)
            {
                datePicker.SetValue(DatePicker.SelectedDateProperty, DateTime.Now);
                e.Handled = true;
            }

            if (datePicker.GetValue(DatePicker.SelectedDateProperty) != null && GetIncrementMode((DatePicker)sender) != DatePickerIncrementMode.NoIncrement)
            {
                if ((DatePickerFormat)datePicker.GetValue(DatePicker.SelectedDateFormatProperty) == DatePickerFormat.Long)
                    selector = datePicker.GetValue(LongDateSelectorPropertyKey.DependencyProperty) as DateElementSelector;
                else
                    selector = datePicker.GetValue(ShortDateSelectorPropertyKey.DependencyProperty) as DateElementSelector;

                if (selector == null)
                    return;

                switch (e.Key)
                {
                    case Key.Left:
                        selector.ToggleSelectedDateElement(HorizontalDirection.Left, datePicker);
                        e.Handled = true;
                        break;

                    case Key.Right:
                        selector.ToggleSelectedDateElement(HorizontalDirection.Right, datePicker);
                        e.Handled = true;
                        break;

                    case Key.Up:
                        selector.ChangeDate(VerticalDirection.Up, datePicker);
                        e.Handled = true;
                        break;

                    case Key.Down:
                        selector.ChangeDate(VerticalDirection.Down, datePicker);
                        e.Handled = true;
                        break;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;

            if (datePicker == null)
                return;

            DateElementSelector selector;

            if (datePicker.GetValue(DatePicker.SelectedDateProperty) != null && GetIncrementMode((DatePicker)sender) != DatePickerIncrementMode.NoIncrement)
            {
                if ((DatePickerFormat)datePicker.GetValue(DatePicker.SelectedDateFormatProperty) == DatePickerFormat.Long)
                    selector = datePicker.GetValue(LongDateSelectorPropertyKey.DependencyProperty) as DateElementSelector;
                else
                    selector = datePicker.GetValue(ShortDateSelectorPropertyKey.DependencyProperty) as DateElementSelector;

                if (selector == null)
                    return;

                if (e.Delta > 0)
                {
                    selector.ChangeDate(VerticalDirection.Up, datePicker);
                    e.Handled = true;
                }
                else
                {
                    selector.ChangeDate(VerticalDirection.Down, datePicker);
                    e.Handled = true;
                }
            }
        }

        private class DateElementSelector
        {
            private struct DateElement
            {
                public int StartPos;    // Where is element located in format string
                public int EndPos;      //                  " "
                public int Length;      // StartPos - EndPos
                public char Element;    // element type (d, M or y)
            }

            private DateElement[] _elements;            // elements based on format string
            private DateElement[] _adjustedelements;    // elements adjused (due to variable length month names)

            private const char Day = 'd';
            private const char Month = 'M';
            private const char Year = 'y';

            private readonly bool _variableMonthLength;          // flag to indicate whether this date format has variable length month
            private readonly string[] _months;                   // contains the list of months based on culture

            public DateElementSelector(DatePickerFormat format)
            {
                string dateFormat;
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;

                // Get date format for current culture
                if (format == DatePickerFormat.Long)
                    dateFormat = cultureInfo.DateTimeFormat.LongDatePattern;
                else
                    dateFormat = cultureInfo.DateTimeFormat.ShortDatePattern;

                // Populate _elements array based on date format
                PopulateElements(dateFormat);

                // If date format contains long month name then add these names to _months array
                if (dateFormat.Contains("MMMM"))
                {
                    _variableMonthLength = true;
                    _months = cultureInfo.DateTimeFormat.MonthNames;
                }
                else
                {
                    _variableMonthLength = false;
                    _months = null;
                }
            }

            private void PopulateElements(string dateFormat)
            {
                _elements = new DateElement[3];
                _adjustedelements = new DateElement[3];

                int elementno = -1;
                char last = '!';
                char[] formatChars = dateFormat.ToCharArray();

                for (int i = 0; i < formatChars.Length; i++)
                {
                    char c = formatChars[i];

                    if (c == Day || c == Month || c == Year)
                    {
                        if (elementno < _elements.Length)
                        {
                            if (last != c)
                            {
                                elementno += 1;
                                _elements[elementno].Element = c;
                                _elements[elementno].StartPos = i;
                                _elements[elementno].EndPos = i;
                                _elements[elementno].Length = 1;
                            }
                            else
                            {
                                _elements[elementno].EndPos = i;
                                _elements[elementno].Length = (i - _elements[elementno].StartPos) + 1;
                            }

                            last = c;
                        }
                    }
                }

                _elements.CopyTo(_adjustedelements, 0);
            }

            public void ToggleSelectedDateElement(HorizontalDirection horizontalDirection, DatePicker datePicker)
            {
                int elementno = GetCurrentElement(datePicker.GetValue(DatePicker.TextProperty) as string);

                if (horizontalDirection == HorizontalDirection.Right)
                    elementno += 1;
                else
                    elementno -= 1;

                if (elementno >= _adjustedelements.Length)
                    elementno = 0;

                if (elementno < 0)
                    elementno = _adjustedelements.Length - 1;

                SelectElement(elementno);
            }

            public void ChangeDate(VerticalDirection verticalDirection, DatePicker datePicker)
            {
                int adjustment;
                DateTime newDate = new DateTime();
                int elementno = GetCurrentElement(datePicker.GetValue(DatePicker.TextProperty) as string);

                if (verticalDirection == VerticalDirection.Up)
                    adjustment = 1;
                else
                    adjustment = -1;

                DateTime? selectedDate = datePicker.GetValue(DatePicker.SelectedDateProperty) as DateTime?;

                if (selectedDate != null)
                {
                    DateTime currentDate = selectedDate.Value;

                    switch (_adjustedelements[elementno].Element)
                    {
                        case Day:
                            newDate = CultureInfo.CurrentCulture.Calendar.AddDays(currentDate,adjustment);
                            break;

                        case Month:
                            newDate = CultureInfo.CurrentCulture.Calendar.AddMonths(currentDate, adjustment);
                            break;

                        case Year:
                            newDate = CultureInfo.CurrentCulture.Calendar.AddYears(currentDate, adjustment);
                            break;

                        default:
                            newDate = selectedDate.Value;
                            break;
                    }

                    if (GetIncrementMode(datePicker) == DatePickerIncrementMode.PartIncrement)
                    {
                        if ((_adjustedelements[elementno].Element == Day) && (currentDate.Month != newDate.Month))
                        {
                            newDate = newDate < currentDate ? CultureInfo.CurrentCulture.Calendar.AddMonths(newDate, 1) : CultureInfo.CurrentCulture.Calendar.AddMonths(newDate, -1);
                        }

                        if ((_adjustedelements[elementno].Element == Month) && (currentDate.Year != newDate.Year))
                        {
                            newDate = newDate < currentDate ? CultureInfo.CurrentCulture.Calendar.AddYears(newDate,1) : CultureInfo.CurrentCulture.Calendar.AddYears(newDate, -1);
                        }
                    }
                }

                datePicker.SetValue(DatePicker.SelectedDateProperty, newDate);
                //AdjustElements(datePicker.GetValue(DatePicker.TextProperty) as string);
                SelectElement(elementno);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
            private int GetCurrentElement(string currentText)
            {
                DatePickerTextBox datePickerTextBox;

                AdjustElements(currentText);

                int returnVal = 0;
                int elementno;
                int selStart = 0;

                IInputElement inputElement = Keyboard.FocusedElement;

                if (inputElement is DatePickerTextBox)
                {
                    datePickerTextBox = (DatePickerTextBox)inputElement;
                    selStart = datePickerTextBox.SelectionStart;
                }

                for (elementno = 0; elementno < 3; elementno += 1)
                {
                    if (selStart >= _adjustedelements[elementno].StartPos)
                        returnVal = elementno;
                }

                return returnVal;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
            private void SelectElement(int elementno)
            {
                IInputElement inputElement = Keyboard.FocusedElement;

                if (inputElement is DatePickerTextBox)
                {
                    DatePickerTextBox datePickerTextBox = (DatePickerTextBox)inputElement;
                    datePickerTextBox.SelectionStart = _adjustedelements[elementno].StartPos;
                    datePickerTextBox.SelectionLength = _adjustedelements[elementno].Length;
                }
            }

            private void AdjustElements(string currentText)
            {
                if (!_variableMonthLength)
                    return;

                _elements.CopyTo(_adjustedelements, 0);

                int adjustment = 0;
                bool foundMonth = false;

                foreach (string month in _months)
                {
                    if (currentText.Contains(month) && month.Length > 0)
                    {
                        adjustment = month.Length - 4;
                        break;
                    }
                }

                for (int i = 0; i < 3; i++)
                {
                    if (_adjustedelements[i].Element == Month)
                        foundMonth = true;

                    if (foundMonth)
                    {
                        if (_adjustedelements[i].Element != Month)
                            _adjustedelements[i].StartPos = _adjustedelements[i].StartPos + adjustment;

                        _adjustedelements[i].EndPos = _adjustedelements[i].EndPos + adjustment;
                        _adjustedelements[i].Length = (_adjustedelements[i].EndPos - _adjustedelements[i].StartPos) + 1;
                    }
                }
            }
        }
    }
}

    

