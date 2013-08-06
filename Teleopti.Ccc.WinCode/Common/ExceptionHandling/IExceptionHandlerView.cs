using System.Drawing;
using System.Windows.Forms;

namespace Teleopti.Ccc.WinCode.Common.ExceptionHandling
{
    public interface IExceptionHandlerView
    {
        string InformationText { set; get; }
        void ButtonCloseApplicationText(string buttonText);
        void ButtonSendEmailText(string buttonText);
        void LinkCopyToClipboardText(string buttonText);
        void CheckBoxIncludeScreenshotText(string buttonText);
        void SetDialogResult(DialogResult value);
        void Close();
        void FormHeaderText(string headerText);
        void SetColors(Color color);
        bool IncludeScreenshot();
        Rectangle ScreenRectangle();
        void ScreenshotFromImage(Bitmap bitmap);
        void ShowMessageBox(string message);
    }
}
