/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Crossroads/crossroads.js" />
/// <reference path="~/Content/hasher/hasher.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/knockout.bindings.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MyReport.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MyAdherence.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MyQueueMetrics.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Request.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Test.js" />

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
	var currentFixedDate = null;
	var _ajax;

	function _layout() {
		Teleopti.MyTimeWeb.Portal.Layout.ActivateHorizontalScroll();
	}

	function _registerPartialCallback(viewId, callBack, disposeCallback) {
		_partialViewInitCallback[viewId.toUpperCase()] = callBack;
		_partialViewDisposeCallback[viewId.toUpperCase()] = disposeCallback;
	}

	//disable navigation controls on ajax-begin
	function _disablePortalControls() {
		$('#Team-Picker').select2("enable", false);
		$('#body-inner').hide();
	}

	function _attachAjaxEvents() {
		$('#loading').hide();  // hide it initially

		$(document).ajaxStart(function () {
			var bodyInner = $('#body-inner');
			$('#loading')
				.css({
					'width': $(bodyInner).width(),
					'height': $(bodyInner).height() + 10
				})
				.show();
			$('img', $('#loading')[0])
				.css({
					'top': 50 + $(window).scrollTop()
				});
		});
		$(document).ajaxStop(function () {
			$('#loading').hide();
		});
	}

	function _showDayScheduleForStartPage() {
		return Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_DayScheduleForStartPage_43446");
	}

	function _initNavigation(window) {
		$('.dropdown-menu a[data-mytime-action]')
			.click(function (e) {
				e.preventDefault();
				_navigateTo($(this).data('mytime-action'));
			})
			;

		if (window.location.hash.length <= 1) {
			if (_showDayScheduleForStartPage() && (_isMobile(window))) {
				window.location.replace("#Schedule/MobileDay");
			} else {
				window.location.replace("#" + (_isMobile(window) ? "Schedule/MobileWeek" : _settings.defaultNavigation));
			}
		}
		console.log(window.location)
		var asmWindow;
		$('#asm-link').click(function (ev) {
			$(".dropdown#user-settings").removeClass("open");

			asmWindow = window.open(_settings.baseUrl + 'Asm', 'AsmWindow', 'width=435,height=100;channelmode=1,directories=0,left=0,location=0,menubar=0,resizable=1,scrollbars=0,status=0,titlebar=0,toolbar=0,top=0');

			if (asmWindow.focus) {
				asmWindow.focus();
			}

			if (!asmWindow.closed) {
				asmWindow.focus();
			}

			ev.preventDefault();

			return false;
		});
		$('#signout').click(function () {
			if (asmWindow != undefined && !asmWindow.closed) {
				asmWindow.close();
			}
		});
	}

	function _setupRoutes() {
		var viewRegex = '[a-z]+';
		var actionRegex = '[a-z]+';
		var dateRegex = '\\d{8}';
		var intOptionRegex = "\\d{1}";
		var yearsRegex = '\\d{4}';
		var monthRegex = '\\d{2}';
		var dayRegex = '\\d{2}';

		crossroads.addRoute(new RegExp('^(MyReport)/(' + actionRegex + ')/(' + yearsRegex + ')/(' + monthRegex + ')/(' + dayRegex + ')$', 'i'),
			function (view, action, year, month, day) {
				var viewAction = view + '/' + action;
				var hashInfo = _parseHash('#' + viewAction);
				if (viewAction == currentViewId) {
					var parsedDate = new Date(year, month - 1, day);
					var actionUpperCase = action.toUpperCase();
					if (actionUpperCase === "INDEX") {
						Teleopti.MyTimeWeb.MyReport.ForDay(moment(parsedDate));
					}
					if (actionUpperCase === "ADHERENCE") {
						Teleopti.MyTimeWeb.MyAdherence.ForDay(moment(parsedDate));
					}
					if (actionUpperCase === "QUEUEMETRICS") {
						Teleopti.MyTimeWeb.MyQueueMetrics.ForDay(moment(parsedDate));
					}
					return;
				}
				_invokeDisposeCallback(currentViewId);
				_adjustTabs(hashInfo);
				_loadContent(hashInfo);
			});

		crossroads.addRoute(new RegExp('^(' + viewRegex + ')/(' + actionRegex + ')/(ShiftTrade)/(' + dateRegex + ')$', 'i'),
			function (view, action, secondAction, date) {
				var hashInfo = _parseHash('#' + view + '/' + action);

				var parsedDate;
				if (/^(\d){8}$/.test(date)) {
					var y = date.substr(0, 4),
						m = date.substr(4, 2) - 1,
						d = date.substr(6, 2);
					parsedDate = new Date(y, m, d);
				}
				_invokeDisposeCallback(currentViewId);
				_adjustTabs(hashInfo);
				_loadContent(hashInfo,
					function () {
						Teleopti.MyTimeWeb.Request.ShiftTradeRequest(parsedDate);
					});
			});

		crossroads.addRoute(new RegExp('^(' + viewRegex + ')/(' + actionRegex + ')/(ShiftTradeBulletinBoard)/(' + dateRegex + ')$', 'i'),
			function (view, action, secondAction, date) {
				var hashInfo = _parseHash('#' + view + '/' + action);

				var parsedDate;
				if (/^(\d){8}$/.test(date)) {
					var y = date.substr(0, 4),
						m = date.substr(4, 2) - 1,
						d = date.substr(6, 2);
					parsedDate = new Date(y, m, d);
				}
				_invokeDisposeCallback(currentViewId);
				_adjustTabs(hashInfo);
				_loadContent(hashInfo,
					function () {
						Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardRequest(parsedDate);
					});
			});

		crossroads.addRoute(new RegExp('^(' + viewRegex + ')/(' + actionRegex + ')/(PostShiftForTrade)/(' + dateRegex + ')$', 'i'),
			function (view, action, secondAction, date) {
				var hashInfo = _parseHash('#' + view + '/' + action);

				var parsedDate;
				if (/^(\d){8}$/.test(date)) {
					var y = date.substr(0, 4),
						m = date.substr(4, 2) - 1,
						d = date.substr(6, 2);
					parsedDate = new Date(y, m, d);
				}
				_invokeDisposeCallback(currentViewId);
				_adjustTabs(hashInfo);
				_loadContent(hashInfo,
					function () {
						Teleopti.MyTimeWeb.Request.PostShiftForTradeRequest(parsedDate);
					});
			});

		crossroads.addRoute(new RegExp("^(" + viewRegex + ")/(" + actionRegex + ")/(" + yearsRegex + ")/(" + monthRegex
			+ ")/(" + dayRegex + ")/Probability/(" + intOptionRegex + ")$", "i"),
			function (view, action, year, month, day, probability) {
				var hashInfo = _parseHash("#" + view + "/" + action);
				_invokeDisposeCallback(currentViewId);
				_adjustTabs(hashInfo);
				_loadContent(hashInfo);
			});

		crossroads.addRoute(new RegExp("^(" + viewRegex + ")/(" + actionRegex + ")/Probability/(" + intOptionRegex + ")$", "i"),
			function (view, action, probability) {
				var hashInfo = _parseHash("#" + view + "/" + action);
				_invokeDisposeCallback(currentViewId);
				_adjustTabs(hashInfo);
				_loadContent(hashInfo);
			});

		crossroads.addRoute(new RegExp('^(' + viewRegex + ')/(' + actionRegex + ')/(' + actionRegex + ')/(' + dateRegex + ')$', 'i'),
			function (view, action, secondAction, date) {
				var hashInfo = _parseHash('#' + view + '/' + action);
				_invokeDisposeCallback(currentViewId);
				_adjustTabs(hashInfo);
				_loadContent(hashInfo);
			});

		crossroads.addRoute(new RegExp('^(.*)$', 'i'),
			function (hash) {
				var hashInfo = _parseHash('#' + hash);
				_invokeDisposeCallback(currentViewId);
				_adjustTabs(hashInfo);
				_loadContent(hashInfo);
			});
	}

	function _initializeHasher() {
		hasher.prependHash = '';
		var parseHash = function (newHash, oldHash) {
			crossroads.parse(newHash);
		};
		hasher.initialized.add(parseHash);
		hasher.changed.add(parseHash);
		hasher.init();
	}

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
		hasher.setHash(hash);
	}

	function _endsWith(str, suffix) {
		return str.indexOf(suffix, str.length - suffix.length) !== -1;
	}

	function _isMobile(window) {
		var ua = window.navigator.userAgent;


		if (ua.match(/Android/i) || ua.match(/webOS/i) || ua.match(/iPhone/i) || ua.match(/iPod/i)) {
			return true;
		}
		return false;
	}

	function _parseHash(hash) {
		var isWeekSchedule = hash.indexOf("#Schedule") >= 0;

		if (_endsWith(hash, "Tab")) {
			var suffix = (hash.indexOf("#Schedule") === 0) ? ((_isMobile(window)) ? (_showDayScheduleForStartPage() ? "/MobileDay" : "/MobileWeek") : "/Week") : "/Index";
			hash = hash.substring(0, hash.length - 'Tab'.length) + suffix;
		}


		if (hash.length > 0) {
			hash = hash.substring(1);
		}

		var parts = $.merge(hash.split('/'), [null, null, null, null, null, null, null, null]);
		parts.length = 8;// why set length to 8?

		var probability = undefined;
		if (isWeekSchedule) {
			parts.forEach(function (p, i, arr) {
				if (p == "Probability" && arr[i + 1]) {
					probability = parseInt(arr[i + 1]);
				}
			});
		}

		var controller = parts[0];
		var action = parts[1];
		var actionHash = controller + "/" + action;

		var dateHash = "";
		var dateMatch = hash.match(/\d{4}\/\d{2}\/\d{2}/);
		if (dateMatch)
			dateHash = dateMatch[0];
		return {
			hash: hash,
			parts: parts,
			controller: controller,
			action: action,
			actionHash: actionHash,
			dateHash: dateHash,
			probability: probability
		};
	}

	function _resetParsedHash() {
		location.hash = '';
	}

	function _adjustTabs(hashInfo) {
		var tabHref = '#' + hashInfo.controller + 'Tab';
		$('#bs-example-navbar-collapse-1 .nav li').removeClass('active');
		$('a[href="' + tabHref + '"]').parent().addClass('active');

		// hide off canvas menu when it has been clicked
		var offCanvasMenu = $(".navbar-offcanvas");
		if (offCanvasMenu.hasClass('in')) {
			offCanvasMenu.offcanvas('hide');
		}
	}

	function _loadContent(hashInfo, secondAction) {
		_disablePortalControls();

		_ajax.Ajax({
			url: hashInfo.hash,
			global: true,
			success: function (html) {
				var viewId = hashInfo.actionHash; //gröt
				$('#body-inner').html(html);
				$('#body-inner').show();
				_invokeInitCallback(viewId, secondAction);
				currentViewId = viewId;
			},
			error: function () {
				var htmlString = '<h4 style="text-align:center;">Unable to load the page.</h4>';
				$('#body-inner').show();
				$('#body-inner').html(htmlString);
			}
		});
	}

	function _invokeDisposeCallback(viewId) {
		if (viewId != null)
			viewId = viewId.toUpperCase();
		var partialDispose = _partialViewDisposeCallback[viewId];
		if ($.isFunction(partialDispose))
			partialDispose();
	}

	function _invokeInitCallback(viewId, secondAction) {
		if (viewId != null)
			viewId = viewId.toUpperCase();
		var partialInit = _partialViewInitCallback[viewId];
		if ($.isFunction(partialInit))
			partialInit(_readyForInteraction, _completelyLoaded);
		Teleopti.MyTimeWeb.Common.PartialInit();
		if ($.isFunction(secondAction)) {
			secondAction();
		}
	}

	function _readyForInteraction() {
		Teleopti.MyTimeWeb.Test.TestMessage("Ready for interaction");
	}

	function _completelyLoaded() {
		Teleopti.MyTimeWeb.Test.TestMessage("Completely loaded");
	}

	return {
		Init: function (settings, window, ajax) {
			_ajax = ajax ? ajax : new Teleopti.MyTimeWeb.Ajax();
			Teleopti.MyTimeWeb.AjaxSettings = settings;
			Teleopti.MyTimeWeb.Common.Init(settings, _ajax);
			Teleopti.MyTimeWeb.Test.Init(settings);
			_settings = settings;
			_layout();
			_attachAjaxEvents();
			_initNavigation(window);
			_setupRoutes();
			_initializeHasher();
		},

		NavigateTo: function (action, date, id) {
			_navigateTo(action, date, id);
		},
		ResetParsedHash: function () {
			_resetParsedHash();
		},
		ParseHash: function (locationObj) {
			var location = locationObj || window.location;
			return _parseHash(location.hash);
		},
		RegisterPartialCallBack: function (viewId, callBack, disposeCallback) {
			_registerPartialCallback(viewId, callBack, disposeCallback);
		},
		CurrentFixedDate: function () {
			return currentFixedDate;
		},
		InitPeriodSelection: function (rangeSelectorId, periodData, actionSuffix) {
		},
		IsMobile: function () { return _isMobile(window) }
	};
})(jQuery);

Teleopti.MyTimeWeb.Portal.Layout = (function ($) {
	return {
		ActivateHorizontalScroll: function () {
			$(window).scroll(function () {
				$('header').css("left", -$(window).scrollLeft() + "px");
			});
		}
	};
})(jQuery);