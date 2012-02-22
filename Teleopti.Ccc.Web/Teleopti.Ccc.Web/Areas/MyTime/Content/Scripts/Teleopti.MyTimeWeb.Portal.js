/// <reference path="~/Scripts/jquery-1.5.1.js" />
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
	var top_nav_selector = 'ul.ui-tabs-nav a';
	var _settings = {};
	var _partialViewInitCallback = {};

	function _layout() {
		Teleopti.MyTimeWeb.Common.Layout.ActivateTabs();
		Teleopti.MyTimeWeb.Common.Layout.ActivateStdButtons();
		Teleopti.MyTimeWeb.Common.Layout.ActivateCustomInput();
		Teleopti.MyTimeWeb.Portal.Layout.ActivateToolbarButtons();
		Teleopti.MyTimeWeb.Portal.Layout.ActivateDateButtons();
		Teleopti.MyTimeWeb.Portal.Layout.ActivateHorizontalScroll();
		Teleopti.MyTimeWeb.Portal.Layout.ActivateSettingsMenu();
	}

	function _registerPartialCallback(viewId, callBack) {
		_partialViewInitCallback[viewId] = callBack;
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

	function _checkLogin(htmlDom) {
		return (htmlDom.indexOf('<section id="page-signin">') > -1);
	}

	function _datePickerPartsToFixedDate(year, month, day) {
		return jQuery.map([parseInt(year), 1 + parseInt(month), parseInt(day)], function (val) { return (val < 10 ? '0' : '') + val.toString(); }).join('-');
	}

	function _attachAjaxEvents() {
		$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
			if (options.url.indexOf('http://') == -1)
				options.url = _settings.baseUrl + options.url;
		});
		$('#loading')
			.hide()  // hide it initially
			.ajaxStart(function () {
				var bodyInner = $('#body-inner');
				$(this).css({
					'width': $(bodyInner).width(),
					'height': $(bodyInner).height() + 10
				}).show();
				$('img', this).css({
					'top': 50 + $(window).scrollTop()
				});
				$('.toolbar-inner button.icon[disabled!="disabled"]').attr('disabled', 'disabled').addClass('ajax-disabled');
			})
			.ajaxStop(function () {
				$(this).hide();
				$('.toolbar-inner button.icon.ajax-disabled').removeAttr('disabled').removeClass('ajax-disabled');
			});
	}

	function _initNavigation() {

		topNavTabs = $('#tabs');

		// Enable tabs on all tab widgets. The `event` property must be overridden so
		// that the tabs aren't changed on click, and any custom event name can be
		// specified. Note that if you define a callback for the 'select' event, it
		// will be executed for the selected tab whenever the hash changes.
		topNavTabs.tabs({ event: 'change' });

		// Define our own click handler for the top nav.
		topNavTabs
			.find('[data-mytime-action]')
			.click(function () {
				_navigateTo($(this).data('mytime-action'));
			})
			;
		if (location.hash.length <= 1) {
			location.hash = '#' + _settings.defaultNavigation;
		} else {
			$(window).trigger('hashchange');
		}
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

		var navSectionHash = '#' + hashInfo.controller + 'Tab';
		var navSectionAction = hashInfo.actionHash;

		var tab = topNavTabs
			.find(top_nav_selector)
			.filter((function () { return this.hash == navSectionHash; }))
			;
		var navSection = $(navSectionHash);
		navSection
			.find('input[data-mytime-action]')
			.each(function (i, inp) {
				var b = $(inp);
				b.attr('checked', (b.data('mytime-action') == navSectionAction) ? 'checked' : '');
			})
			;
		navSection
			.find('.buttonset-nav')
			.buttonset("refresh")
			;

		navSection
			.find('.date-range-selector')
			.data('mytime-action', navSectionAction)
			;

		tab.triggerHandler('change');
	}

	function _loadContent(hashInfo) {
		var baseUrl = _settings.baseUrl;
		_disablePortalControls();
		$.ajax({
			url: hashInfo.hash,
			cache: false,
			success: function (html) {
				if (_checkLogin(html)) {
					location.href = baseUrl || '/';
				}

				$('#body-inner').html(html);
				var partialFn = _partialViewInitCallback[hashInfo.actionHash];
				if ($.isFunction(partialFn))
					partialFn();
			}
		}).error(function (e, status) {
			$('#body-inner').html('<h2>Error: ' + e.status + '</h2>');
			Teleopti.MyTimeWeb.Common.AjaxFailed(e, null, status);
		});
	}

	return {
		Init: function (settings) {
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
		RegisterPartialCallBack: function (viewId, callBack) {
			_registerPartialCallback(viewId, callBack);
		},
		InitPeriodSelection: function (rangeSelectorId, periodData, actionSuffix) {
			var common = Teleopti.MyTimeWeb.Common;
			var range = $(rangeSelectorId);
			var actionPrefix = range.data('mytime-action');
			actionSuffix = actionSuffix || '';
			if (typeof (periodData) !== 'undefined') {
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

				range.find('.datepicker').datepicker('setDate', common.ParseToDate(periodData.Date));

				var nextPeriod = periodData.PeriodNavigation.NextPeriod;
				var prevPeriod = periodData.PeriodNavigation.PrevPeriod;
				var urlNextPeriod = common.FixedDateToPartsUrl(nextPeriod);
				var urlPrevPeriod = common.FixedDateToPartsUrl(prevPeriod);

				range.find('span').html(periodData.Display);
				range.find('button:first').data('mytime-action', actionPrefix + urlPrevPeriod + actionSuffix);
				range.find('button:last').data('mytime-action', actionPrefix + urlNextPeriod + actionSuffix);

				_enablePortalControls(rangeSelectorId);
			}

		}
	};
})(jQuery);

Teleopti.MyTimeWeb.Portal.Layout = (function ($) {
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
		ActivateSettingsMenu: function () {
			$(".dropdown dt span").live("click", function () {
				$(".dropdown dd ul").toggle();
			});

			$(".dropdown dd ul").live("click", function () {
				$(".dropdown dd ul").hide();
			});


			$(document).bind('click', function(e) {
				var $clicked = $(e.target);
				if (!$clicked.parents().hasClass("dropdown"))
					$(".dropdown dd ul").hide();
			});
		}
	};
})(jQuery);