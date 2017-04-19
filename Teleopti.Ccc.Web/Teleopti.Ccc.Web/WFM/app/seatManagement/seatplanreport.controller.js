"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportCtrl', seatPlanReportCtrl);
	seatPlanReportCtrl.$inject = ['$scope', 'seatPlanService'];

	function seatPlanReportCtrl($scope, seatPlanService) {
		var vm = this;
		vm.temp = {};
		vm.showOnlyUnseatedAgents = false;

		vm.paginationOptions = { pageNumber: 1, pageSize: 34, totalPages: 1 };

		vm.getSeatBookings = function (options) {
			vm.isLoadingReport = options.isLoadingReportToDisplay;
			var seatBookingReportParams = {
				startDate: moment(vm.selectedPeriod.startDate).format('YYYY-MM-DD'),
				endDate: moment(vm.selectedPeriod.endDate).format('YYYY-MM-DD'),
				teams: vm.selectedTeams,
				locations: vm.selectedLocations,
				showUnseatedOnly: vm.showOnlyUnseatedAgents,
				skip: options.skip,
				take: options.take
			};

			seatPlanService.seatBookingReport.get(seatBookingReportParams).$promise.then(function (data) {
				options.callback != null && options.callback(data);
			});
		};

		vm.dateFilterIsValid = function () {
			return (vm.selectedPeriod && vm.selectedPeriod.startDate <= vm.selectedPeriod.endDate);
		};

		vm.showAllAgents = function () {
			return !vm.showOnlyUnseatedAgents;
		};

		vm.setShowAllAgents = function (value) {
			vm.showOnlyUnseatedAgents = !value;
		};

		vm.toggleFilterVisibility = function () {

			if (!vm.isFilterOpened) {
				cacheFilterValuesInCaseOfCancel();
			} else {
				reloadCachedFilterValuesAfterCancel();
			}

			vm.isFilterOpened = !vm.isFilterOpened;
		};

		vm.cancelFilter = function () {
			vm.isFilterOpened = false;
			reloadCachedFilterValuesAfterCancel();
		};

		vm.applyFilter = function () {

			if (!vm.dateFilterIsValid()) {
				return;
			}

			vm.isFilterOpened = false;
			vm.paginationOptions.pageNumber = 1;

			vm.getSeatBookings({
				skip: (vm.paginationOptions.pageNumber - 1) * vm.paginationOptions.pageSize,
				take: vm.paginationOptions.pageSize,
				callback: getSeatBookingsCallback,
				isLoadingReportToDisplay: true
			});
		};

		vm.getPageData = function () {
			vm.getSeatBookings({
				skip: (vm.paginationOptions.pageNumber - 1) * vm.paginationOptions.pageSize,
				take: vm.paginationOptions.pageSize,
				callback: getSeatBookingsCallback,
				isLoadingReportToDisplay: true
			});
		};

		vm.init = function () {
			vm.getPageData();
		};

		function cacheFilterValuesInCaseOfCancel() {
			vm.temp.selectedTeams = angular.copy(vm.selectedTeams);
			vm.temp.selectedLocations = angular.copy(vm.selectedLocations);
			vm.temp.selectedPeriod = angular.copy(vm.selectedPeriod);
			vm.temp.showOnlyUnseatedAgents = vm.showOnlyUnseatedAgents;
		};

		function reloadCachedFilterValuesAfterCancel() {
			vm.selectedTeams = angular.copy(vm.temp.selectedTeams);
			vm.selectedLocations = angular.copy(vm.temp.selectedLocations);
			vm.selectedPeriod = angular.copy(vm.temp.selectedPeriod);
			vm.showOnlyUnseatedAgents = vm.temp.showOnlyUnseatedAgents;
		};

		function getSeatBookingsCallback(data) {
			vm.seatBookings = data.SeatBookingsByDate;
			vm.isLoadingReport = false;
			vm.paginationOptions.totalPages = Math.ceil(data.TotalRecordCount / vm.paginationOptions.pageSize);
		};

	};

})();
