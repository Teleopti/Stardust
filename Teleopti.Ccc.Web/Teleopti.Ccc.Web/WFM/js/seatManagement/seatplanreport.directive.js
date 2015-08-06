"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportCtrl', seatPlanReportCtrl);

	seatPlanReportCtrl.$inject = ['seatPlanService', 'seatplanTeamAndLocationService'];

	function seatPlanReportCtrl(seatPlanService, seatplanTeamAndLocationService) {

		var vm = this;

		function getSeatBookings(options) {
			vm.isLoadingReport = true;
			var seatBookingReportParams = {
				startDate: options.startDate != null ? moment(options.startDate).format('YYYY-MM-DD') : moment().format('YYYY-MM-DD'),
				endDate: options.endDate != null ? moment(options.endDate).format('YYYY-MM-DD') : moment().format('YYYY-MM-DD'),
				teams: options.teams != null ? options.teams : [],
				locations: options.locations != null ? options.locations : [],
				skip: options.skip != null ? options.skip : 0,
				take: options.take != null ? options.take : 20
			};
			seatPlanService.seatBookingReport.get(seatBookingReportParams).$promise.then(function (data) {
				vm.seatBookings = data.SeatBookingsByDate;
				vm.isLoadingReport = false;
			});
		};


		vm.locations = [];
		vm.teams = [];
		vm.isDatePickerOpened = true;
		vm.selectedPeriod = vm.selectedPeriod == undefined ? { StartDate: '2015-01-01', EndDate: '2015-01-01' } : vm.selectedPeriod;

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

		vm.getDisplayTimeString = function (startTime, endTime) {
			return moment(startTime).format('HH:mm') + ' - ' + moment(endTime).format('HH:mm');
		};

		vm.getDisplayDateString = function(date) {
			return moment(date).format('L');
		};

		vm.applyFilter = function () {
			getSeatBookings({
				startDate: vm.selectedPeriod.StartDate,
				endDate: vm.selectedPeriod.EndDate,
				teams: seatplanTeamAndLocationService.GetSelectedTeamsFromTeamList(vm.teams),
				locations: seatplanTeamAndLocationService.GetSelectedLocationsFromLocationList(vm.locations)
			});
		};
		vm.setRangeClass = function (date, mode) {
			if (mode === 'day') {
				var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
				var startDay = new Date(vm.selectedPeriod.StartDate).setHours(12, 0, 0, 0);
				var endDay = new Date(vm.selectedPeriod.EndDate).setHours(12, 0, 0, 0);
				if (dayToCheck >= startDay && dayToCheck <= endDay) {
					return 'seatplan-status-success';
				}
			}
			return '';
		};

		vm.selectTeam = seatplanTeamAndLocationService.SelectTeam;

		vm.selectLocation = seatplanTeamAndLocationService.SelectLocation;

		vm.getTeamDisplayText = seatplanTeamAndLocationService.GetTeamDisplayText;

		vm.getLocationDisplayText = seatplanTeamAndLocationService.GetLocationDisplayText;


		getSeatBookings({
			startDate: vm.selectedPeriod == null ? null : vm.selectedPeriod.StartDate,
			endDate: vm.selectedPeriod == null ? null : vm.selectedPeriod.EndDate,
			take: 50

	});

		seatPlanService.locations.get().$promise.then(function (locations) {
			locations.show = true;
			vm.locations.push(locations);
		});

		seatPlanService.teams.get().$promise.then(function (teams) {
			teams.show = true;
			vm.teams.push(teams);
		});

	};
})();


(function () {

	angular.module('wfm.seatPlan').directive('seatPlanReport', seatPlanReport);

	function seatPlanReport() {
		return {
			controller: 'seatPlanReportCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: { toggle: '&toggle', selectedPeriod: '=selectedPeriod'},
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatplanreport.html"
		};
	};
})();