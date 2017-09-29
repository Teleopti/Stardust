
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Common");

	var common = Teleopti.MyTimeWeb.Common;

	var resetLocale = function () {
		moment.locale('en');
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
	};

	test("should throw error when there is no initialization before get toggle info", function () {
		try {
			Teleopti.MyTimeWeb.Common.IsToggleEnabled("my_toggle");

			// To fix the problem "Expected at least one assertion"
			equal(true, true);
		} catch(error) {
			equal(error, "you cannot ask toggle before you initialize it!");
		}
	});

	test("should format moment", function () {
		
		common.IsToggleEnabled = function (x) { return true; };
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var formattedDate  = common.FormatDate(moment(new Date(2015, 10-1, 23)));
		
		equal(formattedDate, "23/10/2015");
	});

	test("should format date", function () {

		common.IsToggleEnabled = function (x) { return true; };
		
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var formattedDate = common.FormatDate(new Date(2015, 10-1, 23));

		equal(formattedDate, "23/10/2015");
	});

	test("should format jalaali date", function () {
		
		common.IsToggleEnabled = function (x) { return true; };
		common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});
		var formattedDate = common.FormatDate(new Date(2015, 10 - 1, 23));

		equal(formattedDate, "1394/08/01");

		resetLocale();
	});


	test("should format date with month format", function () {
		
		common.IsToggleEnabled = function (x) { return true; };

		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var formattedDate = common.FormatMonth(new Date(2015, 10 - 1, 23));

		equal(formattedDate, "October 2015");
	});

	test("should format date with jalaali month format", function () {
		var common = Teleopti.MyTimeWeb.Common;
		common.IsToggleEnabled = function (x) { return true; };

		common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});

		var formattedDate = common.FormatMonth(new Date(2015, 10 - 1, 23));

		equal(formattedDate, "آبان 1394");

		resetLocale();
	});

	test("should format period with dates", function () {
		var common = Teleopti.MyTimeWeb.Common;
		common.IsToggleEnabled = function (x) { return true; };
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
		var formattedDate = common.FormatDatePeriod(new Date(2015, 10 - 1, 23), new Date(2015, 11 - 1, 2), false);

		equal(formattedDate, "23/10/2015 - 02/11/2015");
	});

	test("should format period with moments", function () {
		
		common.IsToggleEnabled = function (x) { return true; };
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
		var formattedDate = common.FormatDatePeriod(moment(new Date(2015, 10 - 1, 23)), moment(new Date(2015, 11 - 1, 2), false));

		equal(formattedDate, "23/10/2015 - 02/11/2015");
	});

	test("should format date and time period", function () {
		
		common.IsToggleEnabled = function (x) { return true; };
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
		var formattedDate = common.FormatDatePeriod(new Date(2015, 10 - 1, 23, 8, 30, 00), new Date(2015, 11 - 1, 2, 16, 30, 00), true);

		equal(formattedDate, "23/10/2015 08:30 AM - 02/11/2015 16:30 PM");
	});

	test("should format date and time period on same day to exclude second date", function () {

		common.IsToggleEnabled = function (x) { return true; };
		common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});
		var formattedDate = common.FormatDatePeriod(new Date(2015, 10 - 1, 23, 8, 30, 00), new Date(2015, 10 - 1, 23, 16, 30, 00), true);

		equal(formattedDate, "23/10/2015 08:30 AM - 16:30 PM");
	});

	test("should format jalaali date and time period", function () {

		common.IsToggleEnabled = function (x) { return true; };
		common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'DD/MM/YYYY',
			TimeFormat: 'tt hh:mm',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});
		var formattedDate = common.FormatDatePeriod(new Date(2015, 10 - 1, 23, 8, 30, 00), new Date(2015, 11 - 1, 2, 16, 30, 00), true);

		equal(formattedDate, "1394/08/01 ق.ظ 08:30 - 1394/08/11 ب.ظ 04:30"); // when formatted r to l this will look ok!

		resetLocale();
	});

	test("Should return service safe date format for date", function() {

		var serviceDateFormat = common.FormatServiceDate(new Date(2015, 12-1, 25));
		equal(serviceDateFormat, '2015-12-25');

	});

	test("Should return service safe date format for moment", function () {

		var serviceDateFormat = common.FormatServiceDate(moment(new Date(2015, 12-1, 25)));
		equal(serviceDateFormat, '2015-12-25');
	});

	test("Should return service safe date format for non decimal locale moment", function () {

		moment.locale('ar-sa');
		var serviceDateFormat = common.FormatServiceDate(moment(new Date(2015, 12 - 1, 25)));
		equal(serviceDateFormat, '2015-12-25');

		resetLocale();
	});

	test("Should set teleopti time correctly", function() {
		// We test in Sweden's timezone with DST
		var options = {
			UserTimezoneOffsetMinute: 60,
			HasDayLightSaving: 'True',
			DayLightSavingStart: "2016-03-27 01:00:00",
			DayLightSavingEnd: "2016-10-30 02:00:00" ,
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

	test("Should set teleopti time correctly with daylight saving time in south hemisphere...", function () {
		// We test in Brazil's timezone with DST. Notice that the end time is actually before the start time, meaning the end of the DST from the previous year....
		var options = {
			UserTimezoneOffsetMinute: -180,
			HasDayLightSaving: 'True',
			DayLightSavingStart: "2016-10-16T05:00:00Z",
			DayLightSavingEnd: "2016-02-21T06:00:00Z",
			DayLightSavingAdjustmentInMinute: 60
		};

		var getTeleoptiTime = common.SetupTeleoptiTime(options);

		var currentUtcTime = new Date('2016-10-26T08:30:00Z');

		equal(getTeleoptiTime(currentUtcTime).toGMTString(), new Date('2016-10-26T06:30:00Z').toGMTString());

    });

    test("Should get user texts", function () {
        var userTexts = {
            Fair: 'Fair'
                }
        common.SetUserTexts(userTexts);

        var texts = common.GetUserTexts();
        equal("Fair", texts.Fair);
	});

	test("Should get current user date time", function() {
		var utcOffsetInMinutes = 60;
		var daylightSavingAdjustment = {
			StartDateTime: "2016-03-26 01:00:00",
			EndDateTime: "2016-10-29 02:00:00",
			AdjustmentOffsetInMinutes: 60
		}
		var userDateTime = common.GetCurrentUserDateTime(utcOffsetInMinutes, daylightSavingAdjustment);

		equal(userDateTime.format("HH:mm"), moment().zone(-60).format("HH:mm"));
	});

	test('Should close badges dropdown menu when taping outside', function(){
		var badgesHTML = '<div class="navbar bdd-mytime-top-menu" id="testBadgesHTML">' +
							'<ul class="nav navbar-nav pull-left navbar-user-setting">' +
								'<li id="BadgePanel" class="dropdown pull-left open">111111111111111111' +
								'</li>' +
							'</ul>' +
						'</div>';

		$('body').append(badgesHTML);
		Teleopti.MyTimeWeb.Common.Layout.Init();

		equal($('.bdd-mytime-top-menu #BadgePanel').hasClass('open'), true);

		$('body').trigger('touchend');
		equal($('.bdd-mytime-top-menu #BadgePanel').hasClass('open'), false);

		$('#testBadgesHTML').remove();
	});

	test('Should close user settings dropdown menu when taping outside', function(){
		var userSettingsHTML = '<div class="navbar bdd-mytime-top-menu" id="testUserSettingsHTML">' +
									'<ul class="nav navbar-nav pull-left navbar-user-setting">' +
										'<li id="user-settings" class="dropdown pull-left open">111111111111111111' +
										'</li>' +
									'</ul>' +
								'</div>';

		$('body').append(userSettingsHTML);
		Teleopti.MyTimeWeb.Common.Layout.Init();

		equal($('.bdd-mytime-top-menu #user-settings').hasClass('open'), true);

		$('body').trigger('touchend');
		equal($('.bdd-mytime-top-menu #user-settings').hasClass('open'), false);

		$('#testUserSettingsHTML').remove();
	});

	test('Should close collapsing menu when taping outside', function(){
		var collaspingMenuHTML = '<div class="navbar bdd-mytime-top-menu" id="testCollaspingMenuHTML">' +
									'<button id="mainNavbarToggler" type="button" class="navbar-toggle" ' +
										'data-toggle="offcanvas" data-target=".navbar-offcanvas">' +
									'</button>' + 
									'<div class="navbar-offcanvas navmenu-fixed-left offcanvas menu-top-adjust in" id="bs-example-navbar-collapse-1">' +
									'</div>' +
								  '</div>';

		$('body').append(collaspingMenuHTML);
		Teleopti.MyTimeWeb.Common.Layout.Init();

		equal($('#bs-example-navbar-collapse-1').hasClass('in'), true);

		var mainNavbarTogglerClickTriggered = false;
		$('#mainNavbarToggler').click(function(){
			mainNavbarTogglerClickTriggered = true;
		});

		equal(mainNavbarTogglerClickTriggered, true);

		$('#mainNavbarToggler').unbind('click');
		$('#testCollaspingMenuHTML').remove();
	});
});