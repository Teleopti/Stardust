"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportCtrl', seatPlanReportCtrl)
								  .value('reportTake', 50 );

	seatPlanReportCtrl.$inject = ['seatPlanService', 'seatplanTeamAndLocationService', 'reportTake'];

	function seatPlanReportCtrl(seatPlanService, seatplanTeamAndLocationService, reportTake) {
		var vm = this;

		vm.teams = [];
		vm.locations = [];
		vm.isDatePickerOpened = true;

		vm.getTeamDisplayText = seatplanTeamAndLocationService.GetTeamDisplayText;
		vm.getLocationDisplayText = seatplanTeamAndLocationService.GetLocationDisplayText;
		vm.selectTeam = seatplanTeamAndLocationService.SelectTeam;
		vm.selectLocation = seatplanTeamAndLocationService.SelectLocation;

		seatPlanService.locations.get().$promise.then(function (locations) {
			locations.show = true;
			vm.locations.push(locations);
		});

		seatPlanService.teams.get().$promise.then(function (teams) {
			teams.show = true;
			vm.teams.push(teams);
		});

		function getSeatBookingsCallback(data) {
			vm.seatBookings = data.SeatBookingsByDate;
			vm.isLoadingReport = false;
			vm.totalPages = Math.ceil(data.TotalRecordCount / reportTake);
		};

		vm.getSeatBookings = function (options) {
			vm.isLoadingReport = options.isLoadingReportToDisplay;
			var seatBookingReportParams = {
				startDate: moment(vm.selectedPeriod.StartDate).format('YYYY-MM-DD'),
				endDate: moment(vm.selectedPeriod.EndDate).format('YYYY-MM-DD'),
				teams: seatplanTeamAndLocationService.GetSelectedTeamsFromTeamList(vm.teams),
				locations: seatplanTeamAndLocationService.GetSelectedLocationsFromLocationList(vm.locations),
				skip: options.skip,
				take: options.take
			};

			seatPlanService.seatBookingReport.get(seatBookingReportParams).$promise.then(function (data) {
				options.callback != null && options.callback(data);
			});
		};

		vm.getDisplayTimeString = function (startTime, endTime) {
			return moment(startTime).format('HH:mm') + ' - ' + moment(endTime).format('HH:mm');
		};

		vm.getDisplayDateString = function (date) {
			return moment(date).format('L');
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

		vm.toggleFilter = function (tabName) {
			vm.isDatePickerOpened = false;
			vm.isTeamPickerOpened = false;
			vm.isLocationPickerOpened = false;

			if (tabName == 'date') {
				vm.isDatePickerOpened = true;
			}
			if (tabName == 'team') {
				vm.isTeamPickerOpened = true;
			}
			if (tabName == 'location') {
				vm.isLocationPickerOpened = true;
			}
		};

		vm.applyFilter = function () {
			vm.getSeatBookings({
				skip: vm.page,
				take: reportTake,
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
					skip: (goToPage - 1) * reportTake,
					take: reportTake,
					callback: getSeatBookingsCallback,
					isLoadingReportToDisplay: true
				});
				vm.currentPage = goToPage;
			}
		};

		vm.init = function () {
			vm.getSeatBookings({
				skip: vm.page,
				take: reportTake,
				callback: getSeatBookingsCallback,
				isLoadingReportToDisplay: true
			});
			vm.currentPage = 1;
		};
	};

})();
