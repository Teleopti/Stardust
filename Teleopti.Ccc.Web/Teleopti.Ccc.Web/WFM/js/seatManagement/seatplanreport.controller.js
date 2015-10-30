"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportCtrl', seatPlanReportCtrl);
	seatPlanReportCtrl.$inject = ['$scope', 'seatPlanService'];

	function seatPlanReportCtrl($scope, seatPlanService) {
		var vm = this;

		vm.temp = {};
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

		vm.dateFilterIsValid = function () {
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

		vm.cancelFilter = function () {
			vm.isFilterOpened = false;
			reloadCachedFilterValuesAfterCancel();
		};


		vm.applyFilter = function () {

			if (!vm.dateFilterIsValid()) {
				return;
			}

			vm.isFilterOpened = false;

			vm.getSeatBookings({
				skip: vm.page,
				take: vm.reportTake,
				callback: getSeatBookingsCallback,
				isLoadingReportToDisplay: true
			});
			vm.currentPage = 1;
		};

		vm.firstPage = function () {
			vm.paging(1);
		};

		vm.previousPage = function () {
			vm.paging(vm.currentPage - 1);
		};

		vm.getVisiblePages = function (start, end) {
			var displayPageCount = 5;
			var ret = [];
			if (!end) {
				end = start;
				start = 1;
			}

			var leftBoundary = start;
			var rightBoundary = end;
			if (end - start >= displayPageCount) {
				var currentPage = vm.currentPage;

				if (currentPage < displayPageCount - 1) {
					leftBoundary = 1;
					rightBoundary = displayPageCount;
				} else if (end - currentPage < 3) {
					leftBoundary = end - displayPageCount + 1;
					rightBoundary = end;
				} else {
					leftBoundary = currentPage - Math.floor(displayPageCount / 2) > 1 ? currentPage - Math.floor(displayPageCount / 2) : 1;
					rightBoundary = currentPage + Math.floor(displayPageCount / 2) > end ? end : currentPage + Math.floor(displayPageCount / 2);
				}
			}

			for (var i = leftBoundary; i <= rightBoundary ; i++) {
				ret.push(i);
			}

			return ret;
		};
		
		vm.seekPage = function (page) {
			vm.paging(page);
		};

		vm.nextPage = function () {
			vm.paging(vm.currentPage + 1);
		};

		vm.lastPage = function () {
			vm.paging(vm.totalPages);
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

			initialiseWatchForDatePickerChanges();

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
			vm.selectedPeriod = angular.copy(vm.temp.selectedPeriod);
		};

		function getSeatBookingsCallback(data) {
			vm.seatBookings = data.SeatBookingsByDate;
			vm.isLoadingReport = false;
			vm.totalPages = Math.ceil(data.TotalRecordCount / vm.reportTake);
		};

		function keepDatePickersInSync(value) {
			vm.selectedPeriod.StartDate = angular.copy(value.startDate);
			vm.selectedPeriod.EndDate = angular.copy(value.endDate);
		};

		function initialiseWatchForDatePickerChanges() {
			$scope.$watch(function () {
				return { startDate: vm.selectedPeriod.StartDate, endDate: vm.selectedPeriod.EndDate };
			}, keepDatePickersInSync, true);
		};

	};

})();
