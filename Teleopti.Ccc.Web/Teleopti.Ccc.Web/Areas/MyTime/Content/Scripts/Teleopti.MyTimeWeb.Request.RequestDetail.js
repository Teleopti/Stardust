﻿/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestViewModel.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.AbsenceRequestDetailViewModel.js"/>

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
        	ajax.Ajax({
        		url: 'Requests/PersonalAccountPermission',
        		dataType: "json",
        		type: 'GET',

        		success: function (data) {
        			vm.readPersonalAccountPermission(data);
        		}
        	});
            return vm;
        };
        
        self.createShiftTradeRequestViewModel = function (id) {
        	var shiftTradeRequestDetailViewModel = new Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel(ajax);
        	shiftTradeRequestDetailViewModel.loadIsEditMessageEnabled();
        	shiftTradeRequestDetailViewModel.setMiscSetting(id);
	        self.requestViewModel(shiftTradeRequestDetailViewModel);
        };

        self.addAbsenceRequestDetail = function (requestViewModel) {
        	Teleopti.MyTimeWeb.Request.AbsenceRequestDetailViewModel(requestViewModel, ajax);
        }

        self.createShiftExchangeOfferViewModel = function (data) {
        	var shiftExchangeOfferDetailViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(ajax, _addItemAtTop).Update(data);
	        shiftExchangeOfferDetailViewModel.DateFormat(_datePickerFormat());
	    	self.requestViewModel(shiftExchangeOfferDetailViewModel);
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
    	return Teleopti.MyTimeWeb.Common.DateFormat;
    }

	function _hideOthers() {
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeWindow();
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeBulletinBoard();
	}

    function _prepareForAddingRequest() {
	    _hideOthers();
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

	function _addPostShiftForTradeClick(date) {
		_hideOthers();
		var defaultTime = { defaultStartTime: defaultDateTimes.defaultStartTime, defaultEndTime: defaultDateTimes.defaultEndTime };
		var model = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(ajax, _addItemAtTop).Create(defaultTime);
		model.DateFormat(_datePickerFormat());
		parentViewModel.requestViewModel(model);

		var tomorrow = moment().startOf('day').add('days', 1);
		var requestDay = moment(date);
		if (requestDay.isBefore(tomorrow)) requestDay = tomorrow;
		model.DateFrom(requestDay);
		model.DateTo(requestDay);
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
				model.Subject("");
				model.Message("");
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

	function _setRequest(data) {
		
		if (data.TypeEnum == 2) {
		    parentViewModel.createShiftTradeRequestViewModel(data.Id);
		    parentViewModel.requestViewModel().Initialize(data);
		    parentViewModel.requestViewModel().loadSwapDetails();

		}
		else if (data.TypeEnum == 3) {
			parentViewModel.createShiftExchangeOfferViewModel(data);
		} else {
			var model = parentViewModel.createRequestViewModel();
			model.IsUpdate(true);
			model.DateFormat(_datePickerFormat());

			parentViewModel.requestViewModel(model);
			parentViewModel.requestViewModel().Initialize(data);

			if (data.TypeEnum == 1) {
				parentViewModel.addAbsenceRequestDetail(model);
			}


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
			    StartDate: Teleopti.MyTimeWeb.Common.FormatServiceDate(model.DateFrom()),
			    StartTime: model.TimeFrom(),
			    EndDate: Teleopti.MyTimeWeb.Common.FormatServiceDate(model.DateTo()),
			    EndTime: model.TimeTo()
			},
			Message: model.Message(),
			EntityId: model.EntityId()
		};
	}
    
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
			_enableDisableDetailSection(data);
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
		AddPostShiftForTradeClick: function (date) {
			_addPostShiftForTradeClick(date);
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