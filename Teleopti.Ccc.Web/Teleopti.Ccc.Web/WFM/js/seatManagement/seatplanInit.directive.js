﻿(function () {

	angular.module('wfm.seatPlan').controller('SeatPlanInitCtrl', seatPlanInitDirectiveController);
	seatPlanInitDirectiveController.$inject = ['seatPlanService', 'growl', '$translate'];

	function seatPlanInitDirectiveController(seatPlanService, growl, $translate) {

		var vm = this;

		vm.selectedLocations = [];
		vm.selectedTeams = [];

		vm.loadDefaultDates = function () {
			if (vm.start != null) {

				vm.period = {
					startDate: moment.utc(vm.start).toDate(),
					endDate: moment.utc(vm.end).toDate()
				};
			} else {
				var startDate = moment.utc().add(1, 'months').startOf('month').toDate();
				var endDate = moment.utc().add(2, 'months').startOf('month').toDate();
				vm.period = { startDate: startDate, endDate: endDate };
			}
		};

		vm.loadDefaultDates();

		vm.addSeatPlan = function () {

			vm.processingSeatPlan = true;

			var addSeatPlanCommand = {
				StartDate: vm.period.startDate,
				EndDate: vm.period.endDate,
				Teams: vm.selectedTeams,
				Locations: vm.selectedLocations
			};

			if (vm.selectedTeams.length == 0 || vm.selectedLocations.length == 0) {
				onSelectedTeamsLocationsEmpty($translate.instant("TeamsOrLocationsAreUnselected"));
				vm.processingSeatPlan = false;
			}
			else {

				vm.onSeatPlanStart();
				seatPlanService.addSeatPlan(addSeatPlanCommand).$promise.then(function (seatPlanResultMessage) {
					onAddSeatPlanCompleted(seatPlanResultMessage);

				});

			}
		};
		
		function onAddSeatPlanCompleted(seatPlanResultMessage) {

			var seatPlanResultDetailMessage = $translate.instant('SeatPlanResultDetailMessage')
						.replace('{0}', seatPlanResultMessage.NumberOfBookingRequests)
						.replace('{1}', seatPlanResultMessage.RequestsGranted)
						.replace('{2}', seatPlanResultMessage.RequestsDenied)
						.replace('{3}', seatPlanResultMessage.NumberOfUnscheduledAgentDays);

			if (seatPlanResultMessage.RequestsDenied > 0) {
				onWarningAddSeatPlan(seatPlanResultDetailMessage);
			}
			else {
				onSuccessAddSeatPlan(seatPlanResultDetailMessage);
			}

			vm.processingSeatPlan = false;
			vm.onSeatPlanComplete();
		};

		function onSuccessAddSeatPlan(message) {
			growl.success("<i class='mdi mdi-thumb-up'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		};

		function onWarningAddSeatPlan(message) {
			growl.warning("<i class='mdi mdi-alert'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		};

		function onSelectedTeamsLocationsEmpty(message) {
			growl.error("<i class='mdi  mdi-alert-octagon'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		};

	};

}());


(function () {

	var directive = function () {

		return {
			controller: 'SeatPlanInitCtrl',
			controllerAs: 'vm',
			bindToController: true,
			require: ['seatPlanInit', '^teleoptiCard'],
			scope: {
				start: '@',
				end: '@',
				onSeatPlanStart: '&',
				onSeatPlanComplete: '&',
				showReport: '='
			},
			templateUrl: "js/seatManagement/html/seatplaninit.html",

		};
	};

	angular.module('wfm.seatPlan').directive('seatPlanInit', directive);

}());