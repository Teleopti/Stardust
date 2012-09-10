﻿/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}


Teleopti.MyTimeWeb.Portal = (function ($) {
	var _settings = {};
	var _partialViewInitCallback = {};
	var _partialViewDisposeCallback = {};
	var currentViewId = null;
	var tabs = null;
	var currentFixedDate = null;

	function _layout() {
		Teleopti.MyTimeWeb.Common.Layout.ActivateTabs();
		Teleopti.MyTimeWeb.Common.Layout.ActivateStdButtons();
		Teleopti.MyTimeWeb.Common.Layout.ActivateCustomInput();
		Teleopti.MyTimeWeb.Portal.Layout.ActivateToolbarButtons();
		Teleopti.MyTimeWeb.Portal.Layout.ActivateDateButtons();
		Teleopti.MyTimeWeb.Portal.Layout.ActivateHorizontalScroll();
		Teleopti.MyTimeWeb.Portal.Layout.ActivateSettingsMenu();
	}

	function _registerPartialCallback(viewId, callBack, disposeCallback) {
		_partialViewInitCallback[viewId] = callBack;
		_partialViewDisposeCallback[viewId] = disposeCallback;
	}


	//disable navigation controls on ajax-begin
	function _disablePortalControls() {
		var dp = $('.datepicker');
		dp.css("background-position", "right 0");
		dp.datepicker("destroy");


		$('.ui-buttonset').buttonset("option", "disabled", true);
		$('nav .ui-button').button("option", "disabled", true);

		$('.selected-date-period').css({ opacity: 0.5 });
	}

	//enable navigation controls on ajax-complete
	function _enablePortalControls(periodRangeSelectorId) {
		if (periodRangeSelectorId.substring(0, 1) !== "#") {
			periodRangeSelectorId = "#" + periodRangeSelectorId;
		}

		$(periodRangeSelectorId + ' .datepicker').css("background-position", "right -120px");
		$('.ui-buttonset').buttonset("option", "disabled", false);
		$('.ui-button').button("option", "disabled", false);
		$('.selected-date-period').css({ opacity: 1 });
	}

	function _datePickerPartsToFixedDate(year, month, day) {
		return jQuery.map([parseInt(year), 1 + parseInt(month), parseInt(day)], function (val) { return (val < 10 ? '0' : '') + val.toString(); }).join('-');
	}

	function _attachAjaxEvents() {
		$('#loading')
			.hide()  // hide it initially
			.ajaxStart(function () {
				var bodyInner = $('#body-inner');
				$(this)
					.css({
						'width': $(bodyInner).width(),
						'height': $(bodyInner).height() + 10
					})
					.show()
					;
				$('img', this)
					.css({
						'top': 50 + $(window).scrollTop()
					});
				$('.toolbar-inner button.icon[disabled!="disabled"]')
					.attr('disabled', 'disabled')
					.addClass('ajax-disabled')
					;
			})
			.ajaxStop(function () {
				$(this).hide();
				$('.toolbar-inner button.icon.ajax-disabled')
					.removeAttr('disabled')
					.removeClass('ajax-disabled')
					;
			});
	}

	function _initNavigation() {

		tabs = $('#tabs')
			.tabberiet({
				click: function () {
					_navigateTo($(this).data('mytime-action'));
				},
				emptyContentSelector: '#EmptyTab'
			})
			;

		if (location.hash.length <= 1) {
			location.hash = '#' + _settings.defaultNavigation;
		} else {
			$(window).trigger('hashchange');
		}

		$('#asm-button').click(function (ev) {
			window.open(_settings.baseUrl + 'Asm', 'AsmWindow', 'width=850,height=100;channelmode=1,directories=0,left=0,location=0,menubar=0,resizable=0,scrollbars=0,status=0,titlebar=0,toolbar=0,top=0');
			ev.preventDefault();
			return false;
		});
	}

	// Bind an event to window.onhashchange that, when the history state changes,
	// iterates over all tab widgets, changing the current tab as necessary.
	$(window)
		.bind('hashchange', function (e) {
			var hashInfo = _parseHash();
			_adjustTabs(hashInfo);
			_loadContent(hashInfo);
		});

	function _navigateTo(action, date, id) {

		//needed due to stopPropagation in tabberiet
		// ^^^ replace with a callback?
		Teleopti.MyTimeWeb.Portal.Layout.HideSettingsMenu();
		_invokeDisposeCallback(currentViewId);

		var hash = action;
		if (date) {
			if (Teleopti.MyTimeWeb.Common.IsFixedDate(date)) {
				date = Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date);
			}
			hash += date;
		}
		if (id) {
			hash += "/" + id;
		}
		_pushHash(hash);
	}

	function _pushHash(hash) {
		// this will trigger the hashchange event, which we listen for
		location.hash = hash;
	}

	function _parseHash() {
		var hash = location.hash || '';
		if (hash.length > 0) { hash = hash.substring(1); }
		var parts = $.merge(hash.split('/'), [null, null, null, null, null, null, null, null]);
		parts.length = 8;

		var controller = parts[0];
		var action = parts[1];
		var actionHash = controller + "/" + action;

		var dateHash = '';
		var dateMatch = hash.match(/\d{4}\/\d{2}\/\d{2}/);
		if (dateMatch)
			dateHash = dateMatch[0];

		return {
			hash: hash,
			parts: parts,
			controller: controller,
			action: action,
			actionHash: actionHash,
			dateHash: dateHash
		};
	}

	function _adjustTabs(hashInfo) {

		var tabId = '#' + hashInfo.controller + 'Tab';
		tabs.tabberiet('selectById', tabId);

		// initializes action for next/previous buttons in the period picker.
		// should be set from whoever initializes the period picker instead.
		var toolbar = $(tabId);
		toolbar
			.find('.date-range-selector')
			.data('mytime-action', hashInfo.actionHash)
			;
	}

	function _loadContent(hashInfo) {
		_disablePortalControls();

		Teleopti.MyTimeWeb.Ajax.AjaxAbortAll();
		Teleopti.MyTimeWeb.Ajax.Ajax({
			url: hashInfo.hash,
			global: true,
			success: function (html) {
				var viewId = hashInfo.actionHash; //gröt
				$('#body-inner').html(html);
				_invokeInitCallback(viewId);
				currentViewId = viewId;
			}
		});
	}

	function _invokeDisposeCallback(viewId) {
		var partialDispose = _partialViewDisposeCallback[viewId];
		if ($.isFunction(partialDispose))
			partialDispose();
	}

	function _invokeInitCallback(viewId) {
		var partialInit = _partialViewInitCallback[viewId];
		if ($.isFunction(partialInit))
			partialInit();
	}

	return {
		Init: function (settings) {
			Teleopti.MyTimeWeb.Ajax.Init(settings);
			Teleopti.MyTimeWeb.Common.Init(settings);
			Teleopti.MyTimeWeb.Test.Init(settings);
			_settings = settings;
			_layout();
			_attachAjaxEvents();
			_initNavigation();
		},

		NavigateTo: function (action, date, id) {
			_navigateTo(action, date, id);
		},
		ParseHash: function () {
			return _parseHash();
		},
		RegisterPartialCallBack: function (viewId, callBack, disposeCallback) {
			_registerPartialCallback(viewId, callBack, disposeCallback);
		},
		CurrentFixedDate: function () {
			return currentFixedDate;
		},
		InitPeriodSelection: function (rangeSelectorId, periodData, actionSuffix) {
			var common = Teleopti.MyTimeWeb.Common;
			var range = $(rangeSelectorId);
			var actionPrefix = range.data('mytime-action');
			actionSuffix = actionSuffix || '';

			currentFixedDate = null;
			if (typeof (periodData) !== 'undefined') {
				currentFixedDate = periodData.Date;

				var dp = range.find('.datepicker').datepicker({
					minDate: common.ParseToDate(periodData.SelectableDateRange.MinDate),
					maxDate: common.ParseToDate(periodData.SelectableDateRange.MaxDate),
					defaultDate: common.ParseToDate(periodData.Date),
					showAnim: "slideDown",
					onSelect: function (dateText, inst) {
						var fixedDate = _datePickerPartsToFixedDate(inst.selectedYear, inst.selectedMonth, inst.selectedDay);
						var urlDate = Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(fixedDate);
						var hash = actionPrefix + urlDate + actionSuffix;
						_navigateTo(hash);
					}
				});

				range.find('.datepicker')
					.datepicker('setDate', common.ParseToDate(periodData.Date));

				var nextPeriod = periodData.PeriodNavigation.NextPeriod;
				var prevPeriod = periodData.PeriodNavigation.PrevPeriod;
				var urlNextPeriod = common.FixedDateToPartsUrl(nextPeriod);
				var urlPrevPeriod = common.FixedDateToPartsUrl(prevPeriod);

				range.find('span').html(periodData.Display);
				range.find('button:first')
					.unbind('click')
					.click(function () {
						_navigateTo(actionPrefix + urlPrevPeriod + actionSuffix);
					})
					;
				range.find('button:last')
					.unbind('click')
					.click(function () {
						_navigateTo(actionPrefix + urlNextPeriod + actionSuffix);
					})
					;

				_enablePortalControls(rangeSelectorId);
			}

		}
	};
})(jQuery);

