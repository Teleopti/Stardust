/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/jquery.qtip.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Common = (function ($) {
    var _settings = {},
        _ajax,
		_userData = null,
		_userTexts = null,
		_userDataCallQueue = [],
		_userDataFetchInProgress = false,
		toggleCache = {},
		constants = {
			scheduleHeight: 668, // Same value as height of class "weekview-day-schedule"
			mobileMinScheduleHeight: 550,
			pixelToDisplayAll: 38,
			pixelToDisplayTitle: 16,

			timelineMarginInMinutes: 15, // Refer to Teleopti.Ccc.Web.Areas.MyTime.Core.WeekScheduleDomainDataProvider.getMinMaxTime()
			totalMinutesOfOneDay: 1440, //: 24 * 60, Total minutes of a day
			maximumDaysDisplayingProbability: 14,
			probabilityIntervalLengthInMinute: 15,
			probabilityType: {
				none: 0,
				absence: 1,
				overtime: 2,
			},
			probabilityClass: {
				lowProbabilityClass: "probability-low",
				highProbabilityClass: "probability-high",
				expiredProbabilityClass: "probability-expired"
			},
			layoutDirection: {
				vertical: 0,
				horizontal: 1
			},
			dateOnlyFormat:"YYYY-MM-DD"
		};

	function isToggleEnabled(toggleName) {
		var result = false;
		if (toggleCache[toggleName] !== undefined) {
			return toggleCache[toggleName];
		}

		if (_settings.baseUrl == undefined) {
			throw "you cannot ask toggle before you initialize it!";
        }

		_ajax.Ajax({
			url: "../ToggleHandler/IsEnabled?toggle=" + toggleName,
			async: false,
			success: function (data) {
				result = data.IsEnabled;
				toggleCache[toggleName] = result;
			}
		});
		return result;
	}

	function _log() {
		if (window.console && window.console.log)
			window.console.log(Array.prototype.join.call(arguments, ' '));
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
	};

	function _getUserData(callback) {

		if (_userData !== null) {
			callback(_userData);
			return;
		};
		
		if (_userDataFetchInProgress) {
			_userDataCallQueue.push(callback);
			return;
		};

        _userDataFetchInProgress = true;

		_ajax.Ajax({
			url: 'UserData/FetchUserData',
			dataType: "json",
			type: 'GET',
			success: function (data) {
				_userData = data;
				callback(data);
				for (var queuedRequestIdx in _userDataCallQueue) {
					_userDataCallQueue[queuedRequestIdx](data);
				}
			}
		});
    };

	function _subscribeToMessageBroker(options) {
		_ajax = _ajax || new Teleopti.MyTimeWeb.Ajax();

		if (_userData == null) {
			var onSuccessCallback = function (data) {
				_addSubscription(options);
			}

			_getUserData(onSuccessCallback);
			return;
		}
		_addSubscription(options);
	};

	function _setupCalendar(options) {

		var timeFormat = "";

		if (options.TimeFormat) {
			timeFormat = _updateMeridiem(options.TimeFormat);
		}

		var isJalaali = options.UseJalaaliCalendar;
		Teleopti.MyTimeWeb.Common.UseJalaaliCalendar = isJalaali;
		Teleopti.MyTimeWeb.Common.DateFormat = isJalaali ? "jYYYY/jMM/jDD" : options.DateFormat;
		Teleopti.MyTimeWeb.Common.DayOnlyFormat = isJalaali ? "jDD" : 'DD';
		Teleopti.MyTimeWeb.Common.MonthOnlyFormat = isJalaali ? "jMMMM" : 'MMMM';
		Teleopti.MyTimeWeb.Common.TimeFormat = timeFormat;
		Teleopti.MyTimeWeb.Common.Meridiem = { AM: options.AMDesignator, PM: options.PMDesignator };
		Teleopti.MyTimeWeb.Common.DateTimeFormat = Teleopti.MyTimeWeb.Common.DateFormat + " " + Teleopti.MyTimeWeb.Common.TimeFormat;

		Teleopti.MyTimeWeb.Common.DateTimeDefaultValues = options.DateTimeDefaultValues;

		if (isJalaali) {
			moment.loadPersian();
		}

	};

	function _setupTeleoptiTime(options) {
	
		return function(currentUtcTime) {
			var timezoneOffsetInMinute = options.UserTimezoneOffsetMinute * 60000;
			var currentTime, currentUtcExpressedInLocalTime;
			if (!currentUtcTime) {
				currentTime = new Date();
				currentUtcExpressedInLocalTime = new Date(currentTime.getTime() + currentTime.getTimezoneOffset() * 60000);
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
		}
	};

	function _updateMeridiem(timeFormat) {
		return timeFormat.replace(/tt|t/gi, "A");
	};

	function _formatDate(date) {
		if (moment.isMoment(date)) {
			return date.format(Teleopti.MyTimeWeb.Common.DateFormat);
		}
		return moment(date).format(Teleopti.MyTimeWeb.Common.DateFormat);
	};

	function _formatDayOnly(date) {
		if (moment.isMoment(date)) {
			return date.format(Teleopti.MyTimeWeb.Common.DayOnlyFormat);
		}
		return moment(date).format(Teleopti.MyTimeWeb.Common.DayOnlyFormat);
	};

	function _formatMonthOnly(date) {
		if (moment.isMoment(date)) {
			return date.format(Teleopti.MyTimeWeb.Common.MonthOnlyFormat);
		}
		return moment(date).format(Teleopti.MyTimeWeb.Common.MonthOnlyFormat);
	};


	function _formatDateTime(dateTime) {
		
		if (moment.isMoment(dateTime)) {
			return dateTime.format(Teleopti.MyTimeWeb.Common.DateTimeFormat);
		}
		return moment(dateTime).format(Teleopti.MyTimeWeb.Common.DateTimeFormat);
	};
	
	function _formatTime(dateTime) {
		if (moment.isMoment(dateTime)) {
			return date.format(Teleopti.MyTimeWeb.Common.TimeFormat);
		}
		return moment(dateTime).format(Teleopti.MyTimeWeb.Common.TimeFormat);

	};

	function _formatMonth(date) {

		if (moment.isMoment(date)) {
			return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar ? date.format('jMMMM jYYYY') : date.format('MMMM YYYY');
		}
		return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar ? moment(date).format('jMMMM jYYYY') : moment(date).format('MMMM YYYY');

	};

	 function _datesAreSame(dateOne, dateTwo) {
			return (dateOne.isSame(dateTwo,'day'));
	};

	function _formatDatePeriod(start, end, showTime) {

		if (!moment.isMoment(start)) {

			start = moment(start);
			end = moment(end);
		}

		if (showTime) {

			if (_datesAreSame(start, end)) {
				return start.format(Teleopti.MyTimeWeb.Common.DateTimeFormat) + " - " + end.format(Teleopti.MyTimeWeb.Common.TimeFormat);
			}

			return start.format(Teleopti.MyTimeWeb.Common.DateTimeFormat) + " - " + end.format(Teleopti.MyTimeWeb.Common.DateTimeFormat);
		}

		return start.format(Teleopti.MyTimeWeb.Common.DateFormat) + " - " + end.format(Teleopti.MyTimeWeb.Common.DateFormat);

	};

	function _formatServiceDate(date) {
		// prevent locale translation to non-decimal symbols
		var localeSafeMoment = moment(date).locale('en');
		return localeSafeMoment.format(Teleopti.MyTimeWeb.Common.ServiceDateFormat);
	};

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
		return '/' + fixedDate.split("-").join("/");
	}

	function _getTextColorBasedOnBackgroundColor(backgroundColor) {
		backgroundColor = backgroundColor.slice(backgroundColor.indexOf('(') + 1, backgroundColor.indexOf(')'));

		var backgroundColorArr = backgroundColor.split(',');

		var brightness = backgroundColorArr[0] * 0.299 + backgroundColorArr[1] * 0.587 + backgroundColorArr[2] * 0.114;

		return brightness < 100 ? 'white' : 'black';
	}

	function _rightPadNumber(number, padding) {
		var formattedNumber = padding + number;
		var start = formattedNumber.length - padding.length;
		formattedNumber = formattedNumber.substring(start);
		return formattedNumber;
	};

	return {
		Init: function (settings, ajax) {
            _settings = settings;
            _ajax = ajax ? ajax : new Teleopti.MyTimeWeb.Ajax();
		},
		PartialInit: function () {
            Teleopti.MyTimeWeb.Common.Layout.ActivateTooltip();
		},
		AjaxFailed: function (jqXHR, noIdea, title) {
			var msg = $.parseJSON(jqXHR.responseText);
			$('#dialog-modal').attr('title', 'Error: ' + msg.ShortMessage);
			$('#dialog-modal').dialog({
				width: 800,
				height: 500,
				position: 'center',
				modal: true,
				create: function (event, ui) {
					var responseText = msg.Message;
					$(this).html(responseText);

					var closeBtn = $('.ui-dialog-titlebar-close');
					closeBtn.addClass('ui-state-default');
					closeBtn.append('<span class="ui-button-icon-primary ui-icon ui-icon-closethick"></span>');
				}
			});
			
			_log("Method Failed" + jqXHR + noIdea + title);
		},
		Log: function (logmessage) {
			_log(logmessage);
		},
		SetUserTexts: function(userTexts) {
            _userTexts = userTexts;
		},
		SetupCalendar: function (options) {
			_setupCalendar(options);
		},
		
		SetupTeleoptiTime : function(options) {
			return _setupTeleoptiTime(options);
		},

		FormatDate: function (date) {
			return _formatDate(date);
		},

		FormatDayOnly: function(date) {
			return _formatDayOnly(date);
		},

		FormatMonthOnly: function (date) {
			return _formatMonthOnly(date);
		},

		FormatDateTime: function (dateTime) {
			return _formatDateTime(dateTime);
		},

		FormatTime: function (dateTime) {
			return _formatTime(dateTime);
		},


		FormatDatePeriod: function (startDate, endDate, showTimes) {
			return _formatDatePeriod(startDate, endDate, showTimes);
		},

		FormatMonth: function (date) {
			return _formatMonth(date);
		},

		FormatServiceDate: function (date) {
			return _formatServiceDate(date);
		},

		ParseToDate: function (dateString) {
			return _parseFixedDateStringToDate(dateString);
		},

		FormatTimeSpan: function (totalMinutes) {
			if (!totalMinutes)
				return "0:00";
			var minutes = totalMinutes % 60;
			var hours = Math.floor(totalMinutes / 60);
			var roundedMinutes = Math.round(minutes);

			return hours + ":" + _rightPadNumber(roundedMinutes, "00");
		},

		FixedDateToPartsUrl: function (fixedDate) {
			return _fixedDateToPartsUrl(fixedDate);
		},
		IsFixedDate: function (dateString) {
			return _isFixedDate(dateString);
		},
		GetTextColorBasedOnBackgroundColor: function (backgroundColor) {
			return _getTextColorBasedOnBackgroundColor(backgroundColor);
		},
		IsRtl: function () {
			return $("html").attr("dir") == "rtl";
		},
		IsHostAMobile: function () {
			return (/Mobile/i.test(navigator.userAgent) && !/ipad/i.test(navigator.userAgent));
		},
		Constants: constants,
		SubscribeToMessageBroker: _subscribeToMessageBroker,
		GetUserData: _getUserData,
        IsToggleEnabled: isToggleEnabled,
        GetUserTexts: function () {
            return _userTexts;
        }
	};

})(jQuery);

