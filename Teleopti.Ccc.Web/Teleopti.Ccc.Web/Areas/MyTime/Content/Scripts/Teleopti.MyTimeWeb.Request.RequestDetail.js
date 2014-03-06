/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestViewModel.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel.js"/>

Teleopti.MyTimeWeb.Request.RequestDetail = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var parentViewModel = null;
    var defaultDateTimes = null;
	var weekStart = 3;
	Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
		weekStart = data.WeekStart;
	});

    var RequestDetailParentViewModel = function() {
        var self = this;
        self.requestViewModel = ko.observable();

        self.createRequestViewModel = function () {
        	var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel(_addRequest, weekStart, defaultDateTimes);
        	//ajax.Ajax({
        	//	url: 'Requests/Absences',
        	//	dataType: "json",
        	//	type: 'GET',
        	//	data: {
        	//		date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
        	//	},
        	//	success: function (data) {
        	//		vm.readAbsences(data);
        	//	}
        	//});
            return vm;
        };
        
        self.createShiftTradeRequestViewModel = function () {
            self.requestViewModel(new Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel(ajax));
        };

        self.CancelAddingNewRequest = function () {
            self.requestViewModel(null);
            Teleopti.MyTimeWeb.Request.ResetToolbarActiveButtons();
        };
    };

    function _addItemAtTop(data) {
        Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
        parentViewModel.CancelAddingNewRequest();
	}
    
    function _datePickerFormat() {
        return $('#Request-detail-datepicker-format').val().toUpperCase();
    }

    function _prepareForAddingRequest() {
        Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeWindow();
        var model = parentViewModel.createRequestViewModel();
        model.AddRequestCallback = _addItemAtTop;
        model.DateFormat(_datePickerFormat());
        parentViewModel.requestViewModel(model);
    }

	function _addTextRequestClick() {
		_prepareForAddingRequest();
		parentViewModel.requestViewModel().AddTextRequest(true);
	}

	function _addAbsenceRequestClick() {
	    _prepareForAddingRequest();
	    parentViewModel.requestViewModel().AddAbsenceRequest(true);
	}

	function _databindModel(requestDetailViewModel) {
	    parentViewModel = requestDetailViewModel;
	    var element = $('#Request-add-data-binding-area')[0];
	    if (element) {
	        ko.cleanNode(element);
	        ko.applyBindings(parentViewModel, element);
	    }
	}

	function _addRequest(model, successCallback) {
		var formData = _getFormData(model);
		ajax.Ajax({
			url: formData.Url,
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "POST",
			data: JSON.stringify(formData),
			success: function (data, textStatus, jqXHR) {
			    model.IsNewInProgress(false);
			    if (successCallback != undefined)
			        successCallback(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					model.ErrorMessage(data.Errors.join('</br>'));
					model.ShowError(true);
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function _showRequest(data) {
		_enableDisableDetailSection(data);
		_fillFormData(data);
	}

	function _setRequest(data) {
		if (data.TypeEnum == 2) {

		    parentViewModel.createShiftTradeRequestViewModel();
		    parentViewModel.requestViewModel().Initialize(data);
		    parentViewModel.requestViewModel().loadSwapDetails();

		}
		else {
		    var model = parentViewModel.createRequestViewModel();
			model.IsUpdate(true);
			model.TypeEnum(data.TypeEnum);
			model.DateFormat(_datePickerFormat());
		    parentViewModel.requestViewModel(model);
		}
	}

	function _enableDisableDetailSection(data) {
		if (data.Link.Methods.indexOf("PUT") == -1) {
		    parentViewModel.requestViewModel().IsEditable(false);
		} else {
		    parentViewModel.requestViewModel().IsEditable(true);
		}
	}
    
	function _getFormData(model) {
	    var absenceId = model.AbsenceId();
		if (absenceId == undefined) {
			absenceId = null;
		}
		
		return {
		    Url: model.TypeEnum() == 0 ? "Requests/TextRequest" : "Requests/AbsenceRequest",
		    Subject: model.Subject(),
			AbsenceId: absenceId,
			Period: {
			    StartDate: model.DateFrom().format('YYYY-MM-DD'),
			    StartTime: model.TimeFrom(),
			    EndDate: model.DateTo().format('YYYY-MM-DD'),
			    EndTime: model.TimeTo()
			},
			Message: model.Message(),
			EntityId: model.EntityId()
		};
	}

	function _fillFormData(data) {
	    parentViewModel.requestViewModel().Subject(data.Subject);
	    parentViewModel.requestViewModel().Message(data.Text);
	    parentViewModel.requestViewModel().DateFrom(moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth)));
	    parentViewModel.requestViewModel().TimeFrom(data.RawTimeFrom);
	    parentViewModel.requestViewModel().DateTo(moment(new Date(data.DateToYear, data.DateToMonth - 1, data.DateToDayOfMonth)));
	    parentViewModel.requestViewModel().TimeTo(data.RawTimeTo);
	    parentViewModel.requestViewModel().EntityId(data.Id);
	    parentViewModel.requestViewModel().AbsenceId(data.PayloadId);
	    parentViewModel.requestViewModel().DenyReason(data.DenyReason);
	    parentViewModel.requestViewModel().IsFullDay(data.IsFullDay);
	};
    
	function _prepareForViewModel(object) {
	    defaultDateTimes = object;
	}

	return {
	    Init: function() {
	        parentViewModel = new RequestDetailParentViewModel();
	        _databindModel(parentViewModel);
	    },
		ShowRequest: function (data) {
			_setRequest(data);
			_showRequest(data);
			var detailModel = parentViewModel.requestViewModel();
		    parentViewModel.requestViewModel(null);
		    return detailModel;
		},
		AddTextRequestClick: function () {
			_addTextRequestClick();
		},
		AddAbsenceRequestClick: function () {
			_addAbsenceRequestClick();
		},
		HideNewTextOrAbsenceRequestView: function() {
		    parentViewModel.requestViewModel(null);
		},
		AddTextOrAbsenceRequest: function (model, successCallback) {
		    _addRequest(model, successCallback);
		},
		PrepareForViewModel: function(object) {
		    _prepareForViewModel(object);
		}
	};
})(jQuery);