Teleopti.MyTimeWeb.Portal.Layout = (function ($) {

    function _hideSettingsMenu() {
        $(".dropdown dd ul").hide();
    }
    return {
        // Activating buttons in toolbar
        ActivateToolbarButtons: function () {
            $(".buttonset-nav").buttonset();
        },

        // Activate date buttons
        ActivateDateButtons: function () {
            $(".date-range-selector").each(function () {
                var t = $(this);
                t.find("button:first").button({
                    icons: {
                        primary: "ui-icon-triangle-1-w"
                    },
                    text: false
                });
                t.find("button:last").button({
                    icons: {
                        primary: "ui-icon-triangle-1-e"
                    },
                    text: false
                });
            });
        },
        ActivateHorizontalScroll: function () {
            $(window).scroll(function () {
                $('header').css("left", -$(window).scrollLeft() + "px");
            });
        },
        HideSettingsMenu: function () {
            _hideSettingsMenu();
        },
        ActivateSettingsMenu: function () {
            $(".dropdown dt span").live("click", function () {
                $(".dropdown dd ul").toggle();
            });

            $(".dropdown dd ul").live("click", function () {
                _hideSettingsMenu();
            });


            $(document).bind('click', function (e) {
                var $clicked = $(e.target);
                if (!$clicked.parents().hasClass("dropdown"))
                    _hideSettingsMenu();
            });

            $(".dropdown a").hover(function () {
                $(this).addClass('ui-state-hover');
            }, function () {
                $(this).removeClass('ui-state-hover');
            });
        }
    };
})(jQuery);




 