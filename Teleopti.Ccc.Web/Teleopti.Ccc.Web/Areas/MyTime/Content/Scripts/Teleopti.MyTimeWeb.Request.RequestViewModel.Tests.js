
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.RequestViewModel");

	test("should read absences", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readAbsences({
			AbsenceTypes: [{
			Id: "{7F546993-2760-44DD-90AF-0D1BB8E42428}",
			Name: "Vacation",
		}]});

		equal(vm.Absences()[0].Id, "{7F546993-2760-44DD-90AF-0D1BB8E42428}");
		equal(vm.Absences()[0].Name, "Vacation");
	});

});