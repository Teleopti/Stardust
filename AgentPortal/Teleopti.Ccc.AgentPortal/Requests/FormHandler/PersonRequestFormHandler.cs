using System;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortal.Requests.FormHandler
{
    public class PersonRequestFormHandler
    {
        private readonly IWin32Window _parentForm;

        public PersonRequestFormHandler(IWin32Window parentForm)
        {
            _parentForm = parentForm;
        }

        public void ShowRequestScreen(PersonRequestDto personRequest)
        {
            IRequestView view = Create(personRequest, _parentForm);
            view.ShowRequestScreen();
        }

        public IRequestView Create(PersonRequestDto dto, IWin32Window parentForm)
        {
            if (dto.Request is ShiftTradeRequestDto)
            {
                return new ShiftTradeRequestViewer(dto, parentForm);
            }

            if (dto.Request is AbsenceRequestDto)
            {
                return new AbsenceRequestViewer(dto, parentForm);
            }

            if (dto.Request is TextRequestDto)
            {
                return new TextRequestViewer(dto, parentForm);
            }
            throw new MissingViewerException("No form exists for this type " + dto.Request.GetType());
        }


    }

    [Serializable]
    public class MissingViewerException : Exception
    {
        public MissingViewerException(string message):base(message){}
        protected MissingViewerException(SerializationInfo info, StreamingContext context):base(info,context){}
    }
}
