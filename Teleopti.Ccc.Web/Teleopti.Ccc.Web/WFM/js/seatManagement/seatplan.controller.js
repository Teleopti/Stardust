'use strict';

(function () {

	angular.module('wfm.seatPlan').controller('SeatPlanCtrl', seatPlanDirectiveController);

	seatPlanDirectiveController.$inject = ['ResourcePlannerSvrc', 'seatPlanService', '$stateParams', 'Toggle', '$timeout'];

	function seatPlanDirectiveController(resourcePlannerService, seatPlanService, params, toggleService, $timeout) {

		var vm = this;

		vm.isLoadingCalendar = true;
		vm.isLoadingPlanningPeriods = true;
		vm.planningPeriods = [];

	    vm.isLoadingStuff = function() {
	        return vm.isLoadingCalendar || vm.isLoadingPlanningPeriods;
	    };

		vm.setupSeatPlanStatusStrings = function () {
			vm.seatPlanStatus = {};
			vm.seatPlanStatus[0] = 'SeatPlanStatusOK';
			vm.seatPlanStatus[1] = 'SeatPlanStatusInProgress';
			vm.seatPlanStatus[2] = 'SeatPlanStatusError';
			vm.seatPlanStatus[3] = 'SeatPlanStatusNoSeatPlanned';
		};

		vm.setupToggles = function () {
			vm.showOccupancyView = toggleService.Wfm_SeatPlan_SeatMapBookingView_32814;
		};

		var getServiceSafeDate = function(dateMoment) {

			return dateMoment.locale('en').format("YYYY-MM-DD");
		};

		vm.getPreviousMonthStart = function (dateMoment) {
			return getServiceSafeDate(moment(dateMoment).subtract(1, 'months').startOf('month'));
		};

		vm.getNextMonthEnd = function (dateMoment) {

			return getServiceSafeDate(moment(dateMoment).add(1, 'months').endOf('month'));
		};

		vm.seatPlanStatusClass = ['seatplan-status-success', 'seatplan-status-inprogress', 'seatplan-status-error'];

		vm.loadMonthDetails = function (date) {
			var dateMoment = moment(date);
			vm.isLoadingPlanningPeriods = true;

			vm.currentMonth = dateMoment.month();

			vm.loadCalendarDetails(dateMoment);
			vm.loadPlanningPeriods(dateMoment);
		};

		vm.loadCalendarDetails = function (dateMoment) {

			vm.isLoadingCalendar = true;
			vm.seatPlanDateStatuses = [];

			var seatPlansParams = {
				startDate: vm.getPreviousMonthStart(dateMoment),
				endDate: vm.getNextMonthEnd(dateMoment)
			};

			seatPlanService.seatPlans
				.query(seatPlansParams)
				.$promise.then(function (data) {
					vm.seatPlanDateStatuses = data;
					vm.selectedDate = dateMoment.toDate();
					vm.isLoadingCalendar = false;
				});
		};

		vm.loadPlanningPeriods = function (dateMoment) {
			vm.isLoadingPlanningPeriods = true;

			var planningPeriodParams = {
				startDate: moment(dateMoment).startOf('month').format('YYYY-MM-DD'),
				endDate: moment(dateMoment).endOf('month').format('YYYY-MM-DD')
			};

			resourcePlannerService.getPlanningPeriodsForRange
			   .query(planningPeriodParams)
			   .$promise.then(function (data) {
			       vm.planningPeriods = data;
			       vm.isLoadingPlanningPeriods = false;
				});
		};

		vm.getDateString = function (date) {
			return moment(date).format('LL');
		};

		vm.onChangeOfDate = function () {
			if (vm.currentMonth != moment(vm.selectedDate).month()) {
				vm.loadMonthDetails(moment(vm.selectedDate));
			}
		};

		vm.onChangeOfMonth = function (date) {
			if (vm.isLoadingCalendar) {
				return;
			}

			vm.loadMonthDetails(date);

		};

		vm.onSeatPlanStart = function () {
			vm.isLoadingPlanningPeriods = true;
		};

		vm.onSeatPlanComplete = function () {
			vm.loadMonthDetails(moment(vm.selectedDate));
		};

		vm.showReport = function (period, teams, locations) {
			vm.isReportOpened = !vm.isReportOpened;
			vm.reportPeriod = period;
			vm.reportSelectedTeams = teams;
			vm.reportSelectedLocations = locations;
		};

		vm.showSeatmap = function () {
			vm.isSeatMapBookingViewOpened = !vm.isSeatMapBookingViewOpened;
			if (!vm.isSeatMapBookingViewOpened) {
				vm.loadMonthDetails(moment(vm.selectedDate));
			}
		};


		vm.getSelectedMonthName = function () {
			return moment(vm.selectedDate).format("MMMM");
		};

		vm.getToDayInfo = function () {

			if (vm.isLoadingCalendar) {
				return 'LoadingSeatPlanStatus';
			}

			var dayInfoString = vm.seatPlanStatus[3];

			vm.seatPlanDateStatuses.forEach(function (dateEvent) {

				if (moment(vm.selectedDate).isSame(dateEvent.Date, 'day')) {
					dayInfoString = vm.seatPlanStatus[dateEvent.Status];
				}
			});

			return dayInfoString;
		};

		vm.getDayClass = function (date, mode) {
			if (mode === 'day') {

				var dayClass = '';
				var dayToCheck = moment(date);

				vm.seatPlanDateStatuses.forEach(function (status) {

					if (dayToCheck.isSame(moment(status.Date), 'day')) {
						dayClass = vm.seatPlanStatusClass[status.Status];
					}
				});
			}
			return dayClass;
		};

		vm.printReport = function (event) {
			var seatBookingReportParams = {
				startDate: moment(vm.reportPeriod.StartDate).format('YYYY-MM-DD'),
				endDate: moment(vm.reportPeriod.EndData).format('YYYY-MM-DD'),
				teams: vm.reportSelectedTeams,
				locations: vm.reportSelectedLocations
			};
			
			seatPlanService.seatBookingReport.get(seatBookingReportParams).$promise.then(function (data) {
				vm.seatBookingsAll = data.SeatBookingsByDate;
				angular.element(document).ready(function () {
					
					if (event.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList[0] == undefined) {
						event.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.add('container-to-print-report');
						window.print();
						event.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.remove('container-to-print-report');
					} else {
						event.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.add('container-to-print-report');
						window.print();
						event.target.parentElement.parentElement.parentElement.parentElement.parentElement.parentElement.classList.remove('container-to-print-report');
					}
				});
			});
		};

		vm.init = function () {
			vm.setupSeatPlanStatusStrings();
			vm.setupToggles();
		};

		vm.init();

		var date = (angular.isDefined(params.viewDate) && params.viewDate != "") ? params.viewDate : null;

		if (date != null) {
			vm.loadMonthDetails(moment(params.viewDate));
		} else {
			vm.loadMonthDetails(moment());
		}

	}
}());


