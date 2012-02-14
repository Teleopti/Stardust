

if (typeof(Teleopti) === 'undefined') {
	Teleopti = { };

	if (typeof(Teleopti.Start) === 'undefined') {
		Teleopti.Start = { };
	}
	if (typeof(Teleopti.Start.Mobile) === 'undefined') {
		Teleopti.Start.Mobile = { };
	}
}

Teleopti.Start.Mobile.Common = (function($) {

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

	function _prefixModel(prefix, model) {
		var prefixedModel = { };
		$.each(model, function(key, val) {
			prefixedModel[prefix + key] = val;
		});
		return prefixedModel;
	}

	function _xhr(selector, url, model, okFn) {
		var ownerSelector = selector;
		var errFn = function(xhr, recv) {
			Teleopti.Start.Mobile.Common.HandleScriptResponse(xhr);
			if (xhr.status == 400 || (recv != null && typeof recv.responseText != 'undefined')) {
				var msg = 'Error: ' + xhr.responseText, data = $.parseJSON(xhr.responseText);
				if (data != null && typeof data.Errors != 'undefined') {
					msg = data.Errors.join('<br />');
				} else if (data != null && typeof data.Message != 'undefined') {
					msg = data.Message;
				}
				Teleopti.Start.Mobile.Common.AddError(ownerSelector, msg);
				return true;
			} else if (xhr.status == 500) {
				Teleopti.Start.Mobile.Common.AddError(ownerSelector, 'Server returned an error');
				return true;
			}
			return false;
		};
		var param = {
			url: url,
			dataType: 'json',
			contentType: 'application/json; charset=utf-8',
			type: 'POST',
			cache: false,
			data: JSON.stringify(_prefixModel('SignIn.', model)),
			beforeSend: function() {
				$(ownerSelector).find('.ui-btn:not(.ui-disabled)').each(function() {
					$(this).addClass('ui-disabled').addClass('by-me');
				});
				$.mobile.showPageLoadingMsg();
			},
			success: function(data, t, xhr) {
				if (errFn(xhr, data)) {
					return;
				}
				okFn(data);
			},
			error: function(xhr) {
				errFn(xhr, null);
			},
			complete: function() {
				$(ownerSelector).find('.ui-btn.by-me').each(function() {
					$(this).removeClass('ui-disabled').removeClass('by-me');
				});
				$.mobile.hidePageLoadingMsg();
			}
		};
		$.ajax(param);
	}

	return {
		HandleScriptResponse: function(jqXhr) { _handleScriptResponse(jqXhr); },
		AddError: function(viewId, errorMessage) {
			_addError(viewId, errorMessage);
		},
		ClearError: function() {
			_clearError();
		},
		XRequest: function(selector, url, model, okFn) {
			_xhr(selector, url, model, okFn);
		}
	};
})(jQuery);