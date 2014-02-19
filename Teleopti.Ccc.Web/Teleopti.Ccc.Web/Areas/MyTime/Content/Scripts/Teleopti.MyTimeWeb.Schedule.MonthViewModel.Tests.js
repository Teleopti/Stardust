
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Schedule.MonthViewModel");
	
	test("should read absence data", function () {

		var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

		viewModelMonth.readData({
			ScheduleDays: [{
				Absence: {
					Name: "Illness",
					ShortName: "IL"
				}
			}]
		});

		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].absenceName, "Illness");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].absenceShortName, "IL");
	});
	
	test("should read shift data", function () {

		var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

		viewModelMonth.readData({
			ScheduleDays: [{
				Shift: {
					Name: "Early",
					StartTime: "09:00",
					EndTime: "18:00",
					WorkingHours: "8:00"
				}
			}]
		});

		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftName, "Early");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftStartTime, "09:00");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftEndTime, "18:00");
		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].shiftWorkingHours, "8:00");
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

	test("should read scheduled days", function () {

	    var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

	    viewModelMonth.readData({
	        ScheduleDays: [{
	        }]
	    });

	    equal(viewModelMonth.weekViewModels()[0].dayViewModels().length, 1);
	});
});