if (typeof Teleopti.MyTimeWeb.UserInfo === 'undefined') {
	Teleopti.MyTimeWeb.UserInfo = {
		WhenLoaded: function(cb) {
			cb && cb();
		}
	};
}

Teleopti.MyTimeWeb.Common = (function($) {
	var _settings = {},
		_ajax,
		_userData = null,
		_userTexts = null,
		_userDataCallQueue = [],
		_userDataFetchInProgress = false,
		toggleCache = {},
		constants = {
			scheduleHeight: 668, // Same value as height of class "weekview-day-schedule"
			mobileMinScheduleHeight: 700,
			pixelOfOneHourInTeamSchedule: 60,
			pixelToDisplayAll: 38,
			pixelToDisplayTitle: 16,

			timelineMarginInMinutes: 15, // Refer to Teleopti.Ccc.Web.Areas.MyTime.Core.WeekScheduleDomainDataProvider.getMinMaxTime()
			totalMinutesOfOneDay: 1440, //: 24 * 60, Total minutes of a day
			fullDayHourStr: '1.00', //: 24 * 60, Total minutes of a day
			probabilityIntervalLengthInMinute: 15,
			probabilityType: {
				none: 0,
				absence: 1,
				overtime: 2
			},
			probabilityLevel: {
				low: 0,
				high: 1
			},
			probabilityClass: {
				lowProbabilityClass: 'probability-low',
				highProbabilityClass: 'probability-high',
				expiredProbabilityClass: 'probability-expired'
			},
			layoutDirection: {
				vertical: 0,
				horizontal: 1
			},
			serviceDateTimeFormat: {
				dateTime: 'YYYY-MM-DD HH:mm',
				dateOnly: 'YYYY-MM-DD',
				timeOnly: 'HH:mm'
			}
		};

	function isToggleEnabled(toggleName) {
		var result = false;
		if (toggleCache[toggleName] !== undefined) {
			return toggleCache[toggleName];
		}

		if (_settings.baseUrl == undefined) {
			throw 'you cannot ask toggle before you initialize it!';
		}

		_ajax.Ajax({
			url: '../ToggleHandler/IsEnabled?toggle=' + toggleName,
			async: false,
			success: function(data) {
				result = data.IsEnabled;
				toggleCache[toggleName] = result;
			}
		});
		return result;
	}

	function _log() {
		if (window.console && window.console.log) window.console.log(Array.prototype.join.call(arguments, ' '));
	}

	function _addSubscription(options) {
		Teleopti.MyTimeWeb.MessageBroker.AddSubscription({
			url: _userData.Url,
			referenceId: _userData.AgentId,
			callback: options.successCallback,
			errCallback: options.errorCallback,
			domainType: options.domainType,
			page: options.page
		});
	}

	function _getUserData(callback) {
		if (_userData !== null) {
			callback(_userData);
			return;
		}

		if (_userDataFetchInProgress) {
			_userDataCallQueue.push(callback);
			return;
		}

		_userDataFetchInProgress = true;

		_ajax.Ajax({
			url: 'UserData/FetchUserData',
			dataType: 'json',
			type: 'GET',
			success: function(data) {
				_userData = data;
				callback(data);
				for (var queuedRequestIdx in _userDataCallQueue) {
					_userDataCallQueue[queuedRequestIdx](data);
				}
			}
		});
	}

	function _subscribeToMessageBroker(options, ajax) {
		_ajax = ajax || _ajax || new Teleopti.MyTimeWeb.Ajax();

		if (_userData == null) {
			var onSuccessCallback = function(data) {
				_addSubscription(options);
			};

			_getUserData(onSuccessCallback);
			return;
		}
		_addSubscription(options);
	}

	function _setupCalendar(options) {
		var timeFormat = '';

		if (options.TimeFormat) {
			timeFormat = _updateMeridiem(options.TimeFormat);
		}

		var isJalaali = options.UseJalaaliCalendar;
		Teleopti.MyTimeWeb.Common.UseJalaaliCalendar = isJalaali;
		Teleopti.MyTimeWeb.Common.DateFormat = isJalaali
			? 'jYYYY/jMM/jDD'
			: options.DateFormat.replace(/'([^\[\]]*)'/g, '[$1]');
		Teleopti.MyTimeWeb.Common.DayOnlyFormat = isJalaali ? 'jDD' : 'DD';
		Teleopti.MyTimeWeb.Common.MonthOnlyFormat = isJalaali ? 'jMMMM' : 'MMMM';
		Teleopti.MyTimeWeb.Common.TimeFormat = timeFormat;
		Teleopti.MyTimeWeb.Common.Meridiem = { AM: options.AMDesignator, PM: options.PMDesignator };
		Teleopti.MyTimeWeb.Common.DateTimeFormat =
			Teleopti.MyTimeWeb.Common.DateFormat + ' ' + Teleopti.MyTimeWeb.Common.TimeFormat;
		Teleopti.MyTimeWeb.Common.DateFormatLocale = options.DateFormatLocale;

		Teleopti.MyTimeWeb.Common.DateTimeDefaultValues = options.DateTimeDefaultValues;

		if (isJalaali) {
			moment.loadPersian();
		}
	}

	function _setupTeleoptiTime(options) {
		return function(currentUtcTime) {
			var timezoneOffsetInMinute = options.UserTimezoneOffsetMinute * 60000;
			var currentTime, currentUtcExpressedInLocalTime;
			if (!currentUtcTime) {
				currentTime = new Date();
				currentUtcExpressedInLocalTime = new Date(
					currentTime.getTime() + currentTime.getTimezoneOffset() * 60000
				);
			} else {
				currentTime = currentUtcTime;
				currentUtcExpressedInLocalTime = currentUtcTime;
			}

			var offset = timezoneOffsetInMinute;

			var hasDayLightSavingStr = options.HasDayLightSaving;
			if (hasDayLightSavingStr.toLowerCase() === 'true') {
				var dlsStart = new Date(options.DayLightSavingStart);
				var dlsEnd = new Date(options.DayLightSavingEnd);

				if (dlsStart < dlsEnd) {
					if (currentTime >= dlsStart && currentTime <= dlsEnd) {
						offset += options.DayLightSavingAdjustmentInMinute * 60000;
					}
				} else {
					if (currentTime >= dlsStart || currentTime <= dlsEnd) {
						offset += options.DayLightSavingAdjustmentInMinute * 60000;
					}
				}
			}

			var currentUserTime = new Date(currentUtcExpressedInLocalTime.getTime() + offset);
			return currentUserTime;
		};
	}

	function _updateMeridiem(timeFormat) {
		return timeFormat.replace(/tt|t/gi, 'A');
	}

	function _formatDate(date) {
		if (moment.isMoment(date)) {
			return date.format(Teleopti.MyTimeWeb.Common.DateFormat);
		}
		return moment(date).format(Teleopti.MyTimeWeb.Common.DateFormat);
	}

	function _formatDayOnly(date) {
		if (moment.isMoment(date)) {
			return date.format(Teleopti.MyTimeWeb.Common.DayOnlyFormat);
		}
		return moment(date).format(Teleopti.MyTimeWeb.Common.DayOnlyFormat);
	}

	function _formatMonthOnly(date) {
		if (moment.isMoment(date)) {
			return date.format(Teleopti.MyTimeWeb.Common.MonthOnlyFormat);
		}
		return moment(date).format(Teleopti.MyTimeWeb.Common.MonthOnlyFormat);
	}

	function _formatDateTime(dateTime) {
		if (moment.isMoment(dateTime)) {
			return dateTime.format(Teleopti.MyTimeWeb.Common.DateTimeFormat);
		}
		return moment(dateTime).format(Teleopti.MyTimeWeb.Common.DateTimeFormat);
	}

	function _formatTime(dateTime) {
		if (moment.isMoment(dateTime)) {
			return dateTime.format(Teleopti.MyTimeWeb.Common.TimeFormat);
		}
		return moment(dateTime).format(Teleopti.MyTimeWeb.Common.TimeFormat);
	}

	function _formatMonth(date) {
		if (moment.isMoment(date)) {
			return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar ? date.format('jMMMM jYYYY') : date.format('MMMM YYYY');
		}
		return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar
			? moment(date).format('jMMMM jYYYY')
			: moment(date).format('MMMM YYYY');
	}

	function _formatMonthShort(date) {
		if (moment.isMoment(date)) {
			return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar ? date.format('jMMM jYYYY') : date.format('MMM YYYY');
		}
		return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar
			? moment(date).format('jMMM jYYYY')
			: moment(date).format('MMM YYYY');
	}

	function _formatMonthShort(date) {
		if (moment.isMoment(date)) {
			return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar ? date.format('jMMM jYYYY') : date.format('MMM YYYY');
		}
		return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar
			? moment(date).format('jMMM jYYYY')
			: moment(date).format('MMM YYYY');
	}

	function _datesAreSame(dateOne, dateTwo) {
		return dateOne.isSame(dateTwo, 'day');
	}

	function _formatDatePeriod(start, end, showTime) {
		if (!moment.isMoment(start)) {
			start = moment(start);
			end = moment(end);
		}

		if (showTime) {
			if (_datesAreSame(start, end)) {
				return (
					start.format(Teleopti.MyTimeWeb.Common.DateTimeFormat) +
					' - ' +
					end.format(Teleopti.MyTimeWeb.Common.TimeFormat)
				);
			}

			return (
				start.format(Teleopti.MyTimeWeb.Common.DateTimeFormat) +
				' - ' +
				end.format(Teleopti.MyTimeWeb.Common.DateTimeFormat)
			);
		}

		return (
			start.format(Teleopti.MyTimeWeb.Common.DateFormat) +
			' - ' +
			end.format(Teleopti.MyTimeWeb.Common.DateFormat)
		);
	}

	function _formatServiceDate(date) {
		// prevent locale translation to non-decimal symbols
		var localeSafeMoment = moment(date).locale('en');
		return localeSafeMoment.format(Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly);
	}

	function _isFixedDate(dateString) {
		return dateString.match(/^\d{4}-\d{2}-\d{2}$/);
	}

	function _parseFixedDateStringToDate(dateString) {
		if (!_isFixedDate(dateString)) {
			return new Date();
		}
		return moment(dateString).toDate();
	}

	function _fixedDateToPartsUrl(fixedDate) {
		return '/' + fixedDate.split('-').join('/');
	}

	function getTextColorBasedOnBackgroundColor(backgroundColor) {
		//Note: for the unified look of the activity color and text in Web,
		//please keep this color strategy synced with the one in WFM: colorUtils service
		//which locates in \Teleopti.Ccc.Web\Teleopti.Ccc.Web\WFM\app\global\utilities\colorUtils.service.js

		if (typeof backgroundColor != 'string' || backgroundColor.length == 0) return 'black';

		if (backgroundColor.indexOf('#') > -1) {
			backgroundColor = _hexToRGB(backgroundColor);
		}

		backgroundColor = backgroundColor.slice(backgroundColor.indexOf('(') + 1, backgroundColor.indexOf(')'));

		var backgroundColorArr = backgroundColor.split(',');

		var brightness =
			backgroundColorArr[0] * 0.299 + backgroundColorArr[1] * 0.587 + backgroundColorArr[2] * 0.114;

		return brightness < 128 ? 'white' : 'black';
	}

	function _hexToRGB(hex) {
		var result = /^#?([a-f\d]{2}|[a-f\d]{1})([a-f\d]{2}|[a-f\d]{1})([a-f\d]{2}|[a-f\d]{1})$/i.exec(hex);
		var rgb = result
			? {
					r: parseInt(_fillupDigits(result[1]), 16),
					g: parseInt(_fillupDigits(result[2]), 16),
					b: parseInt(_fillupDigits(result[3]), 16)
				}
			: null;
		if (rgb) return 'rgb(' + rgb.r + ',' + rgb.g + ',' + rgb.b + ')';
		return rgb;
	}

	function _fillupDigits(hex) {
		if (hex.length == 1) {
			hex += '' + hex;
		}
		return hex;
	}

	function _rgbToHex(rgb) {
		if (rgb.charAt(0) === '#') return rgb;
		var ds = rgb.split(/\D+/);
		var decimal = Number(ds[1]) * 65536 + Number(ds[2]) * 256 + Number(ds[3]);
		var digits = 6;
		var hexString = decimal.toString(16);
		while (hexString.length < digits) hexString += '0';

		return '#' + hexString;
	}

	function _rightPadNumber(number, padding) {
		var formattedNumber = padding + number;
		var start = formattedNumber.length - padding.length;
		formattedNumber = formattedNumber.substring(start);
		return formattedNumber;
	}

	function stripTeleoptiTimeToUTCForScenarioTest() {
		var timeWithTimezone,
			teleoptiTime = Date.prototype.getTeleoptiTime && Date.prototype.getTeleoptiTime();
		if (teleoptiTime) timeWithTimezone = moment(teleoptiTime).format();
		else timeWithTimezone = moment().format();

		return moment(timeWithTimezone.substr(0, 19) + '+00:00'); //btw, timezone info is wrong? why? need confirmation
	}

	function _ensureDST(userTime, daylightSavingAdjustment, utcOffsetInMinutes) {
		//whether in DST is judged in UTC time
		if (daylightSavingAdjustment == undefined || daylightSavingAdjustment === null) {
			return;
		}

		var userTimestamp = userTime.valueOf();
		var dstStartTimestamp = moment(daylightSavingAdjustment.StartDateTime + '+00:00').valueOf();
		var dstEndTimestamp = moment(daylightSavingAdjustment.EndDateTime + '+00:00')
			.add(-daylightSavingAdjustment.AdjustmentOffsetInMinutes, 'minutes')
			.valueOf();

		if (dstStartTimestamp < dstEndTimestamp) {
			if (dstStartTimestamp < userTimestamp && userTimestamp < dstEndTimestamp) {
				adjustToDST(userTime, daylightSavingAdjustment, utcOffsetInMinutes);
			} else {
				resetToUserTimeWithoutDST(userTime, utcOffsetInMinutes);
			}
		} else {
			// for DST like Brasilia
			if (dstEndTimestamp <= userTimestamp && userTimestamp <= dstStartTimestamp) {
				resetToUserTimeWithoutDST(userTime, utcOffsetInMinutes);
			} else {
				adjustToDST(userTime, daylightSavingAdjustment, utcOffsetInMinutes);
			}
		}
	}

	function adjustToDST(userTime, daylightSavingAdjustment, baseUtcOffsetInMinutes) {
		userTime.zone(-daylightSavingAdjustment.AdjustmentOffsetInMinutes - baseUtcOffsetInMinutes);
	}

	function resetToUserTimeWithoutDST(userTime, baseUtcOffsetInMinutes) {
		userTime.zone(-baseUtcOffsetInMinutes);
	}

	function _getCurrentUserDateTime(utcOffsetInMinutes, daylightSavingAdjustment) {
		var currentUserDateTime =
			Date.prototype.getTeleoptiTimeChangedByScenario === true
				? stripTeleoptiTimeToUTCForScenarioTest().zone(-utcOffsetInMinutes)
				: moment().zone(-utcOffsetInMinutes); //work in user timezone, just make life easier

		_ensureDST(currentUserDateTime, daylightSavingAdjustment, utcOffsetInMinutes);
		return currentUserDateTime;
	}

	function _convertColorToRGB(color) {
		if (!color) {
			return '';
		}
		if (color.indexOf('#') > -1) {
			return color;
		} else if (color.split(',').length == 3) {
			return 'rgb(' + color + ')';
		} else {
			return 'rgba(' + color + ')';
		}
	}
	return {
		Init: function(settings, ajax) {
			_settings = settings;
			_ajax = ajax ? ajax : new Teleopti.MyTimeWeb.Ajax();
		},
		PartialInit: function() {
			Teleopti.MyTimeWeb.Common.Layout.ActivateTooltip();
		},
		AjaxFailed: function(jqXHR, status, title) {
			Teleopti.MyTimeWeb.Ajax.UI.ShowErrorDialog(jqXHR, status, title);
		},
		Log: function(logmessage) {
			_log(logmessage);
		},
		SetUserTexts: function(userTexts) {
			_userTexts = userTexts;
		},
		SetupCalendar: function(options) {
			_setupCalendar(options);
		},

		SetupTeleoptiTime: function(options) {
			return _setupTeleoptiTime(options);
		},

		FormatDate: function(date) {
			return _formatDate(date);
		},

		FormatDayOnly: function(date) {
			return _formatDayOnly(date);
		},

		FormatMonthOnly: function(date) {
			return _formatMonthOnly(date);
		},

		FormatDateTime: function(dateTime) {
			return _formatDateTime(dateTime);
		},

		FormatTime: function(dateTime) {
			return _formatTime(dateTime);
		},

		FormatDatePeriod: function(startDate, endDate, showTimes) {
			return _formatDatePeriod(startDate, endDate, showTimes);
		},

		FormatMonth: function(date) {
			return _formatMonth(date);
		},

		FormatMonthShort: function(date) {
			return _formatMonthShort(date);
		},

		FormatMonthShort: function(date) {
			return _formatMonthShort(date);
		},

		FormatServiceDate: function(date) {
			return _formatServiceDate(date);
		},

		ParseToDate: function(dateString) {
			return _parseFixedDateStringToDate(dateString);
		},

		FormatTimeSpan: function(totalMinutes) {
			if (!totalMinutes) return '0:00';
			var minutes = totalMinutes % 60;
			var hours = Math.floor(totalMinutes / 60);
			var roundedMinutes = Math.round(minutes);

			return hours + ':' + _rightPadNumber(roundedMinutes, '00');
		},

		FixedDateToPartsUrl: function(fixedDate) {
			return _fixedDateToPartsUrl(fixedDate);
		},
		IsFixedDate: function(dateString) {
			return _isFixedDate(dateString);
		},
		MomentAsUTCIgnoringTimezone: function(dateTimeStr) {
			return moment.tz(dateTimeStr, 'UTC');
		},
		GetTextColorBasedOnBackgroundColor: function(backgroundColor) {
			return getTextColorBasedOnBackgroundColor(backgroundColor);
		},
		RGBTohex: _rgbToHex,
		IsRtl: function() {
			return $('html').attr('dir') == 'rtl';
		},
		IsHostAMobile: function(ua) {
			ua = ua || navigator.userAgent;
			return /Mobile/i.test(ua) && !/ipad/i.test(ua);
		},
		IsHostAniPhone: function(ua) {
			ua = ua || navigator.userAgent;
			return /Mobile/i.test(ua) && /iPhone/i.test(ua);
		},
		IsHostAniPad: function(ua) {
			ua = ua || navigator.userAgent;
			return /ipad/i.test(ua) || (/Android/i.test(ua) && !/Mobile/i.test(ua));
		},
		Constants: constants,
		SubscribeToMessageBroker: _subscribeToMessageBroker,
		GetUserData: _getUserData,
		IsToggleEnabled: isToggleEnabled,
		GetUserTexts: function() {
			return _userTexts;
		},
		ShowAgentScheduleMessenger: function() {
			$('#autocollapse.teleopti-mytime-top-menu ul.show-outside-toolbar #asm-divider').show();
			$('#autocollapse.teleopti-mytime-top-menu ul.show-outside-toolbar #asm-link').show();
		},
		HideAgentScheduleMessenger: function() {
			$('#autocollapse.teleopti-mytime-top-menu ul.show-outside-toolbar #asm-divider').hide();
			$('#autocollapse.teleopti-mytime-top-menu ul.show-outside-toolbar #asm-link').hide();
		},
		GetCurrentUserDateTime: _getCurrentUserDateTime,
		FakeToggles: function(toggles) {
			toggleCache = toggles;
		},
		DisableToggle: function(toggleName) {
			toggleCache[toggleName] = false;
		},
		EnableToggle: function(toggleName) {
			toggleCache[toggleName] = true;
		},
		ConvertColorToRGB: _convertColorToRGB
	};
})(jQuery);

