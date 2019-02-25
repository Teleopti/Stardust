
$(document).ready(function () {

	var _hash = '';
	module('Teleopti.MyTimeWeb.Schedule.MonthViewModel', {
		setup: function () {
			setup();
		},
		teardown: function () {
			_hash = '';
		}
	});

	function setup() {
		this.hasher = {
			initialized: {
				add: function () { }
			},
			changed: {
				add: function () { }
			},
			init: function () { },
			setHash: function (data) {
				_hash = data;
			}
		};
	}
	
	test("should change text color based on background color", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		vm.readData({ScheduleDays: [{Shift: {Color: "rgb(0,0,0)"}}]});
		equal(vm.weekViewModels()[0].dayViewModels()[0].shiftTextColor, "white");
		
		vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		vm.readData({ ScheduleDays: [{ Shift: { Color: "rgb(255,255,255)" } }] });
		equal(vm.weekViewModels()[0].dayViewModels()[0].shiftTextColor, "black");

		vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		vm.readData({ ScheduleDays: [{ Shift: { Color: "rgb(0,128,0)" } }] });
		equal(vm.weekViewModels()[0].dayViewModels()[0].shiftTextColor, "white");
	});
	
	test("should read absence data", function () {

		var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

		viewModelMonth.readData({
			ScheduleDays: [{
				Absences: [{
					Name: "Illness",
					ShortName: "IL",
					IsFullDayAbsence: true
				}]
			}]
		});

		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].absenceName, "Illness");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].absenceShortName, "IL");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].isFullDayAbsence, true);
	});
	
	test("should read shift data", function () {

		var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

		viewModelMonth.readData({
			ScheduleDays: [{
				Shift: {
					Name: "Early",
					ShortName: "AM",
					TimeSpan: "09:00-18:00",
					WorkingHours: "8:00",
					Color: "green"
				}
			}]
		});

		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftName, "Early");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftShortName, "AM");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftTimeSpan, "09:00-18:00");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftWorkingHours, "8:00");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftColor, "green");
	});

	test("should read dayoff data", function () {

		var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

		viewModelMonth.readData({
			ScheduleDays: [{
				IsDayOff: true
			}]
		});

		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].isDayOff, true);
	});
	
	test("should indicate absence", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		vm.readData({
			ScheduleDays: [{}]
		});
		equal(vm.weekViewModels()[0].dayViewModels()[0].hasAbsence, false);

		vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		vm.readData({
			ScheduleDays: [{ Absences: [{ Name: "Illness" }] }]
		});
		equal(vm.weekViewModels()[0].dayViewModels()[0].hasAbsence, true);
	});

	test("should indicate shift", function () {
		var vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		vm.readData({
			ScheduleDays: [{}]
		});
		equal(vm.weekViewModels()[0].dayViewModels()[0].hasShift, false);

		vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		vm.readData({
			ScheduleDays: [{ Shift: { Name: "Late" } }]
		});
		equal(vm.weekViewModels()[0].dayViewModels()[0].hasShift, true);
	});

	test("should read scheduled days", function () {

	    var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

	    viewModelMonth.readData({
	        ScheduleDays: [{
	        }]
	    });

	    equal(viewModelMonth.weekViewModels()[0].dayViewModels().length, 1);
	});

	test("should go to today when from month view to week view without any date chosen", function() {
		var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		var dayView = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel( { FixedDate: undefined }, moment());
		dayView.currentDate = undefined;

		viewModelMonth.week(dayView);

		equal(_hash, 'Schedule/Week');
	});

	test('should indicate bankholiday', function() {
		var vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

		vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
		vm.readData({
			ScheduleDays: [
				{
					BankHolidayCalendar: {
						CalendarId: 'c50b3a81-dc8a-44ce-820d-08c6f0e798fe',
						CalendarName: 'China Bank Holiday',
						DateDescription: 'A bank holiday calendar'
					}
				}
			]
		});
		equal(vm.weekViewModels()[0].dayViewModels()[0].bankHoliday.calendarId, 'c50b3a81-dc8a-44ce-820d-08c6f0e798fe');
		equal(vm.weekViewModels()[0].dayViewModels()[0].bankHoliday.calendarName, 'China Bank Holiday');
		equal(vm.weekViewModels()[0].dayViewModels()[0].bankHoliday.dateDescription, 'A bank holiday calendar');
	});
});