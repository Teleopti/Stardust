﻿using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Requests
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
			_view.Subject = _model.Subject;
			_view.Message = _model.Message;
			_view.LabelName = _model.Name;
			_view.Status = _model.StatusText;
            _personRequestAuthorization = new PersonRequestAuthorization(TeleoptiPrincipal.Current.PrincipalAuthorization);
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
