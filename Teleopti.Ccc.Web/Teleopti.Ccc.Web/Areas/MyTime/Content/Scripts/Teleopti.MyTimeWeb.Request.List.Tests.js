
$(document).ready(function() {

	module("Teleopti.MyTimeWeb.Request.List");

	var getData = function(from, to , isFullDay, isShiftTrade) {

		return {
			DateTimeFrom: from,
			DateTimeTo: to,
			UpdatedOnDateTime: "2014-05-01 10:00",
			IsFullDay: isFullDay,
			TypeEnum: isShiftTrade ? 2 : 1,
			Text: 'test',
			Link: {
				href: 'http://erehwon.com',
				Methods: ["DELETE", "PUT"]
			}
		};
	};
	
	var setupStandardLocaleAndCalendar = function() {
		moment.locale('en');

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

	};

	var standardSetup = function() {
		setupStandardLocaleAndCalendar();
		return Teleopti.MyTimeWeb.Request.List.GetRequestItemViewModel();
	};

	var jalaaliCalendarSetup = function () {

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});

		return Teleopti.MyTimeWeb.Request.List.GetRequestItemViewModel();
	};

	
	test("should show start and end dates without time for full day request", function () {
		var vm = standardSetup();
		var data = getData("2014-05-14 08:00:00", "2014-05-15 17:00:00", true, false);

		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14 - 2014/05/15");
	});

	test("should show start and end date with times for non-full day requests on a single day", function () {
		var vm = standardSetup();
		var data = getData("2014-05-14 08:00:00", "2014-05-14 17:00:00", false, false);

		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14 08:00 AM - 17:00 PM");
	});
	
	test("should show start and end date with times for non-full day requests on multiple days", function () {
		var vm = standardSetup();
		var data = getData("2014-05-14 08:00:00", "2014-05-15 17:00:00", false, false);

		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14 08:00 AM - 2014/05/15 17:00 PM");
	});

	test("should show a single date for shift trade requests starting and finishing on same day", function () {
		var vm = standardSetup();
		var data = getData("2014-05-14 08:00:00", "2014-05-14 17:00:00", false, true);
		
		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14");
	});

	test("should show a single date for full day (non-shift trade) requests starting and finishing on same day", function () {
		var vm = standardSetup();
		var data = getData("2014-05-14 00:00:00", "2014-05-14 23:59:00", true, false);

		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14");
	});

	test("should show start and end dates for shift trade requests starting and finishing on different days", function () {
		var vm = standardSetup();
		var data = getData("2014-05-14 08:00:00", "2014-05-15 17:00:00", false, true);
		
		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14 - 2014/05/15");
	});

	test("should display jalaali dates", function () {
		var vm = jalaaliCalendarSetup();
		var data = getData("2014-05-14", "2014-05-21", false, true);

		vm.Initialize(data, false);

		equal(Teleopti.MyTimeWeb.Common.FormatDate(vm.StartDateTime()),"1393/02/24");
		equal(Teleopti.MyTimeWeb.Common.FormatDate(vm.EndDateTime()), "1393/02/31");
		
		setupStandardLocaleAndCalendar(); 
	});

	test("should display date time periods correctly, with no utc conversion", function () {
		var vm = standardSetup();
		var data = getData("2014-05-14 8:00", "2014-05-14 17:00", false, false);

		vm.Initialize(data, false);

		equal(vm.StartDateTime().hour(),8);
		equal(vm.EndDateTime().hour(),17);
	});
	

});