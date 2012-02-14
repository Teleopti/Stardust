using System.Windows.Forms;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Requests.FormHandler
{
    public class AbsenceRequestViewer : IRequestView
    {
        private readonly PersonRequestDto _personRequestDto;
        private readonly IWin32Window _parentForm;

        public AbsenceRequestViewer(PersonRequestDto personRequestDto, IWin32Window parentForm)
        {
            _personRequestDto = personRequestDto;
            _parentForm = parentForm;
        }

        public void ShowRequestScreen()
        {
            AbsenceRequestView absenceRequestView = new AbsenceRequestView(_personRequestDto);
            absenceRequestView.StartPosition = FormStartPosition.CenterScreen;
            absenceRequestView.ShowDialog(_parentForm);
        }
    }
}