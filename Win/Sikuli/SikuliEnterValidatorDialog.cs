﻿using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.Win.Sikuli
{
	public partial class SikuliEnterValidatorDialog : MetroForm
	{

		public SikuliEnterValidatorDialog()
		{
			InitializeComponent();
		}

		public string GetValidatorName
		{
			get { return textBoxInput.Text; }
		}
	}
}
