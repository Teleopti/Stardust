using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Common.ExceptionHandling
{
    public class ExceptionHandlerPresenter
    {
        private readonly IExceptionHandlerView _view;
        private readonly ExceptionHandlerModel _model;

        public ExceptionHandlerPresenter(IExceptionHandlerView view, ExceptionHandlerModel model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            _view.InformationText = ExceptionHandlerModel.InformationText;
            _view.LinkCopyToClipboardText(ExceptionHandlerModel.LinkCopyToClipboardText);
            _view.ButtonCloseApplicationText(ExceptionHandlerModel.ButtonCloseApplicationText);
            _view.ButtonSendEmailText(ExceptionHandlerModel.ButtonSendEmailText);
            _view.CheckBoxIncludeScreenshotText(ExceptionHandlerModel.CheckBoxIncludeScreenshotText);
            _view.SetColors(ExceptionHandlerModel.FormColor);
            _view.FormHeaderText(ExceptionHandlerModel.FormHeaderText);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void CopyToClipboard()
        {
            try
            {
                string stringToCopy = _model.CompleteStackAndAssemblyText();
                System.Windows.Forms.Clipboard.SetDataObject(stringToCopy, true);
            }
            catch (Exception)
            {
                MessageBox.Show("Clipboard error", "Could not copy to clipboard", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        public void PopEmail()
        {
            _model.MapiMessage.Files.Clear();
            string filePath = Path.GetTempPath();
            var paths = new ArrayList();
            if (_view.IncludeScreenshot())
                paths.Add(CreateScreenshot(filePath));
            filePath = string.Concat(filePath, "TeleopiErrorMessage.txt");
            WriteToFile(filePath);
            paths.Add(filePath);

            _model.MapiMessage.Body = ExceptionHandlerModel.EmailBody;
            _model.MapiMessage.Subject = _model.Exception.Message;
            _model.MapiMessage.CreateMessage(_model.DefaultEmail, paths);
        }

        public void WriteToFile(string path)
        {
            try
            {
                _model.FileWriter.Save(path, _model.CompleteStackAndAssemblyText());
            }
            catch (UnauthorizedAccessException exception)
            {
                string message = ExceptionHandlerModel.CannotWriteToFileMessage(exception.Message);
                _view.ShowMessageBox(message);
            }
        }

        public void Close()
        {
            _view.Close();
            _view.SetDialogResult(DialogResult.OK);
        }

	    public string CreateScreenshot(string filePath)
	    {
		    var bmpScreenshot = new Bitmap(_view.ScreenRectangle().Width, _view.ScreenRectangle().Height,
			    PixelFormat.Format32bppArgb);

		    _view.ScreenshotFromImage(bmpScreenshot);
		    var screenshotPath = string.Concat(filePath, "TeleoptiScreenshot", ".png");
		    bmpScreenshot.Save(screenshotPath, ImageFormat.Png);
		    return screenshotPath;
	    }
    }
}
