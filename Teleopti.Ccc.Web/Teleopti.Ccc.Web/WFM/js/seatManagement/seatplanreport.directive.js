"use strict";

(function () {

	angular.module('wfm.seatPlan').controller('seatPlanReportCtrl', seatPlanReportCtrl);

	seatPlanReportCtrl.$inject = ['seatPlanService', 'seatplanTeamAndLocationService'];

	function seatPlanReportCtrl(seatPlanService, seatplanTeamAndLocationService) {
		var vm = this;

		vm.teams = [];
		vm.locations = [];

		vm.getTeamDisplayText = seatplanTeamAndLocationService.GetTeamDisplayText;
		vm.getLocationDisplayText = seatplanTeamAndLocationService.GetLocationDisplayText;

		vm.selectTeam = seatplanTeamAndLocationService.SelectTeam;
		vm.selectLocation = seatplanTeamAndLocationService.SelectLocation;
		vm.selectedPeriod = vm.selectedPeriod == undefined ? { StartDate: '2015-01-01', EndDate: '2015-01-01' } : vm.selectedPeriod;

		vm.isDatePickerOpened = true;

		vm.currentPage = 1;
		vm.take = 50;
		vm.totalPages = 1;

		seatPlanService.locations.get().$promise.then(function (locations) {
			locations.show = true;
			vm.locations.push(locations);
		});

		seatPlanService.teams.get().$promise.then(function (teams) {
			teams.show = true;
			vm.teams.push(teams);
		});

		vm.getSeatBookingsCallback = function (data) {
			vm.seatBookings = data.SeatBookingsByDate;
			vm.isLoadingReport = false;
			vm.totalPages = Math.ceil(data.TotalRecordCount / vm.take);
		};

		vm.getSeatBookings = function (options, takeAllPages) {
			if (!takeAllPages)
				vm.isLoadingReport = true;

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

		vm.getSeatBookings({
			skip: vm.page,
			take: vm.take,
			callback : vm.getSeatBookingsCallback
		});

		vm.getDisplayTimeString = function (startTime, endTime) {
			return moment(startTime).format('HH:mm') + ' - ' + moment(endTime).format('HH:mm');
		};

		vm.getDisplayDateString = function (date) {
			return moment(date).format('L');
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
				take: vm.take,
				callback: vm.getSeatBookingsCallback
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
					skip: (goToPage - 1) * vm.take,
					take: vm.take,
					callback: vm.getSeatBookingsCallback
				});
				vm.currentPage = goToPage;
			}
		};
	};
})();


(function () {

	angular.module('wfm.seatPlan').directive('seatPlanReport', seatPlanReport);

	function seatPlanReport() {
		return {
			controller: 'seatPlanReportCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: { toggle: '&toggle', selectedPeriod: '=selectedPeriod' },
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatplanreport.html"
		};
	};

	angular.module('wfm.seatPlan').directive('seatPlanReportTable', seatPlanReportTable);

	function seatPlanReportTable() {
		return {
			restrict: "E",
			scope: {
				seatBookings: '=',
				getDisplayDate: '=',
				getDisplayTime: '='
			},
			templateUrl: "js/seatManagement/html/seatplanreporttable.html"
		};
	};

	angular.module('wfm.seatPlan').directive('seatPlanReportPrint', seatPlanReportPrint);

	function seatPlanReportPrint() {

		var printSectionDiv = document.createElement('div');

		function printElement(elem) {
			printSectionDiv = elem.cloneNode(true);
			printSectionDiv.id = "seatPlanReportContentPrintDiv";
			document.body.appendChild(printSectionDiv);
			window.print();
			if (document.getElementById('seatPlanReportContentPrintDiv')) {
				document.body.removeChild(printSectionDiv);
			}
		}

		function link(scope, element, attrs) {
			element.on('click', function () {
				scope.vm.getSeatBookings({
					callback: function (data) {
						scope.vm.seatBookingsAll = data.SeatBookingsByDate;
						angular.element(document).ready(function () {
							var elemToPrint = document.getElementById(attrs.printElementId);
							if (elemToPrint) {
								printElement(elemToPrint);
							}
						});
					}
				}, scope.vm.totalPages);
			});
		}

		return {
			link: link,
			restrict: 'A'
		};
	}
})();