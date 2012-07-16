/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>

Teleopti.MyTimeWeb.Request.RequestDetail = (function ($) {

	var requestViewModel = null;

	function _initToolbarButtons() {
		$('#Requests-addTextRequest-button')
			.click(function () {
				Teleopti.MyTimeWeb.Request.List.DisconnectAll();
				_clearFormData();
				requestViewModel.TextRequestTabVisible(true);
				requestViewModel.AbsenceRequestTabVisible(true);
				_hideEditSection();
				_showEditSection();
				$('#Text-request-tab').click();
			})
			.removeAttr('disabled')
			;
	}

	function _initEditSection() {
		_initControls();
		_initLabels();
	}

	function _clearValidationError() {
		$('#Request-detail-error').html('');
	}

	function _selectAbsenceRequestTab() {
		$('#Absence-type-element').show();
		$('#Absence-request-tab').addClass("selected-tab");
		$('#Text-request-tab').removeClass("selected-tab");
	}

	function _selectTextRequestTab() {
		$('#Absence-type-element').hide();
		$('#Text-request-tab').addClass("selected-tab");
		$('#Absence-request-tab').removeClass("selected-tab");
	}

	function _initControls() {
		requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		ko.applyBindings(requestViewModel, ko.dataFor($('#Request-detail-section')));

		$('#Request-detail-section .date-input')
			.datepicker()
			;
		$("#Request-detail-section .combobox.time-input")
			.combobox()
			;
		$("#Request-detail-section .combobox.absence-input").combobox();
		$("#Absence-type-input").prop('readonly', true);

		$('#Request-detail-ok-button')
			.click(function () {
				$(this).prop('disabled', true);
				if ($('#Text-request-tab.selected-tab').length > 0) {
					_addRequest("Requests/TextRequest");
				} else {
					_addRequest("Requests/AbsenceRequest");
				}
			});

		$('#Text-request-tab')
			.click(function () {
				_clearValidationError();
				if (!$('#Text-request-tab').hasClass('selected-tab')) {
					_selectTextRequestTab();
					requestViewModel.IsFullDay(false);
				}
			});
		$('#Absence-request-tab')
			.click(function () {
				_clearValidationError();
				if (!$('#Absence-request-tab').hasClass('selected-tab')) {
					_selectAbsenceRequestTab();
					requestViewModel.IsFullDay(true);
				}
			});
	}

	function _addRequest(requestUrl) {
		var formData = _getFormData();
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: requestUrl,
			dataType: "json",
			contentType: 'application/json; charset=utf-8',
			type: "POST",
			data: JSON.stringify(formData),
			success: function (data, textStatus, jqXHR) {
				Teleopti.MyTimeWeb.Request.List.RemoveItem(data);
				_fadeEditSection();
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

	function _showRequestTypeTab(requestType) {
		if (requestType == 0) {
			requestViewModel.TextRequestTabVisible(true);
			_selectTextRequestTab();
			requestViewModel.AbsenceRequestTabVisible(false);
		} else if (requestType == 1) {
			requestViewModel.TextRequestTabVisible(false);
			requestViewModel.AbsenceRequestTabVisible(true);
			_selectAbsenceRequestTab();
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
		var topPosition = $('#Requests-list').position().top - 1;
		if (!position)
			position = topPosition;
		if (position < topPosition)
			position = topPosition;
		$('#Request-detail-section')
			.css({
				'top': position
			})
			.fadeIn()
			;
	}

	function _SetOkButtonValue() {
		if ($('#Request-detail-entityid').val() == '') {
			$('#Request-detail-ok-button').val($('#Request-detail-ok-button').attr('new-value'));
		}
		else {
			$('#Request-detail-ok-button').val($('#Request-detail-ok-button').attr('update-value'));
		}
	}

	function _hideEditSection() {
		$('#Request-detail-section')
			.hide()
			;
	}

	function _fadeEditSection() {
		$('#Request-detail-section')
			.fadeOut()
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
				StartDate: $('#Request-detail-fromDate-input').val(),
				StartTime: $('#Request-detail-fromTime-input-input').val(),
				EndDate: $('#Request-detail-toDate-input').val(),
				EndTime: $('#Request-detail-toTime-input-input').val()
			},
			Message: $('#Request-detail-message-input').val(),
			EntityId: $('#Request-detail-entityid').val()
		};
	}

	function _fillFormData(data) {
		$('#Request-detail-subject-input').val(data.Subject);
		$('#Request-detail-fromDate-input').val(data.RawDateFrom);
		$('#Request-detail-fromTime-input-input').val(data.RawTimeFrom);
		$('#Request-detail-toDate-input').val(data.RawDateTo),
		$('#Request-detail-toTime-input-input').val(data.RawTimeTo);
		$('#Request-detail-message-input').val(data.Text);
		$('#Request-detail-entityid').val(data.Id);
		$('#Request-detail-subject-input').change();
		$('#Request-detail-message-input').change();
		requestViewModel.IsFullDay(data.IsFullDay);
	};

	function _fillFormRequestType(payload) {
		$('#Absence-type').combobox("set", payload);
	};

	function _clearFormData() {
		$('#Request-detail-section input, #Request-detail-section textarea, #Request-detail-section select')
			.not(':button, :submit, :reset')
			.reset()
			;
		$('#Absence-type').prop('selectedIndex', 0);
		requestViewModel.IsFullDay(false);
		_clearValidationError();
		_enableDetailSecion();
		_enableTimeinput();
	}

	function _enableTimeinput() {
		$('#Request-detail-fromTime button, #Request-detail-fromTime-input-input, #Request-detail-toTime button, #Request-detail-toTime-input-input')
			.removeAttr("disabled");
		$('#Request-detail-fromTime-input-input').css("color", "black");
		$('#Request-detail-toTime-input-input').css("color", "black");
	}

	return {
		Init: function () {
			_initToolbarButtons();
			_initEditSection();
		},
		HideEditSection: function () {
			_hideEditSection();
		},
		FadeEditSection: function () {
			_fadeEditSection();
		},
		ShowRequest: function (data, position) {
			_showRequest(data, position);
		},
		EnableTimeinput: function () {
			_enableTimeinput();
		}
	};

})(jQuery);

Teleopti.MyTimeWeb.Request.RequestViewModel = (function RequestViewModel() {
	var self = this;

	this.TextRequestTabVisible = ko.observable(true);
	this.AbsenceRequestTabVisible = ko.observable(true);
	this.TabSeparatorVisible = ko.computed(function () {
		return self.TextRequestTabVisible() && self.AbsenceRequestTabVisible();
	});
	this.IsFullDay = ko.observable(false);

	ko.computed(function () {
		if (self.IsFullDay()) {
			$('#Request-detail-fromTime-input-input').val($('#Request-detail-default-start-time').text());
			$('#Request-detail-toTime-input-input').val($('#Request-detail-default-end-time').text());
			_disableTimeinput();
		} else {
			$('#Request-detail-fromTime-input-input').reset();
			$('#Request-detail-toTime-input-input').reset();
			Teleopti.MyTimeWeb.Request.RequestDetail.EnableTimeinput();
		}

	});

	function _disableTimeinput() {
		$('#Request-detail-fromTime button, #Request-detail-fromTime-input-input, #Request-detail-toTime button, #Request-detail-toTime-input-input')
			.attr("disabled", "disabled");
		$('#Request-detail-fromTime-input-input').css("color", "grey");
		$('#Request-detail-toTime-input-input').css("color", "grey");
	}
});