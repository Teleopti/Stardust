using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common
{
    class ClipboardOperationsForText : IClipboardOperationsForText
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
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

        public void Cut(string text)
        {
            throw new NotImplementedException();
        }

        public string Paste()
        {
            throw new NotImplementedException();
        }
    }
}
