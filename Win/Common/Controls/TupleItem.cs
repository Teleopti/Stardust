namespace Teleopti.Ccc.Win.Common.Controls
{
    /// <summary>
    /// an object and a textfield
    /// it is made for dropdowns and the communication in the sortbox
    /// feel free to use it for other purposes
    /// the equal/== != suppresion is because i dont use it like that
    /// if you have need for it implement it as you like
    /// </summary>
    /// <remarks>
    /// Created by: ostenpe
    /// Created date: 2008-09-25
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct TupleItem
    {
        private string _text;
        private object _valueMember;

        /// <summary>
        /// Initializes a new instance of the <see cref="TupleItem"/> struct.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="valueMember">The value member.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-09-25
        /// </remarks>
        public TupleItem(string text, object valueMember)
        {
            _text = text;
            _valueMember = valueMember;
        }

        /// <summary>
        /// Gets or sets the text. Typically the text visible in the dropdown
        /// </summary>
        /// <value>The text.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-09-25
        /// </remarks>
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        /// <summary>
        /// Gets or sets the value member. Typically the tag sent into the dropdown
        /// </summary>
        /// <value>The value member.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-09-25
        /// </remarks>
        public object ValueMember
        {
            get { return _valueMember; }
            set { _valueMember = value; }
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> containing a fully qualified type name.
        /// </returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2009-10-23
        /// </remarks>
        public override string ToString()
        {
            return _text;
        }
    }
}