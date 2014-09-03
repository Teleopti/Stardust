using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common.Controls;

namespace Teleopti.Ccc.Win.Common
{
    public partial class PromptTextBox : BaseDialogForm
    {
        internal event EventHandler<CustomEventArgs<TupleItem>> NameThisView;
        private readonly object _tag;
        private readonly string _name;
        private readonly string _type;
        private readonly int _maxLength;
	    private readonly Func<string, bool> _validationMethod; 
        private string _helpId;

        protected PromptTextBox()
        {
            InitializeComponent();
            _helpId = Name;
            if(!DesignMode) SetTexts();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PromptTextBox"/> class. That can be used to show a rename dialogue
        /// </summary>
        /// <param name="tag">The tag, perhaps the object you are going to rename.</param>
        /// <param name="name">The current name.</param>
        /// <param name="type">The name of the type you are naming.</param>
        /// <param name="maxLength">Maximum length of the name.</param>
        public PromptTextBox(object tag, string name, string type, int maxLength) : this()
        {
            _tag = tag;
            _name = name;
            _type = type;
            _maxLength = maxLength;
        }

		public PromptTextBox(object tag, string name, string type, int maxLength, Func<string, bool> validationMethod)
			: this(tag, name, type, maxLength)
		{
			_validationMethod = validationMethod;
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text = String.Format(CultureInfo.InvariantCulture, Text, _type.ToLower(CultureInfo.CurrentCulture));
            labelName.Text = String.Format(CultureInfo.InvariantCulture, labelName.Text, _type.ToLower(CultureInfo.CurrentCulture));
            textBoxExt1.Text = _name;
            textBoxExt1.MaxLength = _maxLength;
        }

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			textBoxExt1.Focus();
			textBoxExt1.SelectAll();
		}

        private void buttonAdvSave_Click(object sender, EventArgs e)
        {
            string viewName = textBoxExt1.Text.Trim();
            if (string.IsNullOrEmpty(viewName)) return;
            if(viewName.Length>_maxLength)
            {
                ShowErrorMessage(UserTexts.Resources.TextTooLong, UserTexts.Resources.TextTooLong);
                return;
            }
			if (_validationMethod != null && !_validationMethod.Invoke(viewName))
			{
				ShowErrorMessage(UserTexts.Resources.NameAlreadyExists, UserTexts.Resources.NameAlreadyExists);
				return;
			}
	        Hide();

        	var handler = NameThisView;
            if (handler!= null)
            {
                var tuple = new TupleItem(viewName, _tag);
                handler.Invoke(this, new CustomEventArgs<TupleItem>(tuple));
            }
            Close();
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public override string HelpId
        {
            get
            {
                return _helpId;
            }
        }

        public void SetHelpId(string helpId)
        {
            _helpId = helpId;
        }
    }
}
