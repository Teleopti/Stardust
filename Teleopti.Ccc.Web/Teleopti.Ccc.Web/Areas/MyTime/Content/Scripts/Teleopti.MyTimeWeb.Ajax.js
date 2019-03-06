Teleopti.MyTimeWeb.Ajax = function () {
	var _requests = [];
	var callWhenAllAjaxIsCompleted;

	function _ajax(options) {
		_setupUrl(options);
		_setupError(options);
		_setupCache(options);
		_setupGlobal(options);
		_setupHeaders(options);
		_setupRequestStack(options);

		return $.ajax(options);
	}

	function _setupUrl(options) {
		if (options.url.indexOf('://') !== -1)
			return;
		if (options.url.indexOf('/') === 0)
			return;
		options.url = Teleopti.MyTimeWeb.AjaxSettings.baseUrl + options.url;
	}

	function _setupError(options) {
		options.statusCode401 = options.statusCode401 || function () { window.location.href = Teleopti.MyTimeWeb.AjaxSettings.baseUrl; };
		options.statusCode403 = options.statusCode403 || function () { window.location.href = Teleopti.MyTimeWeb.AjaxSettings.baseUrl; };
		options.abort = options.abort || function () { };

		var errorCallback = options.error;

		options.error = function (jqXHR, textStatus, errorThrown) {
			if (options.abort && jqXHR && textStatus === "abort") {
				options.abort(jqXHR, textStatus, errorThrown);
				return;
			}
			if (options.statusCode400 && jqXHR && jqXHR.status === 400) {
				options.statusCode400(jqXHR, textStatus, errorThrown);
				return;
			}
			if (options.statusCode401 && jqXHR && jqXHR.status === 401) {
				options.statusCode401(jqXHR, textStatus, errorThrown);
				return;
			}
			if (options.statusCode403 && jqXHR && jqXHR.status === 403) {
				options.statusCode403(jqXHR, textStatus, errorThrown);
				return;
			}
			if (options.statusCode404 && jqXHR && jqXHR.status === 404) {
				options.statusCode404(jqXHR, textStatus, errorThrown);
				return;
			}
			if (errorCallback) {
				errorCallback(jqXHR, textStatus, errorThrown);
				return;
			}

			Teleopti.MyTimeWeb.Ajax.UI.ShowAjaxError(jqXHR, textStatus, errorThrown);
		};
	}

	function _setupCache(options) {
		options.cache = options.cache || false;
	}

	function _setupGlobal(options) {
		options.global = options.global || false;
	}

	function _setupHeaders(options) {
		if (options.headers) {
			options.headers['X-Use-GregorianCalendar'] = true;
		} else {
			options.headers = { 'X-Use-GregorianCalendar': true };
		}
	}

	function _setupRequestStack(options) {
		var beforeSendCallback = options.beforeSend;
		options.beforeSend = function (jqXHR) {
			_requests.push(jqXHR);
			if (beforeSendCallback)
				beforeSendCallback(jqXHR);
		};

		var completeCallback = options.complete;
		options.complete = function (jqXHR, textStatus) {
			var index = _requests.indexOf(jqXHR);
			if (index > -1)
				_requests.splice(index, 1);
			if (completeCallback)
				completeCallback(jqXHR, textStatus);
			_handleCallWhenAllAjaxCompleted();
		};
	}

	function _handleCallWhenAllAjaxCompleted() {
		if (callWhenAllAjaxIsCompleted && _requests.length === 0) {
			callWhenAllAjaxIsCompleted();
			callWhenAllAjaxIsCompleted = null; // prevent next call. good or bad?
		}
	}

	function _ajaxAbortAll() {
		$(_requests).each(function (idx, jqXHR) {
			jqXHR.abort();
		});
		_requests.length = 0;
	}

	return {
		Ajax: function (options) {
			return _ajax(options);
		},
		AbortAll: function () {
			_ajaxAbortAll();
		},
		CallWhenAllAjaxCompleted: function (callback) {
			callWhenAllAjaxIsCompleted = callback;
			_handleCallWhenAllAjaxCompleted();
		}
	};
};

Teleopti.MyTimeWeb.AjaxSettings = { baseUrl: '' };

Teleopti.MyTimeWeb.Ajax.UI = (function ($) {
	function _getErrorMessage(jqXHR) {
		var errorMessage = "An error has occurred, please try again.";
		if (jqXHR) {
			try {
				var response = JSON.parse(jqXHR.responseText);
				if (response.Message) {
					errorMessage = response.Message;
				}
			} catch (e) {
			}
		}
		return errorMessage;
	}

	function _ajaxErrorBody(jqXHR) {
		var htmlString = '<h4 style="text-align:center;">' + _getErrorMessage(jqXHR) + '</h4>';
		htmlString += '<p style="text-align:center;"><a href="javascript:window.location.reload()" class="btn btn-primary">Refresh</a></p>';
		$('#body-inner').html('<div id="error-panel">' + htmlString + '</div>');
	}

	function _ajaxErrorDialog(jqXHR) {
		$('#dialog-modal .modal-body').html(_getErrorMessage(jqXHR));
		$('#dialog-modal').modal("show");

	}

	function _logError(jqXHR, textStatus, errorThrown) {
		if (console && console.error)
			console.error("Method Failed: " + (jqXHR ? jqXHR.responseText : "") + textStatus + errorThrown);
	}

	return {
		ShowAjaxError: function (jqXHR, textStatus, errorThrown) {
			if (!!$('#body-inner').length) {
				_ajaxErrorBody(jqXHR);
			}
			else {
				_ajaxErrorDialog(jqXHR);
			}
			_logError(jqXHR, textStatus, errorThrown);
		},
		ShowErrorDialog: function (jqXHR, textStatus, errorThrown) {
			_ajaxErrorDialog(jqXHR);
			_logError(jqXHR, textStatus, errorThrown);
		}
	};
})(jQuery);