Teleopti.MyTimeWeb.Common.Layout = (function($) {
	return {
		ActivatePlaceHolder: function() {
			$(
				'textarea[placeholder][placeholder!=""], :text[placeholder][placeholder!=""], :password[placeholder][placeholder!=""]'
			).placeholder();
		},

		Init: function() {
			function autocollapse() {
				var navbar = $('#autocollapse');
				var innerNavbar = $('#innerNavBar');
				var button = $('ul.navbar-nav li');

				navbar.removeClass('custom-collapsed'); // set standart view
				innerNavbar.addClass('container'); //size according to bootstrap
				//toolbar min-heigth: 50px, button: 50px;
				if (Math.floor(navbar.innerHeight()) > button.height() + 1) {
					// check if we've got 2 lines
					innerNavbar.removeClass('container');
					navbar.addClass('custom-collapsed'); // force collapse mode
				}
			}
			//And touchend event to close dropdown menus when taping outside
			$('body').on('touchend', function(e) {
				var badgePanel = $('.navbar.teleopti-mytime-top-menu ul.navbar-nav li.dropdown#BadgePanel');
				if (badgePanel[0] && !badgePanel[0].contains(e.target)) {
					badgePanel.removeClass('open');
				}

				var userSettings = $('.navbar.teleopti-mytime-top-menu ul.navbar-nav li.dropdown#user-settings');
				if (userSettings[0] && !userSettings[0].contains(e.target)) {
					userSettings.removeClass('open');
				}

				var collapsingMenu = $('.teleopti-mytime-top-menu .navbar-offcanvas');
				var mainNavbarToggler = $('.navbar-toggle-button');
				if (
					collapsingMenu[0] &&
					!collapsingMenu[0].contains(e.target) &&
					mainNavbarToggler[0] &&
					!mainNavbarToggler[0].contains(e.target)
				) {
					if (collapsingMenu.hasClass('in')) {
						mainNavbarToggler.trigger('click');
					}
				}
			});

			$(document).on('ready', autocollapse);
			$(window).on('resize', autocollapse);
		},

		//Activating tooltip where available
		ActivateTooltip: function() {
			$('.qtip-tooltip').each(function() {
				var content = {
					title: $(this).attr('tooltip-title'),
					text: $(this).attr('tooltip-text')
				};

				var attr = $(this).attr('title');
				if (typeof attr !== 'undefined' && attr !== false) {
					content = {
						text: function() {
							return $(this).attr('title');
						}
					};
				}

				$(this).qtip({
					content: content,
					style: {
						def: false,
						classes: 'ui-tooltip-custom ui-tooltip-rounded ui-tooltip-shadow',
						tip: true
					},
					position: {
						my: 'bottom left',
						at: 'top right',
						target: 'mouse',
						adjust: {
							x: 10,
							y: -13
						}
					}
				});
			});
		}
	};
})(jQuery);

Teleopti.MyTimeWeb.Common.LoadingOverlay = (function($) {
	function _addOverlay(optionsOrElement) {
		var options = {};
		if (optionsOrElement.element) {
			options = optionsOrElement;
		} else {
			options.element = optionsOrElement;
		}
		options.loadingClass = options.loadingClass || 'loading';

		options.element.addClass('relative');
		$('<div>')
			.css({
				height: '100%'
			})
			.addClass(options.loadingClass)
			.addClass('overlay')
			.appendTo(options.element)
			.show();
	}

	function _removeOverlay(element) {
		$('.overlay', element).remove();
	}

	return {
		Add: function(optionsOrElement) {
			_addOverlay(optionsOrElement);
		},
		Remove: function(element) {
			_removeOverlay(element);
		}
	};
})(jQuery);

String.prototype.format = function() {
	var args = arguments;
	return this.replace(/{(\d+)}/g, function(match, number) {
		return typeof args[number] != 'undefined' ? args[number] : match;
	});
};
