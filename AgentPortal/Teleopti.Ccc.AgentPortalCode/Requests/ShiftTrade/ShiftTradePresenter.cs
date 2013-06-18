using System;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade
{
    public sealed class ShiftTradePresenter
    {
        private readonly IShiftTradeView _view;
        private readonly ShiftTradeModel _model;
        private readonly PersonDto _loggedOnPerson;

        public ShiftTradePresenter(IShiftTradeView view, ShiftTradeModel model)
        {
            _view = view;
            _model = model;
            _loggedOnPerson = _model.LoggedOnPerson;
        }

        public void Initialize()
        {
            _view.SetPanelAcceptDenyVisible(true);
            _view.Message = _model.Message;
            _view.Subject = _model.Subject;
            _view.SetLabelName(_model.LabelName);
            _view.SetPersonName(_model.PersonName);
            _view.SetReasonMessageVisibility(false);
            _view.SetInitialDate(_model.InitialDate);
            drawStatus(_model.ShiftTradeStatus);
        }

        private void checkMainButtonSet(bool updateState)
        {
            if(updateState)
            {
                _view.SetOkButtonVisible(false);
                _view.DisableMessage();
            }
        }

        public void Accept()
        {
            Save();
            try
            {
                _model.PersonRequestDto = _model.SdkService.AcceptShiftTradeRequest(_model.PersonRequestDto);
            }
            catch(FaultException soapException)
            {
                _view.ShowErrorMessage(
                    string.Format(CultureInfo.CurrentUICulture,
                                  UserTexts.Resources.ShiftTradeRequestCouldNotBeAcceptedParameter0,
                                  soapException.Message), UserTexts.Resources.SaveError);
            }
            if (_model.PersonRequestDto.IsDeleted)
            {
                drawStatus(_model.ShiftTradeStatus);
            }
            else
            {
                _view.Close();
                _view.SetDialogResult(DialogResult.Yes);
            }

        }

        private void SetDeletedState()
        {
            _view.SetAcceptButtonEnabled(false);
            _view.SetDenyButtonEnabled(false);
            _view.SetDeleteButtonEnabled(false);
            _view.SetResponseTabEnabled(false);
            _view.SetOkButtonVisible(false);
            _view.ReasonMessage = UserTexts.Resources.RequestHasBeenDeleted;
            _view.SetReasonMessageVisibility(true);
        }

        public void Deny()
        {
            Save();
            _model.PersonRequestDto = _model.SdkService.DenyShiftTradeRequest(_model.PersonRequestDto);
            if (_model.PersonRequestDto.IsDeleted)
            {
                drawStatus(_model.ShiftTradeStatus);
            }
            else
            {
                _view.Close();
                _view.SetDialogResult(DialogResult.No);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade.IShiftTradeView.ShowErrorMessage(System.String,System.String)")]
		public bool Delete()
        {
        	try
        	{
				_model.SdkService.DeletePersonRequest(_model.PersonRequestDto);
        	}
        	catch (FaultException exception)
        	{
        		_view.ShowErrorMessage(
        			string.Concat(UserTexts.Resources.PleaseTryAgainLater, Environment.NewLine, Environment.NewLine,
        			              "Error information: ", exception.Message), UserTexts.Resources.AgentPortal);
        		return false;
        	}

            _view.SetDialogResult(DialogResult.Ignore);
        	return true;
        }

        /// <summary>
        /// Draws the status.
        /// </summary>
        /// <param name="shiftTradeStatusDto">The shift trade status dto.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2009-09-04
        /// </remarks>
        private void drawStatus(ShiftTradeStatusDto shiftTradeStatusDto)
        {
            if (_model.PersonRequestDto.IsDeleted)
            {
                SetDeletedState();
                return;
            }
            bool bothAreSamePerson = _loggedOnPerson.Id == _model.PersonRequestDto.Person.Id;
            bool requestStatusIsPending = _model.PersonRequestDto.RequestStatus == RequestStatusDto.Pending || (_model.PersonRequestDto.RequestStatus == RequestStatusDto.New && bothAreSamePerson);
            bool okToDelete = _model.PersonRequestDto.CanDelete && bothAreSamePerson && (_model.PersonRequestDto.RequestStatus != RequestStatusDto.New);
            checkMainButtonSet(!requestStatusIsPending);
            if (requestStatusIsPending)
            {
                switch (shiftTradeStatusDto)
                {
                    case ShiftTradeStatusDto.OkByMe:
                        if (bothAreSamePerson)
                        {
                            _view.SetAcceptButtonEnabled(false);
                            _view.SetDenyButtonEnabled(false);
                            _view.SetDeleteButtonEnabled(okToDelete);
                            _view.SetResponseTabEnabled(true);
                            _view.SetStatus(UserTexts.Resources.WaitingForOtherPart);
                        }
                        else
                        {
                            _view.SetAcceptButtonEnabled(true);
                            _view.SetDenyButtonEnabled(true);
                            _view.SetDeleteButtonEnabled(false);
                            _view.SetResponseTabEnabled(true);
                            _view.SetStatus(UserTexts.Resources.WaitingForYourApproval);
                        }
                        break;
                    case ShiftTradeStatusDto.Referred:
                        _view.ReasonMessage = UserTexts.Resources.TheScheduleHasChanged;
                        _view.SetReasonMessageVisibility(true);
                        _view.SetStatus(UserTexts.Resources.WaitingForReApproval);
                        if (bothAreSamePerson)
                        {
                            _view.SetAcceptButtonEnabled(true);
                            _view.SetDenyButtonEnabled(false);
                            _view.SetDeleteButtonEnabled(okToDelete);
                            _view.SetResponseTabEnabled(true);
                        }
                        else
                        {
                            _view.SetAcceptButtonEnabled(false);
                            _view.SetDenyButtonEnabled(false);
                            _view.SetDeleteButtonEnabled(false);
                            _view.SetResponseTabEnabled(false);
                        }
                        break;
                    case ShiftTradeStatusDto.OkByBothParts:
                        _view.SetStatus(UserTexts.Resources.WaitingForSupervisorApproval);
                        if (bothAreSamePerson)
                        {
                            _view.SetAcceptButtonEnabled(false);
                            _view.SetDenyButtonEnabled(false);
                            _view.SetDeleteButtonEnabled(okToDelete);
                            _view.SetResponseTabEnabled(true);
                        }
                        else
                        {
                            _view.SetAcceptButtonEnabled(false);
                            _view.SetDenyButtonEnabled(true);
                            _view.SetDeleteButtonEnabled(false);
                            _view.SetResponseTabEnabled(true);
                        }
                        break;
                } 
            }
            else
            {
                if (_model.PersonRequestDto.RequestStatus==RequestStatusDto.Approved)
                {
                    _view.SetStatus(UserTexts.Resources.GrantedBySupervisor);
                }
                else if (_model.PersonRequestDto.RequestStatus==RequestStatusDto.Denied)
                {
                    _view.SetStatus(UserTexts.Resources.Denied);
                    _view.ReasonMessage = LanguageResourceHelper.Translate(_model.PersonRequestDto.DenyReason);
                    _view.SetReasonMessageVisibility(true);
                }

                _view.SetAcceptButtonEnabled(false);
                _view.SetDenyButtonEnabled(false);
                _view.SetDeleteButtonEnabled(_model.PersonRequestDto.CanDelete);
                _view.SetResponseTabEnabled(false);
                _view.SetDeleteButtonEnabled(okToDelete);
            }
        }

        public void Cancel()
        {
            _view.Close();
            _view.SetDialogResult(DialogResult.Cancel);
        }
        //todo: working on the updateState problem
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "updateState")]
        public void Ok(bool updateState)
        {
			if (CheckForAbsences())
			{
				_view.ShowErrorMessage(UserTexts.Resources.ShiftTradeAbsenceDenyReason, UserTexts.Resources.ShiftTradeRequest);
				return;
			}
            if (SubjectIsEmpty())
            {
                _view.ShowErrorMessage(UserTexts.Resources.PersonRequestEmptySubjectError, UserTexts.Resources.ShiftTradeRequest);
                return;
            }

            Save();
            _view.Close();
            _view.SetDialogResult(DialogResult.OK);
        }

        private bool SubjectIsEmpty()
        {
            return string.IsNullOrEmpty(_view.Subject);
        }

        private bool CheckForAbsences()
		{
			foreach (var shifTradeDetail in _model.ShiftTradeDetailModels)
			{
				if (shifTradeDetail.VisualProjectionContainsAbsence)
					return true;
			}

			return false;
		}

    	private void Save()
        {
            _model.Subject = _view.Subject;
            _model.Message = _view.Message;
        }

        public void AddDateRange(DateOnlyDto startDate, DateOnlyDto endDate)
        {
            Save();
            _model.AddDateRange(startDate.DateTime,endDate.DateTime);
            var personReqeust = _model.SdkService.SetShiftTradeRequest(_model.PersonRequestDto, _model.Subject, _model.Message,
                                                   _model.SwapDetails.ToArray());
            UpdateShiftTradeDetails(personReqeust.Request as ShiftTradeRequestDto);
            _view.RefreshVisualView();
        }

        private void UpdateShiftTradeDetails(ShiftTradeRequestDto updatedShiftTradeRequest)
        {
            if (updatedShiftTradeRequest != null)
            {
                var shiftTradeRequest = _model.PersonRequestDto.Request as ShiftTradeRequestDto;
                if (shiftTradeRequest!=null)
                {
                    var updatedList = updatedShiftTradeRequest.ShiftTradeSwapDetails.ToList();
                    shiftTradeRequest.ShiftTradeSwapDetails.Clear();
                    foreach (var shiftTradeSwapDetailDto in updatedList)
                    {
                        shiftTradeRequest.ShiftTradeSwapDetails.Add(shiftTradeSwapDetailDto);
                    }
                    _model.CreateShiftTradeDetails();
                }
            }
        }

        public void DeleteSelectedDates()
        {
            Save();
            foreach (var date in _view.SelectedDates)
            {
                if (_model.SwapDetails.Count==1 &&
                    _model.FindSwapDetailByDate(date)!=null)
                {
                    _view.ShowErrorMessage(UserTexts.Resources.YouMustHaveAtLeastOneDateInShiftTradeRequest,UserTexts.Resources.DeleteDateParenthesisS);
                    break;
                }
                _model.DeleteDate(date);
            } 
            
            var personReqeust = _model.SdkService.SetShiftTradeRequest(_model.PersonRequestDto, _model.Subject, _model.Message,
                                                   _model.SwapDetails.ToArray());
            UpdateShiftTradeDetails(personReqeust.Request as ShiftTradeRequestDto);
            _view.RefreshVisualView();
        }
    }
}
