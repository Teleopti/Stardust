'use strict';

(function () {

	var getPreviousMonthStart = function (dateMoment) {
		return moment(dateMoment).subtract(1, 'months').startOf('month').format("YYYY-MM-DD");
	};

	var getNextMonthEnd = function (dateMoment) {
		return moment(dateMoment).add(1, 'months').endOf('month').format("YYYY-MM-DD");
	};

	var seatPlanStatusClass = ['success', 'inprogress', 'error'];

	angular.module('wfm.seatPlan').controller('SeatPlanCtrl', seatPlanDirectiveController);

	seatPlanDirectiveController.$inject = ['ResourcePlannerSvrc', 'seatPlanService', '$translate'];

	function seatPlanDirectiveController(resourcePlannerService, seatPlanService, translate) {

		var vm = this;
		
		vm.setupTranslatedStrings = function () {

			vm.translatedStrings = {};
			vm.setupTranslatedString("LoadingSeatPlanStatus");
			
			vm.seatPlanStatus = {};
			vm.translateSeatPlanStatus(0, 'SeatPlanStatusOK');
			vm.translateSeatPlanStatus(2, 'SeatPlanStatusError');
			vm.translateSeatPlanStatus(1, 'SeatPlanStatusInProgress');
			vm.translateSeatPlanStatus(3, 'SeatPlanStatusNoSeatPlanned');
		}

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
				startDate: getPreviousMonthStart(dateMoment),
				endDate: getNextMonthEnd(dateMoment)
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
			return moment(date).format('YYYY-MM-DD');
		};


		//ROBTODO: really need this below declaration???
		vm.setupTranslatedStrings();
		var now = moment();
		vm.loadMonthDetails(now);

		vm.onChangeOfDate = function () {

			if (vm.currentMonth != moment(vm.selectedDate).month()) {
				vm.loadMonthDetails(moment(vm.selectedDate));
			}
		};

		vm.onChangeOfMonth = function (date) {
			var dateMoment = moment(date);
			if (dateMoment.month() != vm.currentMonth) {
				vm.loadMonthDetails(dateMoment);
			}
		};

		vm.onSeatPlanComplete = function () {

			vm.loadMonthDetails(moment(vm.selectedDate));
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

		vm.getDayClass = function (date, mode) {
			if (mode === 'day') {

				var dayClass = '';
				var dayToCheck = moment(date);

				vm.seatPlanDateStatuses.forEach(function (status) {

					if (dayToCheck.isSame(moment(status.Date), 'day')) {
						dayClass = seatPlanStatusClass[status.Status];
					}
				});
			}
			return dayClass;
		}



	}
}());


