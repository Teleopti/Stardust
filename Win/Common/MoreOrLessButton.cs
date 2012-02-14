using System;
using System.ComponentModel;

namespace Teleopti.Ccc.Win.Common
{
    public partial class MoreOrLessButton : BaseUserControl
    {
        public event EventHandler<EventArgs> StateChanged;

        private MoreOrLessState _moreOrLessState;
        private string _moreText = "xxAdvanced";
        private string _lessText = "xxStandard";

        /// <summary>
        /// Gets or sets the less text.
        /// </summary>
        /// <value>The less text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        [Localizable(true), Browsable(true), Category("Text"), DefaultValue("xxStandard")]
        public string LessText
        {
            get { return _lessText; }
            set { _lessText = value; }
        }

        /// <summary>
        /// Gets or sets the more text.
        /// </summary>
        /// <value>The more text.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        [Localizable(true), Browsable(true), Category("Text"), DefaultValue("xxAdvanced")]
        public string MoreText
        {
            get { return _moreText; }
            set { _moreText = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoreOrLessButton"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        public MoreOrLessButton()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>The state.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        public MoreOrLessState State
        {
            get { return _moreOrLessState; }
            set
            {
                _moreOrLessState = value;
                if (_moreOrLessState == MoreOrLessState.More)
                {
                    buttonMoreOrLess.Text = _lessText;
                } else
                {
                    buttonMoreOrLess.Text = _moreText;
                }

            	var handler = StateChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [state as boolean].
        /// </summary>
        /// <value><c>true</c> if [state as boolean]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-01-07
        /// </remarks>
        public bool StateAsBoolean
        {
            get
            {
                return (_moreOrLessState == MoreOrLessState.More);
            }
            set
            {
                State = value ? MoreOrLessState.More : MoreOrLessState.Less;
            }
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        private void buttonMoreOrLess_Click(object sender, EventArgs e)
        {
            StateAsBoolean = !StateAsBoolean;
        }
    }
}