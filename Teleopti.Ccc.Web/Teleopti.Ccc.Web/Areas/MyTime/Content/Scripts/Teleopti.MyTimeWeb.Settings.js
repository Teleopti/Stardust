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

        self.avoidReload = false;
        self.cultures = ko.observableArray();
        self.selectedUiCulture = ko.observable();
        self.selectedCulture = ko.observable();

        self.selectedUiCulture.subscribe(function(newValue) {
            if (!self.avoidReload)
                _selectorChanged(newValue, "Settings/UpdateUiCulture");
        });
        
        self.selectedCulture.subscribe(function (newValue) {
            if (!self.avoidReload)
                _selectorChanged(newValue, "Settings/UpdateCulture");
        });
    };

    function _init() {
		Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Settings/Index', Teleopti.MyTimeWeb.Settings.PartialInit);
	}

    function _bindData() {
        ko.applyBindings(vm, $('#page')[0]);
    };

    function _partialInit() {
        vm = new settingsViewModel();
        _loadCultures();
        _bindData();
	}

	function _loadCultures() {
	    ajax.Ajax({
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
	        },
	        error: function(e) {
	            //console.log(e);
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

	return {
		Init: function () {
			_init();
		},
		PartialInit: function (readyForInteraction, completelyLoaded) {

		    $('#Test-Picker').select2();
		    _partialInit();
		    readyForInteraction();
		    completelyLoaded();
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
			    _updatePassword($("input#oldPassword").val(), $("input#password").val());
			});
    }

    function _passwordEvents() {
        $("input#password, input#passwordValidation").keyup(function () {
            var incorrectLabel = $("#nonMatchingPassword");
            var passwordButton = $("#passwordButton");
            var pw = $("input#password").val();
            var pw2 = $("input#passwordValidation").val();
            if (pw != pw2) {
                incorrectLabel.show();
                passwordButton.attr('disabled', 'disabled');
            } else {
                incorrectLabel.hide();
                passwordButton.removeAttr('disabled');
            }
        });

        $("input#oldPassword").keyup(function () {
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
                $("#incorrectOldPassword").hide();
                $("#invalidNewPassword").hide();
                $("#settings input").reset();
                setTimeout(function () { updatedLabel.hide(); }, 2000);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (jqXHR.status == 400) {
                    var error = $.parseJSON(jqXHR.responseText);
                    if (error.IsAuthenticationSuccessful) {
                        $("#invalidNewPassword").show();
                    } else {
                        $("#incorrectOldPassword").show();
                    }
                    return;
                }
                Teleopti.MyTimeWeb.Common.AjaxFailed(jqXHR, null, textStatus);
            }
        });
    }

    function _activatePlaceHolderText() {
        $(':text, :password').placeholder();
    }

    return {
        Init: function () {
            _init();
        },
        PartialInit: function (readyForInteraction, completelyLoaded) {
            _partialInit();
            readyForInteraction();
            completelyLoaded();
            _activatePlaceHolderText();
        }
    };
})(jQuery);