
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
    
	test("should read scheduled days", function () {

	    var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

	    viewModelMonth.readData({
	        ScheduleDays: [{
	        }]
	    });

	    equal(viewModelMonth.weekViewModels()[0].dayViewModels().length, 1);
	});
});