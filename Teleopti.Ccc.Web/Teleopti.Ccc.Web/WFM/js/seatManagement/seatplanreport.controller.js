"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportCtrl', seatPlanReportCtrl);
	seatPlanReportCtrl.$inject = ['$scope', 'seatPlanService'];

	function seatPlanReportCtrl($scope, seatPlanService) {
		var vm = this;

		vm.temp = {};
		
		vm.selectedLocations = [];
		vm.selectedTeams = [];
		vm.isDatePickerOpened = true;
		vm.reportTake = 34;
		
		vm.getSeatBookings = function (options) {
			vm.isLoadingReport = options.isLoadingReportToDisplay;
			var seatBookingReportParams = {
				startDate: moment(vm.selectedPeriod.StartDate).format('YYYY-MM-DD'),
				endDate: moment(vm.selectedPeriod.EndDate).format('YYYY-MM-DD'),
				teams: vm.selectedTeams,
				locations: vm.selectedLocations,
				skip: options.skip,
				take: options.take
			};

			seatPlanService.seatBookingReport.get(seatBookingReportParams).$promise.then(function (data) {
				options.callback != null && options.callback(data);
			});
		};

		vm.setRangeClass = function (date, mode, period) {
			if (mode === 'day') {
				var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
				var startDay = new Date(period.StartDate).setHours(12, 0, 0, 0);
				var endDay = new Date(period.EndDate).setHours(12, 0, 0, 0);
				if (dayToCheck >= startDay && dayToCheck <= endDay) {
					return 'seatplan-status-success';
				}
			}
			return '';
		};

		vm.dateFilterIsValid = function() {
			return (vm.selectedPeriod.StartDate <= vm.selectedPeriod.EndDate);
		};

		vm.toggleFilterVisibility = function () {

			if (!vm.isFilterOpened) {
				cacheFilterValuesInCaseOfCancel();
			} else {
				reloadCachedFilterValuesAfterCancel();
			}
			
			vm.isFilterOpened = !vm.isFilterOpened;
		}

		vm.toggleFilter = function (tabName) {
			vm.isDatePickerOpened = tabName == 'date';
			vm.isTeamPickerOpened = tabName == 'team';
			vm.isLocationPickerOpened = tabName == 'location';

		};

		vm.cancelFilter = function() {
			vm.isFilterOpened = false;
			reloadCachedFilterValuesAfterCancel();
		};
		

		vm.applyFilter = function () {

			vm.isFilterOpened = false;
			
			vm.getSeatBookings({
				skip: vm.page,
				take: vm.reportTake,
				callback: getSeatBookingsCallback,
				isLoadingReportToDisplay: true
			});
			vm.currentPage = 1;
		};

		vm.paging = function (goToPage) {

			if (goToPage > vm.totalPages) {
				goToPage = vm.totalPages;
			} else if (goToPage < 1) {
				goToPage = 1;
			}

			if (vm.currentPage != goToPage) {
				vm.getSeatBookings({
					skip: (goToPage - 1) * vm.reportTake,
					take: vm.reportTake,
					callback: getSeatBookingsCallback,
					isLoadingReportToDisplay: true
				});
				vm.currentPage = goToPage;
			}
		};

		vm.init = function () {
			vm.getSeatBookings({
				skip: vm.page,
				take: vm.reportTake,
				callback: getSeatBookingsCallback,
				isLoadingReportToDisplay: true
			});
			vm.currentPage = 1;
		};


		function cacheFilterValuesInCaseOfCancel() {
			vm.temp.selectedTeams = angular.copy(vm.selectedTeams);
			vm.temp.selectedLocations = angular.copy(vm.selectedLocations);
			vm.temp.selectedPeriod = angular.copy(vm.selectedPeriod);
		};

		function reloadCachedFilterValuesAfterCancel() {
			vm.selectedTeams = angular.copy(vm.temp.selectedTeams);
			vm.selectedLocations = angular.copy(vm.temp.selectedLocations);
			angular.copy(vm.temp.selectedPeriod, vm.selectedPeriod);
		};

		function getSeatBookingsCallback(data) {
			vm.seatBookings = data.SeatBookingsByDate;
			vm.isLoadingReport = false;
			vm.totalPages = Math.ceil(data.TotalRecordCount / vm.reportTake);
		};

	};

})();
