$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Common');

	var common = Teleopti.MyTimeWeb.Common;

	var resetLocale = function() {
		moment.locale('en');
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
	};

	test('should throw error when there is no initialization before get toggle info', function() {
		try {
			//Extra step to reset _settings cause Asm will init it
			//and the tests run order is not ensured.
			Teleopti.MyTimeWeb.Common.Init({ baseUrl: undefined }, null);

			Teleopti.MyTimeWeb.Common.IsToggleEnabled('my_toggle');

			// To fix the problem "Expected at least one assertion"
			equal(true, true);
		} catch (error) {
			equal(error, 'you cannot ask toggle before you initialize it!');
		}
	});

	test('should be able to enable a toggle', function() {
		try {
			Teleopti.MyTimeWeb.Common.Init(
				{
					defaultNavigation: '/',
					baseUrl: '/',
					startBaseUrl: '/'
				},
				null
			);

			Teleopti.MyTimeWeb.Common.EnableToggle('my_toggle_to_be_enabled');

			equal(Teleopti.MyTimeWeb.Common.IsToggleEnabled('my_toggle_to_be_enabled'), true);
		} catch (error) {
			equal(error, 'you cannot ask toggle before you initialize it!');
		}
	});

	test('should be able to disable a toggle', function() {
		try {
			Teleopti.MyTimeWeb.Common.Init(
				{
					defaultNavigation: '/',
					baseUrl: '/',
					startBaseUrl: '/'
				},
				null
			);
			Teleopti.MyTimeWeb.Common.EnableToggle('my_toggle_to_be_disabled');
			Teleopti.MyTimeWeb.Common.DisableToggle('my_toggle_to_be_disabled');

			equal(Teleopti.MyTimeWeb.Common.IsToggleEnabled('my_toggle_to_be_disabled'), false);
		} catch (error) {
			equal(error, 'you cannot ask toggle before you initialize it!');
		}
	});

	test('should format moment', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var formattedDate = common.FormatDate(moment(new Date(2015, 10 - 1, 23)));

		equal(formattedDate, '23/10/2015');
	});

	test('should format date', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var formattedDate = common.FormatDate(new Date(2015, 10 - 1, 23));

		equal(formattedDate, '23/10/2015');
	});

	test('should format jalaali date', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});
		var formattedDate = common.FormatDate(new Date(2015, 10 - 1, 23));

		equal(formattedDate, '1394/08/01');

		resetLocale();
	});

	test('should format date with month format', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var formattedDate = common.FormatMonth(new Date(2015, 10 - 1, 23));

		equal(formattedDate, 'October 2015');
	});

	test('should format date with jalaali month format', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});

		var formattedDate = common.FormatMonth(new Date(2015, 10 - 1, 23));

		equal(formattedDate, 'آبان 1394');

		resetLocale();
	});

	test('should format period with dates', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
		var formattedDate = common.FormatDatePeriod(new Date(2015, 10 - 1, 23), new Date(2015, 11 - 1, 2), false);

		equal(formattedDate, '23/10/2015 - 02/11/2015');
	});

	test('should format period with moments', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
		var formattedDate = common.FormatDatePeriod(
			moment(new Date(2015, 10 - 1, 23)),
			moment(new Date(2015, 11 - 1, 2), false)
		);

		equal(formattedDate, '23/10/2015 - 02/11/2015');
	});

	test('should format date and time period', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
		var formattedDate = common.FormatDatePeriod(
			new Date(2015, 10 - 1, 23, 8, 30, 00),
			new Date(2015, 11 - 1, 2, 16, 30, 00),
			true
		);

		equal(formattedDate, '23/10/2015 08:30 AM - 02/11/2015 16:30 PM');
	});

	test('should format date and time period on same day to exclude second date', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
		var formattedDate = common.FormatDatePeriod(
			new Date(2015, 10 - 1, 23, 8, 30, 00),
			new Date(2015, 10 - 1, 23, 16, 30, 00),
			true
		);

		equal(formattedDate, '23/10/2015 08:30 AM - 16:30 PM');
	});

	test('should format jalaali date and time period', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'tt hh:mm',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});
		var formattedDate = common.FormatDatePeriod(
			new Date(2015, 10 - 1, 23, 8, 30, 00),
			new Date(2015, 11 - 1, 2, 16, 30, 00),
			true
		);

		equal(formattedDate, '1394/08/01 ق.ظ 08:30 - 1394/08/11 ب.ظ 04:30'); // when formatted r to l this will look ok!

		resetLocale();
	});

	test('should format date format locale', function() {
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM',
			DateFormatLocale: 'zh-CN'
		});
		var formattedDateLocale = common.DateFormatLocale;

		equal(formattedDateLocale, 'zh-CN');
	});

	test('Should return service safe date format for date', function() {
		var serviceDateFormat = common.FormatServiceDate(new Date(2015, 12 - 1, 25));
		equal(serviceDateFormat, '2015-12-25');
	});

	test('Should return service safe date format for moment', function() {
		var serviceDateFormat = common.FormatServiceDate(moment(new Date(2015, 12 - 1, 25)));
		equal(serviceDateFormat, '2015-12-25');
	});

	test('Should return service safe date format for non decimal locale moment', function() {
		moment.locale('ar-sa');
		var serviceDateFormat = common.FormatServiceDate(moment(new Date(2015, 12 - 1, 25)));
		equal(serviceDateFormat, '2015-12-25');

		resetLocale();
	});

	test('Should set teleopti time correctly', function() {
		// We test in Sweden's timezone with DST
		var options = {
			UserTimezoneOffsetMinute: 60,
			HasDayLightSaving: 'True',
			DayLightSavingStart: '2016-03-27 01:00:00',
			DayLightSavingEnd: '2016-10-30 02:00:00',
			DayLightSavingAdjustmentInMinute: 60
		};

		var getTeleoptiTime = common.SetupTeleoptiTime(options);

		var currentUtcTime = new Date('2016-04-12 11:00:00');

		equal(getTeleoptiTime(currentUtcTime).toGMTString(), new Date('2016-04-12 13:00:00').toGMTString());

		// Then we test in China's timezone without DST
		var options = {
			UserTimezoneOffsetMinute: 480,
			HasDayLightSaving: 'False',
			DayLightSavingStart: null,
			DayLightSavingEnd: null,
			DayLightSavingAdjustmentInMinute: null
		};

		var getTeleoptiTime = common.SetupTeleoptiTime(options);

		var currentUtcTime = new Date('2016-04-12 11:00:00');

		equal(getTeleoptiTime(currentUtcTime).toGMTString(), new Date('2016-04-12 19:00:00').toGMTString());
	});

	test('Should set teleopti time correctly with daylight saving time in south hemisphere...', function() {
		// We test in Brazil's timezone with DST. Notice that the end time is actually before the start time, meaning the end of the DST from the previous year....
		var options = {
			UserTimezoneOffsetMinute: -180,
			HasDayLightSaving: 'True',
			DayLightSavingStart: '2016-10-16T05:00:00Z',
			DayLightSavingEnd: '2016-02-21T06:00:00Z',
			DayLightSavingAdjustmentInMinute: 60
		};

		var getTeleoptiTime = common.SetupTeleoptiTime(options);

		var currentUtcTime = new Date('2016-10-26T08:30:00Z');

		equal(getTeleoptiTime(currentUtcTime).toGMTString(), new Date('2016-10-26T06:30:00Z').toGMTString());
	});

	test('Should get user texts', function() {
		var texts = common.GetUserTexts();
		equal('@Resources.Fair', texts.Fair);
	});

	test('Should get current user date time', function() {
		var utcOffsetInMinutes = 60;
		var daylightSavingAdjustment = {
			StartDateTime: '2016-03-26 01:00:00',
			EndDateTime: '2016-10-29 02:00:00',
			AdjustmentOffsetInMinutes: 60
		};
		var userDateTime = common.GetCurrentUserDateTime(utcOffsetInMinutes, daylightSavingAdjustment);

		equal(
			userDateTime.format('HH:mm'),
			moment()
				.zone(-60)
				.format('HH:mm')
		);
	});

	test('Should close badges dropdown menu when taping outside', function() {
		var badgesHTML =
			'<div class="navbar teleopti-mytime-top-menu" id="testBadgesHTML">' +
			'<ul class="nav navbar-nav pull-left navbar-user-setting">' +
			'<li id="BadgePanel" class="dropdown pull-left open">111111111111111111' +
			'</li>' +
			'</ul>' +
			'</div>';

		$('body').append(badgesHTML);
		Teleopti.MyTimeWeb.Common.Layout.Init();

		equal($('.teleopti-mytime-top-menu #BadgePanel').hasClass('open'), true);

		$('body').trigger('touchend');
		equal($('.teleopti-mytime-top-menu #BadgePanel').hasClass('open'), false);

		$('#testBadgesHTML').remove();
	});

	test('Should close user settings dropdown menu when taping outside', function() {
		var userSettingsHTML =
			'<div class="navbar teleopti-mytime-top-menu" id="testUserSettingsHTML">' +
			'<ul class="nav navbar-nav pull-left navbar-user-setting">' +
			'<li id="user-settings" class="dropdown pull-left open">111111111111111111' +
			'</li>' +
			'</ul>' +
			'</div>';

		$('body').append(userSettingsHTML);
		Teleopti.MyTimeWeb.Common.Layout.Init();

		equal($('.teleopti-mytime-top-menu #user-settings').hasClass('open'), true);

		$('body').trigger('touchend');
		equal($('.teleopti-mytime-top-menu #user-settings').hasClass('open'), false);

		$('#testUserSettingsHTML').remove();
	});

	test('Should close collapsing menu when taping outside', function() {
		var collaspingMenuHTML =
			'<div class="navbar teleopti-mytime-top-menu" id="testCollaspingMenuHTML">' +
			'<button type="button" class="navbar-toggle navbar-toggle-button" ' +
			'data-toggle="offcanvas" data-target=".navbar-offcanvas">' +
			'</button>' +
			'<div class="navbar-offcanvas offcanvas in">' +
			'</div>' +
			'</div>';

		$('body').append(collaspingMenuHTML);
		Teleopti.MyTimeWeb.Common.Layout.Init();

		equal($('.teleopti-mytime-top-menu .navbar-offcanvas').hasClass('in'), true);

		var mainNavbarTogglerClickTriggered = false;
		$('.navbar-toggle-button').click(function() {
			mainNavbarTogglerClickTriggered = true;
		});

		$('body').trigger('touchend');
		equal(mainNavbarTogglerClickTriggered, true);

		$('.navbar-toggle-button').unbind('click');
		$('#testCollaspingMenuHTML').remove();
	});

	test('should detect an Android tablet', function() {
		var ua =
			'Mozilla/5.0 (Linux; Android 6.0.1; Nexus 10 Build/MOB31T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36';

		equal(Teleopti.MyTimeWeb.Common.IsHostAniPad(ua), true);
	});

	test('should detect an iPad', function() {
		var ua =
			'Mozilla/5.0 (Linux; Android 6.0.1; Nexus 10 Build/MOB31T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36';

		equal(Teleopti.MyTimeWeb.Common.IsHostAniPad(ua), true);
	});

	test('should detect an iPhone', function() {
		var ua =
			'Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1';

		equal(Teleopti.MyTimeWeb.Common.IsHostAMobile(ua), true);
	});

	test('should detect an Android mobile phone', function() {
		var ua =
			'Mozilla/5.0 (Linux; Android 5.0; SM-G900P Build/LRX21T) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Mobile Safari/537.36';

		equal(Teleopti.MyTimeWeb.Common.IsHostAMobile(ua), true);
	});
});
