
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

	test("should show start and end dates without time for full day request", function () {

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var vm = Teleopti.MyTimeWeb.Request.List.GetRequestItemViewModel();
		var data = getData("2014-05-14 08:00:00", "2014-05-15 17:00:00", true, false);

		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14 - 2014/05/15");
	});

	test("should show start and end date with times for non-full day requests", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});


		var vm = Teleopti.MyTimeWeb.Request.List.GetRequestItemViewModel();
		var data = getData("2014-05-14 08:00:00", "2014-05-14 17:00:00", false, false);

		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14 08:00 AM - 2014/05/14 17:00 PM");

	});

	test("should show a single date for shift trade requests starting and finishing on same day", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});


		var vm = Teleopti.MyTimeWeb.Request.List.GetRequestItemViewModel();
		var data = getData("2014-05-14 08:00:00", "2014-05-14 17:00:00", false, true);
		
		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14");
	});

	test("should show start and end dates for shift trade requests starting and finishing on different days", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var vm = Teleopti.MyTimeWeb.Request.List.GetRequestItemViewModel();
		var data = getData("2014-05-14 08:00:00", "2014-05-15 17:00:00", false, true);
		
		vm.Initialize(data, false);
		equal(vm.GetDateDisplay(), "2014/05/14 - 2014/05/15");

	});

	test("should display jalaali dates", function () {

		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});

		var vm = Teleopti.MyTimeWeb.Request.List.GetRequestItemViewModel();
		var data = getData("2014-05-14", "2014-05-21", false, true);

		vm.Initialize(data, false);

		equal(Teleopti.MyTimeWeb.Common.FormatDate(vm.StartDateTime()),"1393/02/24");
		equal(Teleopti.MyTimeWeb.Common.FormatDate(vm.EndDateTime()), "1393/02/31");
		
		resetLocale();

	});

	test("should display date time periods correctly, with no utc conversion", function () {
		
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator : 'PM'
		});

		var vm = Teleopti.MyTimeWeb.Request.List.GetRequestItemViewModel();
		var data = getData("/Date(1435276800000)/", "/Date(1435309200000)/", false, false);

		vm.Initialize(data, false);

		equal(vm.StartDateTime().hour(),8);
		equal(vm.EndDateTime().hour(),17);

		resetLocale();
	});
	

});