Teleopti.MyTimeWeb.Common.ServiceDateFormat = 'YYYY-MM-DD';

Teleopti.MyTimeWeb.Common.Layout = (function ($) {
	return {
		ActivatePlaceHolder: function () {
			$('textarea, :text, :password').placeholder();
		},

		Init: function () {
			function autocollapse() {
				var navbar = $('#autocollapse');
				var innerNavbar = $('#innerNavBar');
				var button = $('ul.navbar-nav li');

				navbar.removeClass('custom-collapsed'); // set standart view
				innerNavbar.addClass('container');  //size according to bootstrap

				if (Math.floor(navbar.innerHeight()) > button.height() + 1) // check if we've got 2 lines
				{
					innerNavbar.removeClass('container');
					navbar.addClass('custom-collapsed'); // force collapse mode
				}
			}

			$(document).on('ready', autocollapse);
			$(window).on('resize', autocollapse);
		},


		//Activating tooltip where available
		ActivateTooltip: function () {
			$('.qtip-tooltip')
				.each(function () {

					var content = {
						title: $(this).attr('tooltip-title'),
						text: $(this).attr('tooltip-text')
					};

					var attr = $(this).attr('title');
					if (typeof attr !== 'undefined' && attr !== false) {
						content = {
							text: function () {
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

Teleopti.MyTimeWeb.Common.LoadingOverlay = (function ($) {

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
				'height': '100%'
			})
			.addClass(options.loadingClass)
			.addClass('overlay')
			.appendTo(options.element)
			.show()
		;
	}

	function _removeOverlay(element) {
		$('.overlay', element).remove();
	}

	return {
		Add: function (optionsOrElement) {
			_addOverlay(optionsOrElement);
		},
		Remove: function (element) {
			_removeOverlay(element);
		}
	};

})(jQuery);

String.prototype.format = function () {
	var args = arguments;
	return this.replace(/{(\d+)}/g, function (match, number) {
		return typeof args[number] != 'undefined'
				? args[number]
				: match
		;
	});
};

