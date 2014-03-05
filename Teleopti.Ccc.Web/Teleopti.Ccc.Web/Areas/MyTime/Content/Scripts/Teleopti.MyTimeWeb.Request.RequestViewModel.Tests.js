
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
	
	test("should read account values", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readAbsences({
			AbsenceAccounts: [{
				AbsenceName: "Vacation",
				StartDate: moment('2014-01-01 00:00:00'),
				EndDate: moment('2014-12-31 23:59:59'),
				Used: '23',
				Remaining: '2'
			}]
		});

		var dateFormat = "YYYY-MM-DD HH:mm:ss";
		var account = vm.AbsenceAccounts()[0];
		equal(account.AbsenceName, "Vacation");
		equal(account.StartDate.format(dateFormat), '2014-01-01 00:00:00');
		equal(account.EndDate.format(dateFormat), '2014-12-31 23:59:59');
		equal(account.Used, '23');
		equal(account.Remaining, '2');
	});
});