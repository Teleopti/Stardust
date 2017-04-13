using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Budgeting
{
	public partial class PromptDoubleBox : BaseDialogForm
	{
		private readonly double _defaultValue;
		private readonly string _type;
		private readonly double _minValue;
		private readonly double _maxValue;
		private string _helpId;

		protected PromptDoubleBox()
		{
			InitializeComponent();
			_helpId = Name;
			if(!DesignMode) SetTexts();
		}

		public PromptDoubleBox(double defaultValue, string type, double minValue, double maxValue) : this()
		{
			_defaultValue = defaultValue;
			_type = type;
			_minValue = minValue;
			_maxValue = maxValue;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Text = String.Format(CultureInfo.InvariantCulture, _type);
			labelName.Text = String.Format(CultureInfo.InvariantCulture, _type);
			numericTextBox1.DoubleValue = _defaultValue;
			numericTextBox1.MinValue = _minValue;
			numericTextBox1.MaxValue = _maxValue;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			numericTextBox1.Focus();
			numericTextBox1.SelectAll();
		}

		private void buttonAdvSaveClick(object sender, EventArgs e)
		{
			if (numericTextBox1.IsValid())
				Result = numericTextBox1.DoubleValue;
		}

		public double? Result { get; private set; }

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			Result = null;
		}

		private void numericTextBox1TextChanged(object sender, EventArgs e)
		{
			if (numericTextBox1.IsValid())
			{
				numericTextBox1.ForeColor = Color.Black;
				buttonAdvSave.Enabled = true;
			}
			else
			{
				numericTextBox1.ForeColor = Color.Red;
				buttonAdvSave.Enabled = false;
			}
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
