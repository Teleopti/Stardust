/// <reference path="~/Content/Scripts/knockout-2.2.1.debug.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}


Teleopti.MyTimeWeb.Settings = (function ($) {
    var ajax = new Teleopti.MyTimeWeb.Ajax();
    var vm;

    var settingsViewModel = function() {
        var self = this;

	    self.culturesLoaded = ko.observable(false);
        self.avoidReload = false;
        self.cultures = ko.observableArray();
        self.selectedUiCulture = ko.observable();
        self.selectedCulture = ko.observable();

        self.CalendarSharingActive = ko.observable(false);
	    self.CalendarUrl = ko.observable();
	    self.ActivateCalendarSharing = function() {
	        _setCalendarLinkStatus(true);
	    };
	    self.DeactivateCalendarSharing = function() {
	    	_setCalendarLinkStatus(false);
	    };

	    self.IsNotificationSupported = ko.computed(function() {
		    if (window.webkitNotifications)
		    	return true;
		    return false;
	    });
	    self.RequestNotificationPermission = function () {
		    if (window.webkitNotifications && window.webkitNotifications.checkPermission() != 0) {
			    window.webkitNotifications.requestPermission();
		    }
	    };

        self.selectedUiCulture.subscribe(function(newValue) {
            if (!self.avoidReload)
                _selectorChanged(newValue, "Settings/UpdateUiCulture");
        });
        
        self.selectedCulture.subscribe(function (newValue) {
            if (!self.avoidReload)
                _selectorChanged(newValue, "Settings/UpdateCulture");
        });

    };

    function _bindData() {
        ko.applyBindings(vm, $('#page')[0]);
    };

	function _loadCultures() {
		return ajax.Ajax({
	        url: "Settings/Cultures",
	        dataType: "json",
	        type: "GET",
	        global: false,
	        cache: false,
	        success: function (data, textStatus, jqXHR) {
	        	vm.cultures(data.Cultures);
	            vm.avoidReload = true;
	            vm.selectedUiCulture(data.ChoosenUiCulture.id);
	            vm.selectedCulture(data.ChoosenCulture.id);
	            vm.avoidReload = false;
		        vm.culturesLoaded(true);
	        }
	    });
	}
    
	function _selectorChanged(value, url) {
		var data = { LCID: value };
		ajax.Ajax({
			url: url,
			contentType: 'application/json; charset=utf-8',
			type: "PUT",
			data: JSON.stringify(data),
			success: function (data, textStatus, jqXHR) {
				ajax.CallWhenAllAjaxCompleted(function () {
					location.reload();
				});
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}
	
	function _getCalendarLinkStatus() {
		if ($(".share-my-calendar").length == 0)
			return null;
		return ajax.Ajax({
			url: "Settings/CalendarLinkStatus",
			contentType: 'application/json; charset=utf-8',
			dataType: "json",
			type: "GET",
			success: function (data, textStatus, jqXHR) {
				vm.CalendarSharingActive(data.IsActive);
				vm.CalendarUrl(data.Url);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
			}
		});
	}
    
	function _setCalendarLinkStatus(isActive) {
	    ajax.Ajax({
	    	url: "Settings/SetCalendarLinkStatus",
	    	contentType: 'application/json; charset=utf-8',
	    	dataType: "json",
	        type: "POST",
	        data: JSON.stringify({IsActive: isActive}),
	        success: function (data, textStatus, jqXHR) {
	        	vm.CalendarSharingActive(data.IsActive);
	        	vm.CalendarUrl(data.Url);
	        },
	        error: function (jqXHR, textStatus, errorThrown) {
	            Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
	        }
	    });
	}
    
	function _cleanBindings() {
	    ko.cleanNode($('#page')[0]);
	    if (vm != null)
	        vm = null;
	}

	return {
	    Init: function () {
	        if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
	            Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Settings/Index', Teleopti.MyTimeWeb.Settings.PartialInit, Teleopti.MyTimeWeb.Settings.PartialDispose);
	        }
		},
		PartialInit: function (readyForInteraction, completelyLoaded) {
			$('#Test-Picker').select2();
		    vm = new settingsViewModel();
		    $.when(_loadCultures(), _getCalendarLinkStatus())
				.done(function () {
					readyForInteraction();
					completelyLoaded();
				});
		    _bindData();
		},
	    PartialDispose: function() {
	        _cleanBindings();
	    }
	};
})(jQuery);


Teleopti.MyTimeWeb.Password = (function ($) {
    var ajax = new Teleopti.MyTimeWeb.Ajax();

    function _init() {
        Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Settings/Password', Teleopti.MyTimeWeb.Password.PartialInit);
    }

    function _partialInit() {
        _passwordEvents();
        _initButton();
    }

    function _initButton() {
        $("#passwordButton")
			.click(function () {
				_updatePassword($("input#oldPassword[type=password]").val(), $("input#password[type=password]").val());
			});
    }

    function _passwordEvents() {
        $("input#password, input#passwordValidation").keyup(function () {
            var alertPassword = $("#alertPassword");
            var incorrectLabel = $("#nonMatchingPassword");
            var passwordButton = $("#passwordButton");
            var pw = $("input#password[type=password]").val();
            var pw2 = $("input#passwordValidation[type=password]").val();
            if (pw != pw2) {
                alertPassword.show();
                incorrectLabel.show();
                passwordButton.attr('disabled', 'disabled');
            } else {
                alertPassword.hide();
                incorrectLabel.hide();
                passwordButton.removeAttr('disabled');
            }
        });

        $("input#oldPassword").keyup(function () {
            $("#alertOldPassword").hide();
            $("#incorrectOldPassword").hide();
        });

        $("input#password").keyup(function () {
        	$("#invalidNewPassword").hide();
        });
        $("input#passwordValidation").keyup(function () {
        	$("#invalidNewPassword").hide();
        });
    }

    function _updatePassword(oldPassword, newPassword) {
    	var data = { OldPassword: oldPassword, NewPassword: newPassword };
        ajax.Ajax({
            url: "Settings/ChangePassword",
            dataType: "json",
            contentType: 'application/json; charset=utf-8',
            type: 'POST',
            data: JSON.stringify(data),
            success: function (data, textStatus, jqXHR) {
                var updatedLabel = $("label#updated");
                updatedLabel.show();
                $("#alertPassword").hide();
                $("#alertOldPassword").hide();
                $("#incorrectOldPassword").hide();
                $("#invalidNewPassword").hide();
                $("#settings input").reset();
                setTimeout(function () { updatedLabel.hide(); }, 2000);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status == 400) {
                	var error = $.parseJSON(jqXHR.responseText);
                	if (error.IsAuthenticationSuccessful) {
                        $("#alertPassword").show();
                        $("#invalidNewPassword").show();
                    } else {
                        $("#alertOldPassword").show();
                        $("#incorrectOldPassword").show();
                    }
                    return;
                }
                Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
            }
        });
    }

    return {
    	Init: function () {
            _init();
    	},
        PartialInit: function (readyForInteraction, completelyLoaded) {
            _partialInit();
            readyForInteraction();
            completelyLoaded();
            Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
        }
    };
})(jQuery);