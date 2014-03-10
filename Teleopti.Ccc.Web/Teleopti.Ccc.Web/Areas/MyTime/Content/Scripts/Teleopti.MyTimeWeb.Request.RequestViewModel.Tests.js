
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
	
	test("should read account values in days", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readAbsenceAccount({
			PeriodStart: moment("2015-01-01"),
			PeriodEnd: moment("2015-12-31"),
			TrackerType: "Days",
			Remaining: "15",
			Used: "2"
		});

		equal(vm.AbsenceAccountExists(), true);
		equal(vm.AbsenceAccountPeriodStart().isSame(moment("2015-01-01")), true),
		equal(vm.AbsenceAccountPeriodEnd().isSame(moment("2015-12-31")), true),
		equal(vm.AbsenceTrackedAsDay(), true);
		equal(vm.AbsenceTrackedAsHour(), false);
		equal(vm.AbsenceUsed(), '2');
		equal(vm.AbsenceRemaining(), '15');
	});

	test("should read account values in hours", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readAbsenceAccount({
			PeriodStart: moment("2015-01-01"),
			PeriodEnd: moment("2015-12-31"),
			TrackerType: "Hours",
			Remaining: "18:00",
			Used: "2:00"
		});

		equal(vm.AbsenceAccountExists(), true);
		equal(vm.AbsenceAccountPeriodStart().isSame(moment("2015-01-01")), true),
		equal(vm.AbsenceAccountPeriodEnd().isSame(moment("2015-12-31")), true),
		equal(vm.AbsenceTrackedAsDay(), false);
		equal(vm.AbsenceTrackedAsHour(), true);
		equal(vm.AbsenceUsed(), '2:00');
		equal(vm.AbsenceRemaining(), '18:00');
	});

	test("should read with null account", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readAbsenceAccount();

		equal(vm.AbsenceAccountExists(), false);
		equal(vm.AbsenceAccountPeriodStart().isSame(moment()), true),
		equal(vm.AbsenceAccountPeriodEnd().isSame(moment()), true),
		equal(vm.AbsenceTrackedAsDay(), false);
		equal(vm.AbsenceTrackedAsHour(), false);
		equal(vm.AbsenceUsed(), '0');
		equal(vm.AbsenceRemaining(), '0');
	});
});