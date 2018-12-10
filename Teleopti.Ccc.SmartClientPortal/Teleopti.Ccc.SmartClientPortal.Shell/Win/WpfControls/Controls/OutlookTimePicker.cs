using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls
{
    /// <summary>
    /// A time picker control which has same functionality as Outlook's
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-14
    /// </remarks>
    [Browsable(true), Category("Teleopti Controls")]
    public class OutlookTimePicker : ComboBox
    {
        private DateTime baseDate = new DateTime(2007, 1, 1);

        /// <summary>
        /// Raises the <see cref="E:System.Windows.FrameworkElement.Initialized"/> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized"/> is set to true internally.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            IList<string> timeList = new List<string>();
            DateTime maxTime = baseDate.Add(new TimeSpan(23, 59, 59));
            for (DateTime timeOfDay = baseDate;
                timeOfDay < maxTime;
                timeOfDay = timeOfDay.AddMinutes(30))
            {
                timeList.Add(timeOfDay.ToShortTimeString());
            }
            ItemsSource = timeList;
        }

        /// <summary>
        /// Reports when a combo box's popup opens.
        /// </summary>
        /// <param name="e">The event data for the <see cref="E:System.Windows.Controls.ComboBox.DropDownOpened"/> event.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);
            FormatTextAsTime();
        }

        

        /// <summary>
        /// Raises the <see cref="E:System.Windows.UIElement.LostFocus"/> GTMT#routed event by using the event data that is provided.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.RoutedEventArgs"/> that contains event data. This event data must contains the identifier for the <see cref="E:System.Windows.UIElement.LostFocus"/> event.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            FormatTextAsTime();
        }

        /// <summary>
        /// Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.LostKeyboardFocus"/> GTMT#attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs"/> that contains event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-16
        /// </remarks>
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            FormatTextAsTime();
        }

        /// <summary>
        /// Formats the text as time.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        private void FormatTextAsTime()
        {
			if (GetTimeInformation(out var timeAsText, out _))
                Text = timeAsText;
        }

        /// <summary>
        /// Gets the time information.
        /// </summary>
        /// <param name="timeAsString">The time as string.</param>
        /// <param name="timeAsTimeSpan">The time as time span.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-14
        /// </remarks>
        private bool GetTimeInformation(out string timeAsString, out TimeSpan timeAsTimeSpan)
        {
            if (TimeHelper.TryParse(Text, out timeAsTimeSpan))
            {
                timeAsString = DateTime.MinValue.Add(timeAsTimeSpan).ToShortTimeString();
                return true;
            }

            timeAsString = Text;
            return false;
        }

        /// <summary>
        /// Gets the time value.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-15
        /// </remarks>
        public TimeSpan TimeValue()
        {
			if (!GetTimeInformation(out _, out var timeOfDay))
                throw new ArgumentException("You must specify a valid time!");
            return timeOfDay;
        }
    }
}
