/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>

Teleopti.MyTimeWeb.Request.TextRequest = (function ($) {

	function _initToolbarButtons() {
		$('#Requests-addTextRequest-button')
			.click(function () {
				Teleopti.MyTimeWeb.Request.List.DisconnectAll();
				_hideEditSection();
				_clearFormData();
				_showEditSection();
			})
			.removeAttr('disabled')
			;
	}

	function _initEditSection() {
		$('#Request-detail-ok-button')
			.click(function () {
				_addTextRequest();
			});
		_initControls();
		_initLabels();
	}

	function _initControls() {
		$('#Request-detail-section .date-input')
			.datepicker()
			;
		$("#Request-detail-section .combobox.time-input")
			.combobox()
			;
	}

	function _initLabels() {
		$('#Request-detail-section input[type=text], #Request-detail-section textarea')
			.labeledinput()
			;
	}

	function _showRequest(data, position) {
		_hideEditSection();
		_clearFormData();
		_fillFormData(data);
		_enableDisableDetailSection(data);
		_showEditSection(position);
	}

	function _enableDisableDetailSection(data) {
		if (data.Link.Methods.indexOf("PUT") == -1) {
			$('#Request-detail-section input').prop('disabled', true);
			$('#Request-detail-section textarea').prop('readonly', true);
			$('#Request-detail-section textarea').css('color', 'gray');
			$('#Request-detail-ok-button').hide();
		} else {
			_enableDetailSecion();
		}
	}

	function _enableDetailSecion() {
		$('#Request-detail-section input').prop('disabled', false);
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

	function _addTextRequest() {
		var formData = _getFormData();
		$.myTimeAjax({
			url: "Requests/TextRequest",
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
					return;
				}
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}

	function _displayValidationError(data) {
		var message = data.Errors.join(' ');
		$('#Request-detail-error').html(message || '');
	}

	function _getFormData() {
		return {
			Subject: $('#Request-detail-subject-input').val(),
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
		$('#Request-detail-subject-input').val(data.Subject),
		$('#Request-detail-fromDate-input').val(data.RawDateFrom),
		$('#Request-detail-fromTime-input-input').val(data.RawTimeFrom),
		$('#Request-detail-toDate-input').val(data.RawDateTo),
		$('#Request-detail-toTime-input-input').val(data.RawTimeTo),
		$('#Request-detail-message-input').val(data.Text);
		$('#Request-detail-entityid').val(data.Id);
		$('#Request-detail-subject-input').change();
		$('#Request-detail-message-input').change();
	};

	function _clearFormData() {
		$('#Request-detail-section input, #Request-detail-section textarea, #Request-detail-section select')
			.not(':button, :submit, :reset')
			.reset()
			;
		$('#Request-detail-error').html('');
		_enableDetailSecion();
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
		ShowRequest: function (url, position) {
			_showRequest(url, position);
		}
	};

})(jQuery);

