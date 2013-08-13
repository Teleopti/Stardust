/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>

Teleopti.MyTimeWeb.Request.RequestDetail = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var requestViewModel = null;
	var weekStart = 3;
	ajax.Ajax({
			url: 'UserInfo/Culture',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				weekStart = data.WeekStart;
			}
	});

	function _addTextRequestClick() {
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeWindow();
		requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel(_addRequest, weekStart);
		_initEditSection(requestViewModel);
		requestViewModel.AddTextRequest();
	}

	function _addAbsenceRequestClick() {
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeWindow();
		requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel(_addRequest, weekStart);
		_initEditSection(requestViewModel);
		requestViewModel.AddAbsenceRequest();
	}

	function _initTemporary() {
		_initEditSection(requestViewModel);
	}

	function _initEditSection(requestDetailViewModel) {
		_initControls(requestDetailViewModel);
	}

	function _initControls(requestDetailViewModel) {
	    requestViewModel = requestDetailViewModel;
	    var element = $('#Request-add-section')[0];
	    if (element) {
	        ko.cleanNode(element);
	        ko.applyBindings(requestViewModel, element);
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
			    Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
			    model.IsNewInProgress(false);

			    if (successCallback != undefined)
			        successCallback();
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

			var vm = new Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel(ajax);
			requestViewModel = vm;
			vm.Initialize(data);
			vm.loadSwapDetails();

		}
		else {
			requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel(_addRequest, weekStart);
			requestViewModel.IsUpdate(true);
			requestViewModel.TypeEnum(data.TypeEnum);
		}
	}

	function _enableDisableDetailSection(data) {
		if (data.Link.Methods.indexOf("PUT") == -1) {
		    requestViewModel.IsEditable(false);
		} else {
		    requestViewModel.IsEditable(true);
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
	    requestViewModel.Subject(data.Subject);
	    requestViewModel.Message(data.Text);
		requestViewModel.DateFrom(moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth)));
		requestViewModel.TimeFrom(data.RawTimeFrom);
		requestViewModel.DateTo(moment(new Date(data.DateToYear, data.DateToMonth - 1, data.DateToDayOfMonth)));
		requestViewModel.TimeTo(data.RawTimeTo);
		requestViewModel.EntityId(data.Id);
	    requestViewModel.AbsenceId(data.PayloadId);
		requestViewModel.DenyReason(data.DenyReason);
		requestViewModel.IsFullDay(data.IsFullDay);
	};

	return {
		Init: function () {
		    _initEditSection(new Teleopti.MyTimeWeb.Request.RequestViewModel(_addRequest));
		},
		ShowRequest: function (data) {
			_setRequest(data);
			_initTemporary();
			_showRequest(data);
		    return requestViewModel;
		},
		AddTextRequestClick: function () {
			_addTextRequestClick();
		},
		AddAbsenceRequestClick: function () {
			_addAbsenceRequestClick();
		},
		HideNewTextOrAbsenceRequestView: function() {
		    requestViewModel.IsNewInProgress(false);
		}
	};
})(jQuery);


Teleopti.MyTimeWeb.Request.RequestViewModel = function RequestViewModel(addRequestMethod,firstDayOfWeek) {
	var self = this;
	self.Templates = ["text-request-detail-template", "absence-request-detail-template", "shifttrade-request-detail-template"];
	self.TextRequestHeaderVisible = ko.observable(false);
	self.AbsenceRequestHeaderVisible = ko.observable(false);
	self.IsFullDay = ko.observable(false);
	self.IsUpdate = ko.observable(false);
    self.DateFrom = ko.observable(moment().startOf('day'));
    self.DateTo = ko.observable(moment().startOf('day'));
    self.TimeFromInternal = ko.observable($('#Request-detail-default-start-time').text());
    self.TimeToInternal = ko.observable($('#Request-detail-default-end-time').text());
    self.DateFormat = ko.observable($('#Request-detail-datepicker-format').val().toUpperCase());
    self.TimeFrom = ko.computed({
        read: function() {
            if (self.IsFullDay()) {
                return $('#Request-detail-default-fullday-start-time').text();
            }
            return self.TimeFromInternal();
        },
        write: function (value) {
            if (self.IsFullDay()) return;
            self.TimeFromInternal(value);
        }
    });
    self.TimeTo = ko.computed({
        read: function () {
            if (self.IsFullDay()) {
                return $('#Request-detail-default-fullday-end-time').text();
            }
            return self.TimeToInternal();
        },
        write: function (value) {
            if (self.IsFullDay()) return;
            self.TimeToInternal(value);
        }
    });
    self.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
    self.TypeEnum = ko.observable(0);
    self.ShowError = ko.observable(false);
    self.ErrorMessage = ko.observable('');
    self.AbsenceId = ko.observable();
    self.Subject = ko.observable();
    self.Message = ko.observable();
    self.EntityId = ko.observable();
    self.DenyReason = ko.observable();
    self.IsEditable = ko.observable(true);
    self.IsNewInProgress = ko.observable(false);
    self.weekStart = ko.observable(firstDayOfWeek); 
    self.IsTimeInputEnabled = ko.computed(function () {
        return !self.IsFullDay() && self.IsEditable();
    });

    self.Template = ko.computed(function () {
		return self.IsUpdate() ? self.Templates[self.TypeEnum()] : "add-new-request-detail-template";
	});

    self.AddRequestCallback = undefined;

    self.AddRequest = function() {
        addRequestMethod(self, self.AddRequestCallback);
    };
	
    function _setDefaultDates() {
        var year = $('#Request-detail-today-year').val();
        var month = $('#Request-detail-today-month').val();
        var day = $('#Request-detail-today-day').val();
        self.DateFrom(moment(new Date(year, month - 1, day)));
        self.DateTo(moment(new Date(year, month - 1, day)));
    }
	
    self.AddTextRequest = function () {
        self.IsNewInProgress(true);
		self.TypeEnum(0);
		self.TextRequestHeaderVisible(true);
		self.AbsenceRequestHeaderVisible(false);
		self.IsFullDay(false);
		_setDefaultDates();
    };

    self.AddAbsenceRequest = function () {
        self.IsNewInProgress(true);
		self.TypeEnum(1);
		self.TextRequestHeaderVisible(false);
		self.AbsenceRequestHeaderVisible(true);
		self.IsFullDay(true);
	    _setDefaultDates();
	};
};