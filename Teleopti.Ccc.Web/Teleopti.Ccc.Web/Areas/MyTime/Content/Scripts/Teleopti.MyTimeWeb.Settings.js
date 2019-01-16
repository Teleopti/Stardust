Teleopti.MyTimeWeb.Settings = (function ($) {
    var ajax = new Teleopti.MyTimeWeb.Ajax();
    var vm;

    function _bindData() {
        ko.applyBindings(vm, $('#page')[0]);
    };
    
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
			vm = new Teleopti.MyTimeWeb.Settings.SettingsViewModel(ajax);
		    $.when(vm.loadSettings(), vm.getCalendarLinkStatus())
				.done(function () {
					vm.featureCheck();
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
				if (!data.IsSuccessful) {
					if (data.IsAuthenticationSuccessful) {
						$("#alertPassword").show();
						$("#invalidNewPassword").show();
					} else {
						$("#alertOldPassword").show();
						$("#incorrectOldPassword").show();
					}
					return;
				}

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