
(function () {

	angular.module('wfm.seatPlan').controller('LocationPickerCtrl', locationPickerDirectiveController);
	locationPickerDirectiveController.$inject = ['seatPlanService', 'seatPlanTranslatorFactory'];

	function locationPickerDirectiveController(seatPlanService, translator) {

		var vm = this;

		vm.locations = [];

		seatPlanService.locations.get().$promise.then(function (locations) {
			locations.show = true;
			vm.locations.push(locations);
		});

		vm.getLocationDisplayText = function (location) {
			if (location == null || location.Name == undefined) {
				return translator.TranslatedStrings["NoLocationsAvailable"];
			}
			return location.Name + " (" + translator.TranslatedStrings["SeatCountTitle"] + ": {0})".replace("{0}", location.Seats.length);
		};

		vm.toggleLocationSelection = function (location) {

			location.selected = location.Seats && location.Seats.length > 0 ? !location.selected : location.selected;
			if (location.selected) {
				vm.selectedlocations.push(location.Id);
			} else {
				unselectLocation(location);
			}
		};

		function unselectLocation(location) {
			var index = vm.selectedlocations.indexOf(location.Id);
			if (index != -1) {
				vm.selectedlocations.splice(index, 1);
			}
		};
	};

}());



(function () {

	var directive = function () {

		return {
			controller: 'LocationPickerCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				selectedlocations: '='
			},
			templateUrl: "js/seatManagement/html/locationpicker.html",
		};
	};

	angular.module('wfm.seatPlan').directive('locationPicker', directive);


}());