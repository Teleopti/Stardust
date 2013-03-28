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

	function _addTextRequestClick() {
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeWindow();
		requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		_clearFormData();
		_initEditSection(requestViewModel);
		_hideEditSection();
		_showEditSection();

		requestViewModel.AddTextRequest();
	}

	function _addAbsenceRequestClick() {
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeWindow();
		requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		_clearFormData();
		_initEditSection(requestViewModel);
		_hideEditSection();
		_showEditSection();

		requestViewModel.AddAbsenceRequest();
	}

	function _initTemporary() {
		_initEditSection(requestViewModel);
	}

	function _initEditSection(requestDetailViewModel) {
		_initControls(requestDetailViewModel);
		_initLabels();
	}

	function _initControls(requestDetailViewModel) {
		requestViewModel = requestDetailViewModel;
		if ($('#Request-detail-section').length == 0)
			return;
		ko.applyBindings(requestViewModel, $('#Request-detail-section')[0]);

		$("#Request-detail-section .combobox.time-input")
			.combobox()
			;
		$("#Request-detail-section .combobox.absence-input").combobox();
		$("#Absence-type-input").prop('readonly', true);

		$('#Request-detail-ok-button')
			.click(function () {
				$(this).prop('disabled', true);
				if (requestViewModel.TypeEnum() == 0) {
					_addRequest("Requests/TextRequest");
				}
				else {
					_addRequest("Requests/AbsenceRequest");
				}
			});
	}

	function _addRequest(requestUrl) {
		var formData = _getFormData();
		ajax.Ajax({
			url: requestUrl,
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "POST",
			data: JSON.stringify(formData),
			success: function (data, textStatus, jqXHR) {
				_fadeEditSection(null);
				Teleopti.MyTimeWeb.Request.List.AddItemAtTop(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				if (jqXHR.status == 400) {
					var data = $.parseJSON(jqXHR.responseText);
					_displayValidationError(data);
					$('#Request-detail-ok-button').prop('disabled', false);
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function _initLabels() {
		$('#Request-detail-section input[type=text], #Request-detail-section textarea')
			.labeledinput()
			;
	}

	function _showRequest(data, position) {

		_hideEditSection();
		_clearFormData();
		_showRequestTypeTab(data.TypeEnum);
		if (data.TypeEnum == 1) {
			_fillFormRequestType(data.Payload);
		}
		_enableDisableDetailSection(data);
		_fillFormData(data);
		_showEditSection(position);
	}

	function _setRequest(data) {
		if (data.TypeEnum == 2) {

			var vm = new Teleopti.MyTimeWeb.Request.ShiftTradeRequestDetailViewModel(ajax);
			requestViewModel = vm;
			vm.Initialize(data);
			vm.loadSwapDetails();

		}
		else {
			requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel();
			requestViewModel.IsUpdate(true);
			requestViewModel.TypeEnum(data.TypeEnum);
		}
	}

	function _showRequestTypeTab(requestType) {
		if (requestType == 0) {
			requestViewModel.TextRequestHeaderVisible(true);
			requestViewModel.AbsenceRequestHeaderVisible(false);
		} else if (requestType == 1) {
			requestViewModel.TextRequestHeaderVisible(false);
			requestViewModel.AbsenceRequestHeaderVisible(true);
		}
	}

	function _enableDisableDetailSection(data) {
		if (data.Link.Methods.indexOf("PUT") == -1) {
			$('#Request-detail-section input').prop('disabled', true);
			$('#Request-detail-section button').prop('disabled', true);
			$('#Request-detail-section textarea').prop('readonly', true);
			$('#Request-detail-section textarea').css('color', 'gray');
			$('#Request-detail-ok-button').hide();
		} else {
			_enableDetailSecion();
		}
	}

	function _enableDetailSecion() {
		$('#Request-detail-section input').prop('disabled', false);
		$('#Request-detail-section button').prop('disabled', false);
		$('#Request-detail-section textarea').prop('readonly', false);
		$('#Request-detail-section textarea').css('color', 'black');
		$('#Request-detail-ok-button').show();
	}

	function _showEditSection(position) {
		_SetOkButtonValue();
		if (!position) {
			position = '15px';
		}
		$('#Request-detail-section')
			.css({
				'top': position
			})
			.fadeIn()
			;
	}

	function _SetOkButtonValue() {
		if ($('#Request-detail-entityid').val() == '') {
			$('#Request-detail-ok-button').text($('#Request-detail-ok-button').attr('new-value'));
		}
		else {
			$('#Request-detail-ok-button').text($('#Request-detail-ok-button').attr('update-value'));
		}
	}

	function _hideEditSection() {
		$('#Request-detail-section')
			.hide()
			;
	}

	function _fadeEditSection(func) {
		$('#Request-detail-section')
			.fadeOut(400, func)
			;
	}

	function _displayValidationError(data) {
		var message = data.Errors.join('</br>');
		$('#Request-detail-error').html(message || '');
	}

	function _getFormData() {
		var absenceId = $('#Absence-type').children(":selected").attr('typeid');
		if (absenceId == undefined) {
			absenceId = null;
		}

		return {
			Subject: $('#Request-detail-subject-input').val(),
			AbsenceId: absenceId,
			Period: {
				StartDate: requestViewModel.DateFrom().format('YYYY-MM-DD'),
				StartTime: $('#Request-detail-fromTime-input-input').val(),
				EndDate: requestViewModel.DateTo().format('YYYY-MM-DD'),
				EndTime: $('#Request-detail-toTime-input-input').val()
			},
			Message: $('#Request-detail-message-input').val(),
			EntityId: $('#Request-detail-entityid').val()
		};
	}

	function _fillFormData(data) {
		$('#Request-detail-subject-input').val(data.Subject);
		requestViewModel.DateTo(moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth)));
		$('#Request-detail-fromTime-input-input').val(data.RawTimeFrom);
		requestViewModel.DateFrom(moment(new Date(data.DateToYear, data.DateToMonth - 1, data.DateToDayOfMonth)));
		$('#Request-detail-toTime-input-input').val(data.RawTimeTo);
		$('#Request-detail-message-input').val(data.Text);
		$('#Request-detail-entityid').val(data.Id);
		$('#Request-detail-subject-input').change();
		$('#Request-detail-message-input').change();
		$('#Request-detail-deny-reason').text(data.DenyReason);
		requestViewModel.IsFullDay(data.IsFullDay);
	};

	function _fillFormRequestType(payload) {
		$('#Absence-type')
			.combobox({
				value: payload
			});
	};

	function _clearFormData() {
		$('#Request-detail-section input, #Request-detail-section textarea, #Request-detail-section select')
			.not(':button, :submit, :reset')
			.reset()
			;
		$('#Absence-type').prop('selectedIndex', 0);
		requestViewModel.IsFullDay(false);
		$('#Request-detail-deny-reason').text('');
		$('#Request-detail-error').html('');
		_enableDetailSecion();
		_enableTimeinput();
	}

	function _enableTimeinput() {
		$('#Request-detail-fromTime button, #Request-detail-fromTime-input-input, #Request-detail-toTime button, #Request-detail-toTime-input-input')
			.removeAttr("disabled");
		$('#Request-detail-fromTime-input-input').css("color", "");
		$('#Request-detail-toTime-input-input').css("color", "");
	}

	return {
		Init: function () {
			_initEditSection(new Teleopti.MyTimeWeb.Request.RequestViewModel());
		},
		HideEditSection: function () {
			_hideEditSection();
		},
		FadeEditSection: function (func) {
			_fadeEditSection(func);
		},
		ShowRequest: function (data, position) {
			_setRequest(data);
			_initTemporary();
			_showRequest(data, position);
		},
		EnableTimeinput: function () {
			_enableTimeinput();
		},
		AddTextRequestClick: function () {
			_addTextRequestClick();
		},
		AddAbsenceRequestClick: function () {
			_addAbsenceRequestClick();
		}
	};
})(jQuery);


Teleopti.MyTimeWeb.Request.RequestViewModel = function RequestViewModel() {

	var self = this;
	self.Templates = ["text-request-detail-template", "absence-request-detail-template", "shifttrade-request-detail-template"];
	self.TextRequestHeaderVisible = ko.observable(false);
	self.AbsenceRequestHeaderVisible = ko.observable(false);
	self.IsFullDay = ko.observable(false);
	self.IsUpdate = ko.observable(false);
    self.DateFrom = ko.observable(moment().startOf('day'));
    self.DateTo = ko.observable(moment().startOf('day'));
    self.TimeFrom = ko.observable($('#Request-detail-default-start-time').text());
    self.TimeTo = ko.observable($('#Request-detail-default-end-time').text());
    self.ShowMeridian = ($('div[data-culture-show-meridian]').attr('data-culture-show-meridian') == 'true');
    self.IsTimeInputEnabled = ko.observable(!self.IsFullDay());
    self.TypeEnum = ko.observable(0);
	self.Template = ko.computed(function () {
		return self.IsUpdate() ? self.Templates[self.TypeEnum()] : "add-new-request-detail-template";
	});

	self.IsFullDay.subscribe(function (newValue) {
	    if (newValue) {
	        $('#Request-detail-fromTime-input-input').val($('#Request-detail-default-fullday-start-time').text());
	        $('#Request-detail-toTime-input-input').val($('#Request-detail-default-fullday-end-time').text());
	        _disableTimeinput($('#Request-detail-default-fullday-end-time').text());
	        
	        self.TimeFrom($('#Request-detail-default-fullday-start-time').text());
	        self.TimeTo($('#Request-detail-default-fullday-end-time').text());
	        self.IsTimeInputEnabled(false);
	    } else {
			$('#Request-detail-fromTime-input-input').reset();
			$('#Request-detail-toTime-input-input').reset();
			Teleopti.MyTimeWeb.Request.RequestDetail.EnableTimeinput();
	        
			self.TimeFrom($('#Request-detail-default-start-time').text());
			self.TimeTo($('#Request-detail-default-end-time').text());
			self.IsTimeInputEnabled(true);
		}
	});

	function _disableTimeinput() {
		$('#Request-detail-fromTime button, #Request-detail-fromTime-input-input, #Request-detail-toTime button, #Request-detail-toTime-input-input')
			.attr("disabled", "disabled");
		$('#Request-detail-fromTime-input-input').css("color", "grey");
		$('#Request-detail-toTime-input-input').css("color", "grey");
	}
    
    function _setDefaultDates() {
        var year = $('#Request-detail-today-year').val();
        var month = $('#Request-detail-today-month').val();
        var day = $('#Request-detail-today-day').val();
        self.DateFrom(moment(new Date(year, month - 1, day)));
        self.DateTo(moment(new Date(year, month - 1, day)));
    }
	
	self.AddTextRequest = function () {
		self._clearValidationError();
		self.TypeEnum(0);
		self.TextRequestHeaderVisible(true);
		self.AbsenceRequestHeaderVisible(false);
		self.IsFullDay(false);
	    _setDefaultDates();
	};

	self.AddAbsenceRequest = function () {
		self._clearValidationError();
		self.TypeEnum(1);
		self.TextRequestHeaderVisible(false);
		self.AbsenceRequestHeaderVisible(true);
		self.IsFullDay(true);
	    _setDefaultDates();
	};

	self._clearValidationError = function () {
		$('#Request-detail-deny-reason').text('');
		$('#Request-detail-error').html('');
	};
};