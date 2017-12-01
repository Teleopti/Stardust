$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Request.RequestViewModel");

	test("should read personal account permissions", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		vm.readPersonalAccountPermission(true);

		equal(vm.PersonalAccountPermission(), true);
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

		var today = moment().startOf('day');
		equal(vm.AbsenceAccountExists(), false);
		equal(vm.AbsenceAccountPeriodStart().isSame(today), true),
		equal(vm.AbsenceAccountPeriodEnd().isSame(today), true),
		equal(vm.AbsenceTrackedAsDay(), false);
		equal(vm.AbsenceTrackedAsHour(), false);
		equal(vm.AbsenceUsed(), '0');
		equal(vm.AbsenceRemaining(), '0');
	});

	test("could tolerate invalid date hash in url", function() {
		var old = Teleopti.MyTimeWeb.Portal.ParseHash;
		Teleopti.MyTimeWeb.Portal.ParseHash = function() {
			return {
				dateHash: "20170101"
			}
		}
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();
		Teleopti.MyTimeWeb.Portal.ParseHash = old;
		equal(vm.DateTo().isValid(), true);
	});

	test("should check required subject for new request", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();

		vm.Subject('');
		vm.checkSubject();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'Missing subject');
	});

	test("should validate date from input for new request", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-21'));
		vm.TimeFrom('08:00');
		vm.TimeTo('08:01');

		vm.AddRequest();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'End time must be greater than start time');
	});

	test("should validate date to input for new request", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-21'));
		vm.TimeFrom('08:00');
		vm.TimeTo('08:01');

		vm.AddRequest();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'End time must be greater than start time');
	});

	test("should validate illegal time from input for new request on same day", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-22'));
		vm.TimeFrom('08:00');
		vm.TimeTo('08:00');

		vm.AddRequest();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'End time must be greater than start time');
	});

	test("should validate legal time from input for new request on same day", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-22'));
		vm.TimeFrom('08:00');
		vm.TimeTo('08:01');

		vm.AddRequest();

		equal(vm.ShowError(), false);
		equal(vm.ErrorMessage(), '');
	});

	test("should validate illegal time to input for new request on same day", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-22'));
		vm.TimeFrom('08:00');
		vm.TimeTo('08:00');

		vm.AddRequest();

		equal(vm.ShowError(), true);
		equal(vm.ErrorMessage(), 'End time must be greater than start time');
	});

	test("should validate legal time to input for new request on same day", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-22'));
		vm.TimeFrom('08:00');
		vm.TimeTo('08:01');

		vm.AddRequest();

		equal(vm.ShowError(), false);
		equal(vm.ErrorMessage(), '');
	});

	test("should validate legal time to input for new request on same day with US time format", function () {
		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel();

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-22'));
		vm.TimeFrom('12:00 AM');
		vm.TimeTo('11:59 PM');

		vm.AddRequest();

		equal(vm.ShowError(), false);
		equal(vm.ErrorMessage(), '');
	});

	test("should trigger absence account reload when absence is change", function () {
		var ajaxTemp = Teleopti.MyTimeWeb.Ajax;

		var absenceAccountReloaded = false;
		Teleopti.MyTimeWeb.Ajax = function(){
			return {
				Ajax: function(option){
					if (option.url == 'Requests/FetchAbsenceAccount'){
						absenceAccountReloaded = true;
					}
				}
			}
		}

		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel(null, null, null, {
			defaultFulldayStartTime: moment('2017-11-22').add(8, 'hours'),
			defaultFulldayEndTime: moment('2017-11-22').add(17, 'hours')
		});

		vm.onLoadComplete();

		vm.PreviousAbsenceId('oldId');
		vm.AbsenceId('newId');

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-22'));
		vm.IsFullDay(true);

		equal(absenceAccountReloaded, true);

		Teleopti.MyTimeWeb.Ajax = ajaxTemp;
	});

	test("should not trigger absence account reload when date to is before date from", function () {
		var ajaxTemp = Teleopti.MyTimeWeb.Ajax;

		var absenceAccountReloaded = false;
		Teleopti.MyTimeWeb.Ajax = function(){
			return {
				Ajax: function(option){
					if (option.url == 'Requests/FetchAbsenceAccount')
						absenceAccountReloaded = true;
				}
			}
		}

		var vm = new Teleopti.MyTimeWeb.Request.RequestViewModel(null, null, null, {
			defaultFulldayStartTime: moment('2017-11-22').add(8, 'hours'),
			defaultFulldayEndTime: moment('2017-11-22').add(17, 'hours')
		});

		vm.onLoadComplete();

		vm.Subject('subject');
		vm.DateFrom(moment('2017-11-22'));
		vm.DateTo(moment('2017-11-21'));
		vm.IsFullDay(true);

		equal(absenceAccountReloaded, false);

		Teleopti.MyTimeWeb.Ajax = ajaxTemp;
	});
});