using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common
{
    public interface IHelpForm
    {
        string HelpId { get; }
	    IHelpContext FindMatchingManualHelpContext(Control control);
    }
}