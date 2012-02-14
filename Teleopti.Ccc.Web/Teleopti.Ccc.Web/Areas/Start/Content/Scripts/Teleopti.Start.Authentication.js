/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />



if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.Start) === 'undefined') {
		Teleopti.Start = {};
	}
}

Teleopti.Start.Authentication = (function ($) {
	
    function _addSelListValToHiddenfield() {
        $('.select-list ul li').live("click", function () {
            $('.select-list ul li.active').removeClass('active');
            $(this).addClass('active');
            var hiddenInputVal = $(this).parents('.select-list').find('input:hidden');
            hiddenInputVal.val($(this).data('mytime-value'));
        });
    }

    return {
        Init: function () {
        	Teleopti.Start.Common.Layout.ActivateStdButtons();
        	Teleopti.Start.Authentication.Layout.ActivateSignInTabs();
        	Teleopti.Start.Authentication.Layout.SetInputfieldPlaceHolderText();
        	Teleopti.Start.Authentication.Layout.DisableSigninButtonOnSubmit();
        	_addSelListValToHiddenfield();
        },

        SignInPartialInit: function (ajaxContext) {
            Teleopti.Start.Common.Layout.ActivateStdButtons();
            Teleopti.Start.Authentication.Layout.SetSelectedListVal();
            Teleopti.Start.Authentication.Layout.SetInputfieldPlaceHolderText();
            if ($('#business_unit_partial_target').exists() || $('#error_partial_target').exists()) {
            	Teleopti.Start.Authentication.Layout.DisableTabs();
            }
        }
    };

})(jQuery);

Teleopti.Start.Authentication.Layout = (function ($) {
    return {
    	DisableTabs: function () {
    		$("#tabs-signin, #tabs").tabs({ disabled: [0, 1] });
    	},
    	ActivateSignInTabs: function () {
            $("#tabs-signin").tabs({
                show: function (event, ui) {
                    $(ui.panel).find('.select-list ul li[data-mytime-value=\'' + $(ui.panel).find('#SignIn_DataSourceName').val() + '\']').addClass('active');
                }
            });
        },
        // Fallback for html5-placeholder text
        SetInputfieldPlaceHolderText: function () {
            if (!Modernizr.input.placeholder && (!$('#usernameText').exists() && !$('#passwordText').exists())) {
                var passwordField = $('.passwordField');
                var usernameField = $('.usernameField');

                $(usernameField).after("<input type='text' id='usernameText' class='text-box floatleft ml10' value='" + $(usernameField).attr('placeholder') + "' />");
                $(usernameField).hide();

                $(passwordField).after("<input type='text' id='passwordText' class='text-box floatleft ml10' value='" + $(passwordField).attr('placeholder') + "' />");
                $(passwordField).hide();

                $('#usernameText').focus(function () {
                    $(this).hide();
                    $(usernameField).show();
                    $(usernameField).focus();
                });
                $(usernameField).blur(function () {
                    if ($(this).val() === '') {
                        $('#usernameText').show();
                        $(this).hide();
                    }
                });

                $('#passwordText').focus(function () {
                    $(this).hide();
                    $(passwordField).show();
                    $(passwordField).focus();
                });
                $(passwordField).blur(function () {
                    if ($(this).val() === '') {
                        $('#passwordText').show();
                        $(this).hide();
                    }
                });
            }
        },
        //Disable Signin button OnBegin   
        DisableSigninButtonOnSubmit: function () {
            if (!Modernizr.textshadow) {
                $('#ApplicationOkButton').bind('click', function () {
                    if ($(this).parents('form').valid()) {
                        $(this).button("option", "disabled", true);
                        $(this).parents('form').submit();
                        $(this).button("option", "disabled", true);
                    }
                });
            }
            else {
                $("form").bind("submit", function () {
                    if ($(this).valid()) {
                        $(":submit", this).button("option", "disabled", true);
                    }
                });
            }
        },
        //Sets the selected listval when signin-form is reloaded.
        SetSelectedListVal: function () {
            if ($('#ApplicationPartialTarget').exists() || $('#windows_partial_target').exists()) {
                $('.ui-tabs-panel:visible').find('.select-list ul li[data-mytime-value=\'' + $('.ui-tabs-panel:visible').find('#SignIn_DataSourceName').val() + '\']').addClass('active');
            }
            if ($('#business_unit_partial_target').exists()) {
                $('.ui-tabs-panel:visible').find('.select-list ul li[data-mytime-value=\'' + $('.ui-tabs-panel:visible').find('#SignIn_BusinessUnitId').val() + '\']').addClass('active');
            }
        }
    };
})(jQuery);