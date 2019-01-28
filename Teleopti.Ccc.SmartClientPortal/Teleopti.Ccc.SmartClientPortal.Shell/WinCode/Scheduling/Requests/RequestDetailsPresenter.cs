using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.Requests
{
	public sealed class RequestDetailsPresenter
	{
		private readonly IRequestDetailsView _view;
		private readonly IPersonRequestViewModel _model;
	    private PersonRequestAuthorization _personRequestAuthorization;

		public RequestDetailsPresenter(IRequestDetailsView view, IPersonRequestViewModel model)
        {
            _view = view;
            _model = model;
        }

		public void Initialize()
		{
			_view.Subject = _model.GetSubject(new NoFormatting());
			_view.Message = _model.GetMessage(new NoFormatting());
			_view.LabelName = _model.Name;
			_view.Status = _model.StatusText;
            _personRequestAuthorization = new PersonRequestAuthorization(PrincipalAuthorization.Current_DONTUSE());
		}

		public bool IsShiftTradeRequest()
		{
			return _model.PersonRequest.Request is IShiftTradeRequest;
		}

		public bool IsRequestEditable()
		{
			return _model.IsEditable 
                && _model.IsSelected 
                && _model.IsWithinSchedulePeriod 
                && _personRequestAuthorization.IsPermittedRequestApprove(_model.RequestType);
		}
	}
}
