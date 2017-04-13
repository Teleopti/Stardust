using System.Runtime.InteropServices;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    class ClipboardOperationsForText : IClipboardOperationsForText
    {
        public void Copy(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                //sometimes it fails for some (external) reason
                try
                {
                    Clipboard.SetDataObject(text, true, 10, 10);
                }
                catch (ExternalException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                                
            }
        }
    }
}
