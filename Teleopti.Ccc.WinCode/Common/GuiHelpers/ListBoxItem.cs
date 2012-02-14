namespace Teleopti.Ccc.WinCode.Common.GuiHelpers
{
    /// <summary>
    /// Represents an 'object - displayed text' item in ListBoxes on Gui
    /// </summary>
    public class ListBoxItem<TValue>
    {
        #region Variables

        private TValue _value;
        private string _displayText;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxItem&lt;TValue&gt;"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="displayText">The display text.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-05-20
        /// </remarks>
        public ListBoxItem(TValue value, string displayText)
        {
            _value = value;
            _displayText = displayText;
        }

        #endregion

        #region Interface

        /// <summary>
        /// Gets or sets the value part.
        /// </summary>
        /// <value>The value.</value>
        public TValue Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets or sets the display text part.
        /// </summary>
        /// <value>The display.</value>
        public string DisplayText
        {
            get { return _displayText; }
            set { _displayText = value; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// In this case it is the Display part of the item.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return _displayText;
        }

        #endregion
    }
}