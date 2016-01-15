
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
});