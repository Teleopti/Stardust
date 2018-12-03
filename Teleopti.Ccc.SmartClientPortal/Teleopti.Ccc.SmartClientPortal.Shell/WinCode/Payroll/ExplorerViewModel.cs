using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll
{
    public class ExplorerViewModel : IExplorerViewModel
    {
        private DateOnly? _date;
        private int _defaultSegment;
        private bool _isRightToLeft;

        /// <summary>
        /// Gets the multiplicator definition set.
        /// </summary>
        /// <value>The definition set.</value>
        public IList<IMultiplicatorDefinitionSet> DefinitionSetCollection { get; set; }

        /// <summary>
        /// Gets or sets the filtered definition set collection.
        /// </summary>
        /// <value>The filtered definition set collection.</value>
        public IList<IMultiplicatorDefinitionSet> FilteredDefinitionSetCollection { get; set; }

        /// <summary>
        /// Gets the multiplicator collection.
        /// </summary>
        /// <value>The multiplicator collection.</value>
        public IList<IMultiplicator> MultiplicatorCollection { get; set; }

        /// <summary>
        /// Gets the selected date.
        /// </summary>
        /// <value>The selected date.</value>
        public DateOnly? SelectedDate
        {
            get
            {
                return _date;
            }
        }

        /// <summary>
        /// Gets the default segment.
        /// </summary>
        /// <value>The default segment.</value>
        public int DefaultSegment
        {
            get
            {
                return _defaultSegment;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is right to left.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is right to left; otherwise, <c>false</c>.
        /// </value>
        public bool IsRightToLeft
        {
            get
            {
                return _isRightToLeft;
            }
        }

        /// <summary>
        /// Gets the translated weekdays collection.
        /// </summary>
        /// <value>The translated weekdays collection.</value>
        public ReadOnlyCollection<string> TranslatedWeekdaysCollection
        {
            get
            {
                IList<KeyValuePair<DayOfWeek, string>> weekDays = LanguageResourceHelper.TranslateEnumToList<DayOfWeek>();
                List<string> weekDayStrings = (from p in weekDays select p.Value).ToList();
                return new ReadOnlyCollection<string>(weekDayStrings);
            }
        }

        /// <summary>
        /// Sets the selected selectedDate.
        /// </summary>
        /// <param name="selectedDate">The date.</param>
        public void SetSelectedDate(DateOnly? selectedDate)
        {
            _date = selectedDate;
        }

        /// <summary>
        /// Sets the default segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        public void SetDefaultSegment(int segment)
        {
            _defaultSegment = segment;
        }

        /// <summary>
        /// Sets the right to left.
        /// </summary>
        /// <param name="isRightToLeft">if set to <c>true</c> [is right to left].</param>
        public void SetRightToLeft(bool isRightToLeft)
        {
            _isRightToLeft = isRightToLeft;
        }
    }
}
