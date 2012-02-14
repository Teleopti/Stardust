using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Requests.FormHandler
{
    public class TextRequestViewer : IRequestView
    {
        private readonly PersonRequestDto _personRequestDto;
        private readonly IWin32Window _parentForm;

        public TextRequestViewer(PersonRequestDto personRequestDto, IWin32Window parentForm)
        {
            _personRequestDto = personRequestDto;
            _parentForm = parentForm;
        }

        public void ShowRequestScreen()
        {
            var requestScreen = new TextRequestView(_personRequestDto, SdkServiceHelper.SchedulingService);
            requestScreen.StartPosition = FormStartPosition.CenterScreen;
            requestScreen.ShowDialog(_parentForm);
        }
    }
}