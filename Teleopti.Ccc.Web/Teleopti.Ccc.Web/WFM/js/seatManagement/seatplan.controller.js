'use strict';

(function () {

	angular.module('wfm.seatPlan').controller('SeatPlanCtrl', seatPlanDirectiveController);

	seatPlanDirectiveController.$inject = ['ResourcePlannerSvrc', 'seatPlanService', '$translate', '$stateParams','Toggle'];

	function seatPlanDirectiveController(resourcePlannerService, seatPlanService, translate, params, toggleService) {

		var vm = this;

		vm.setupTranslatedStrings = function() {

			vm.translatedStrings = {};
			vm.setupTranslatedString("LoadingSeatPlanStatus");

			vm.seatPlanStatus = {};
			vm.translateSeatPlanStatus(0, 'SeatPlanStatusOK');
			vm.translateSeatPlanStatus(2, 'SeatPlanStatusError');
			vm.translateSeatPlanStatus(1, 'SeatPlanStatusInProgress');
			vm.translateSeatPlanStatus(3, 'SeatPlanStatusNoSeatPlanned');
		};


		vm.setupToggles = function() {
			toggleService.isFeatureEnabled.query({ toggle: 'Wfm_SeatPlan_SeatMapBookingView_32814' }).$promise.then(function(result) {
				vm.showOccupancyView = result.IsEnabled;
			});
		};
		
		vm.getPreviousMonthStart = function (dateMoment) {
			return moment(dateMoment).subtract(1, 'months').startOf('month').format("YYYY-MM-DD");
		};

		vm.getNextMonthEnd = function (dateMoment) {
			return moment(dateMoment).add(1, 'months').endOf('month').format("YYYY-MM-DD");
		};

		vm.seatPlanStatusClass = ['seatplan-status-success', 'seatplan-status-inprogress', 'seatplan-status-error'];

		vm.setupTranslatedString = function (key) {
			translate(key).then(function (result) {
				vm.translatedStrings[key] = result;
			});
		};

		vm.translateSeatPlanStatus = function (key, translationConstant) {
			translate(translationConstant).then(function (result) {
				vm.seatPlanStatus[key] = result;
			});
		};


		vm.loadMonthDetails = function (dateMoment) {

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
			   	vm.isLoadingPlanningPeriods = false;
			   	vm.planningPeriods = data;
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
			var dateMoment = moment(date);
			if (dateMoment.month() != vm.currentMonth) {
				vm.loadMonthDetails(dateMoment);
			}
		};

		vm.onSeatPlanComplete = function () {
			vm.loadMonthDetails(moment(vm.selectedDate));
		};

		vm.showReport = function (period) {
			vm.isReportOpened = !vm.isReportOpened;
			vm.reportPeriod = period;
		};

		vm.getSelectedMonthName = function () {
			return moment(vm.selectedDate).format("MMMM");
		};

		vm.getToDayInfo = function () {

			if (vm.isLoadingCalendar) {
				return vm.translatedStrings["LoadingSeatPlanStatus"];
			}

			var dayInfoString = vm.seatPlanStatus[3];

			vm.seatPlanDateStatuses.forEach(function (dateEvent) {

				if (moment(vm.selectedDate).isSame(dateEvent.Date, 'day')) {
					dayInfoString = vm.seatPlanStatus[dateEvent.Status];
				}
			});

			return dayInfoString;
		};

		vm.getDayClass = function(date, mode) {
			if (mode === 'day') {

				var dayClass = '';
				var dayToCheck = moment(date);

				vm.seatPlanDateStatuses.forEach(function(status) {

					if (dayToCheck.isSame(moment(status.Date), 'day')) {
						dayClass = vm.seatPlanStatusClass[status.Status];
					}
				});
			}
			return dayClass;
		};

		vm.init = function () {
			
			vm.setupTranslatedStrings();
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


