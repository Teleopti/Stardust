using System;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Requests.ShiftTrade;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortal.Requests.FormHandler
{
    public class ShiftTradeRequestViewer : IRequestView
    {
        private readonly PersonRequestDto _personRequestDto;
        private readonly IWin32Window _parentForm;

        public ShiftTradeRequestViewer(PersonRequestDto personRequestDto, IWin32Window parentForm)
        {
            _personRequestDto = personRequestDto;
            _parentForm = parentForm;
        }

        public void ShowRequestScreen()
        {
            PersonDto loggedOnPerson = StateHolder.Instance.StateReader.SessionScopeData.LoggedOnPerson;
            ShiftTradeModel model = new ShiftTradeModel(SdkServiceHelper.SchedulingService, _personRequestDto, loggedOnPerson, DateTime.Now);
            using (ShiftTradeView shiftTradeView = new ShiftTradeView(model, true))
            {
                var result = shiftTradeView.ShowDialog(_parentForm);
                // Returns DialogResult.Ignore if Delete was clicked, fix for 10152.
                if (result == DialogResult.OK)
                {
                    model.PersonRequestDto = SdkServiceHelper.SchedulingService.UpdatePersonRequestMessage(model.PersonRequestDto);
                }
            }
        }
    }
}