using System.Windows.Forms;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    public interface IHelpForm
    {
        string HelpId { get; }
	    IHelpContext FindMatchingManualHelpContext(Control control);
    }
}