
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Schedule.MonthViewModel");

	test("should read absence data", function () {
	    
		var viewModelMonth = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();

		viewModelMonth.readData({
		    Date: "2014-02-11",
		    ScheduleDays: [{ Date: "2014-02-11",Absence:"Illness" }]
		});

		equal(viewModelMonth.weekViewModels()[0].dayViewModels()[0].absence, "Illness");
	});

});