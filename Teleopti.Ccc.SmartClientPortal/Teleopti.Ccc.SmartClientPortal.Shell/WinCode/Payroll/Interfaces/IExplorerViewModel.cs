using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces
{
    /// <summary>
    /// Model for the explorer view
    /// </summary>
    /// <remarks>
    /// Created by: VirajS
    /// Created date: 2009-01-20
    /// </remarks>
    public interface IExplorerViewModel
    {
        /// <summary>
        /// Gets the multiplicator collection.
        /// </summary>
        /// <value>The multiplicator collection.</value>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        IList<IMultiplicator> MultiplicatorCollection { get; set; }

        /// <summary>
        /// Gets the multiplicator definition set.
        /// </summary>
        /// <value>The definition set.</value>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-20
        /// </remarks>
        IList<IMultiplicatorDefinitionSet> DefinitionSetCollection { get; set; }

        /// <summary>
        /// Gets or sets the filtered definition set collection.
        /// </summary>
        /// <value>The filtered definition set collection.</value>
        /// <remarks>
        /// Created by: VirajS
        /// Created date: 2009-01-21
        /// </remarks>
        IList<IMultiplicatorDefinitionSet> FilteredDefinitionSetCollection { get; set; }

        /// <summary>
        /// Gets the selected date.
        /// </summary>
        /// <value>The selected date.</value>
        DateOnly? SelectedDate { get; }

        /// <summary>
        /// Gets the default segment.
        /// </summary>
        /// <value>The default segment.</value>
        int DefaultSegment { get; }

        /// <summary>
        /// Sets the selected selectedDate.
        /// </summary>
        /// <param name="selectedDate">The date.</param>
        void SetSelectedDate(DateOnly? selectedDate);

        /// <summary>
        /// Sets the default segment.
        /// </summary>
        /// <param name="segment">The segment.</param>
        void SetDefaultSegment(int segment);

        /// <summary>
        /// Gets a value indicating whether this instance is right to left.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is right to left; otherwise, <c>false</c>.
        /// </value>
        bool IsRightToLeft { get; }

        /// <summary>
        /// Sets the right to left.
        /// </summary>
        /// <param name="isRightToLeft">if set to <c>true</c> [is right to left].</param>
        void SetRightToLeft(bool isRightToLeft);

        /// <summary>
        /// Gets the translated weekdays collection.
        /// </summary>
        /// <value>The translated weekdays collection.</value>
        ReadOnlyCollection<string> TranslatedWeekdaysCollection { get; }
    }
}
