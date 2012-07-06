﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.ExceptionHandling;

namespace Teleopti.Ccc.Win.ExceptionHandling
{
    public partial class ExceptionHandlerView : Form, IExceptionHandlerView, ILocalized
    {
        private ExceptionHandlerPresenter _exceptionHandlerPresenter;

        public ExceptionHandlerView(ExceptionHandlerModel exceptionHandlerModel):this()
        {
            _exceptionHandlerPresenter = new ExceptionHandlerPresenter(this, exceptionHandlerModel);
            _exceptionHandlerPresenter.Initialize();
        }

        public ExceptionHandlerView()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
        }

        public string InformationText
        {
            get { return labelInformationText.Text; }
            set { labelInformationText.Text = value; }
        }

        public void ButtonCloseApplicationText(string buttonText)
        {
            buttonCloseApplication.Text = buttonText;
        }

        public void LinkCopyToClipboardText(string buttonText)
        {
             linkLabelCopy.Text = buttonText;
        }

        public void CheckBoxIncludeScreenshotText(string buttonText) 
        {
            checkBoxIncludeScreenShot.Text = buttonText;
        }

        public void ButtonSendEmailText(string buttonText)
        {
            buttonPopEmail.Text = buttonText;
        }

        public void SetDialogResult(DialogResult value)
        {
            DialogResult = value;
        }

        public void FormHeaderText(string headerText) 
        {
            Text = headerText;
        }

        public void SetColors(Color color)
        {
            DialogResult = DialogResult.None;
            BackColor = color;
            buttonPopEmail.BackColor = color;
            buttonCloseApplication.BackColor = color;
            labelInformationText.BackColor = color;
        }

        public bool IncludeScreenshot()
        {
            return checkBoxIncludeScreenShot.Checked;
        }

        public Rectangle ScreenRectangle()
        {
            return Screen.PrimaryScreen.Bounds;
        }

        public void ScreenshotFromImage(Bitmap bitmap)
        {
            Graphics screenshot = Graphics.FromImage(bitmap);
            screenshot.CopyFromScreen(ScreenRectangle().X, ScreenRectangle().Y, 0, 0, ScreenRectangle().Size, CopyPixelOperation.SourceCopy);
        }

        public void ShowMessageBox(string message) 
        {
            MessageBoxAdv.Show(message, UserTexts.Resources.ErrorMessage, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                    (Rtl == RightToLeft.Yes) ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0);
        }

        public RightToLeft Rtl
        {
            get { return RightToLeft; }
        }

        private void buttonPopEmail_Click(object sender, EventArgs e)
        {
            _exceptionHandlerPresenter.PopEmail();
        }

        private void buttonCloseApplication_Click(object sender, EventArgs e)
        {
            _exceptionHandlerPresenter.Close();
        }

        private void linkLabelCopy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            _exceptionHandlerPresenter.CopyToClipboard();
        }

        public void SetTexts() { new LanguageResourceHelper().SetTexts(this); }
    }
}
