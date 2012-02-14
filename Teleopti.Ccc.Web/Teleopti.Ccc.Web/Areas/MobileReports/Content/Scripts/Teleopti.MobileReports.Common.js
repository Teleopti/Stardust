

if (typeof(Teleopti) === 'undefined') {
	Teleopti = { };

	if (typeof(Teleopti.MobileReports) === 'undefined') {
		Teleopti.MobileReports = { };
	}
}

Teleopti.MobileReports.Common = (function($) {

	function _handleScriptResponse(jqXHR) {
		var contentType = jqXHR.getResponseHeader('Content-Type');
		if ((contentType) && (contentType.indexOf('application/x-javascript') !== -1)) {
			eval(jqXHR.responseText); // Error since we only accepts json.
		}
	}


	function _addError(viewId, errorMessage) {
		var errorTempl = $('#error-message');
		var errorContainer = errorTempl.clone().attr('id', 'error-instance');
		errorContainer.find('.error').html(errorMessage);
		errorContainer.insertBefore($('' + viewId + ' *:jqmData(role="listview")'));
		errorContainer.fadeIn();
		errorContainer.listview();
		errorContainer.listview('refresh');
	}

	function _clearError() {
		var errorContainer = $('#error-instance');
		if (errorContainer.length > 0 && typeof(errorContainer.listview) != 'undefined') {
			errorContainer.listview('destroy');
		}
		errorContainer.hide();
		errorContainer.remove();
	}

	function _datePartsToFixedDate(year, month, day) {
		return $.map([parseInt(year), 1 + parseInt(month), parseInt(day)], function(val) { return (val < 10 ? '0' : '') + val.toString(); }).join('-');
	}

	function _dateToFixedDate(date) {
		return _datePartsToFixedDate(date.getFullYear(), date.getMonth(), date.getDate());
	}

	function _fixedDateToDate(strDate) {

		if (!strDate.match( /^\d{4}-\d{2}-\d{2}$/ )) {
			return new Date();
		}
		return Date.parseExact(strDate, 'yyyy-MM-dd');
	}

	return {
		HandleScriptResponse: function(jqXHR) { _handleScriptResponse(jqXHR); },
		AddError: function(viewId, errorMessage) {
			_addError(viewId, errorMessage);
		},
		ClearError: function() {
			_clearError();
		},
		DatePartsToFixedDate: function(year, month, day) {
			return _datePartsToFixedDate(year, month, day);
		},
		DateToFixedDate: function(date) {
			return _dateToFixedDate(date);
		},
		FixedDateToDate: function(strDate) {
			return _fixedDateToDate(strDate);
		}
	};

})(jQuery);