
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Schedule.MonthViewModel");
	
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
				Absence: {
					Name: "Illness",
					ShortName: "IL",
					IsFullDayAbsence: true
				}
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
			ScheduleDays: [{ Absence: { Name: "Illness" } }]
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
});