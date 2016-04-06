(function () {

	angular.module('wfm.seatPlan').controller('SeatPlanInitCtrl', seatPlanInitDirectiveController);
	seatPlanInitDirectiveController.$inject = ['seatPlanService', 'NoticeService', '$translate'];

	function seatPlanInitDirectiveController(seatPlanService, NoticeService, $translate) {

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
			NoticeService.success(message + ".", 5000, true);
		};

		function onWarningAddSeatPlan(message) {
			NoticeService.warning(message + ".", 5000, true);
		};

		function onSelectedTeamsLocationsEmpty(message) {
			NoticeService.error(message + ".", 5000, true);
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
