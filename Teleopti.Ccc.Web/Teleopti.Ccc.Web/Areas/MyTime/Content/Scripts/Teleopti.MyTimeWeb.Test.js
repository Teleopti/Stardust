/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.min.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/jquery.qtip.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Test = (function ($) {
	var _settings = {};
	var _messages = [];
	var _displayEnabled = false;
    
	function _testMessage(message) {
		_messages.push(message);
		if (_displayEnabled)
		    _displayMessage(message);
    }

	function _getTestMessages() {
	    var messages = "";
	    _displayEnable();
		for (var i = 0; i < _messages.length; i++) {
			messages = messages + _messages[i];
		}
		return messages;
	}

    function _displayEnable() {
        if (!_displayEnabled) {
            _displayEnabled = true;
            for (var i = 0; i < _messages.length; i++) {
				_displayMessage(_messages[i]);
            }
        }
    }
    
    function _displayMessage(message) {
        $('#page').append(message).append($('<br>'));
    }
    
	function _expireMyCookie(message) {
		$.ajax({
			url: _settings.startBaseUrl + 'Test/ExpireMyCookie',
			global: false,
			cache: false,
			async: false,
			success: function () {
			    $.ajax({
			        url: _settings.startBaseUrl + 'Menu/Applications',
			        global: false,
			        cache: false,
			        async: false,
			        success: function () { },
			        error: function (r) {
			            if (r.status == 401) //401 = not logged in
			                _testMessage(message);
			        }
			    });
			}
		});
	}

	return {
		Init: function (settings) {
		    _settings = settings;
		},
		TestMessage: function (message) {
		    _testMessage(message);
		},
		GetTestMessages: function () {
		    return _getTestMessages();
		},
		ExpireMyCookie: function (message) {
			_expireMyCookie(message);
		}
	};

})(jQuery);
