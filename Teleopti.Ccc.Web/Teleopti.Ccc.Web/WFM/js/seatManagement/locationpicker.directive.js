
(function () {

	angular.module('wfm.seatPlan').controller('LocationPickerCtrl', locationPickerDirectiveController);
	locationPickerDirectiveController.$inject = ['$scope','seatPlanService', 'seatPlanTranslatorFactory'];

	function locationPickerDirectiveController($scope, seatPlanService, translator) {

		var vm = this;
		
		vm.locations = [];
		vm.selectedLocations = [];

		$scope.$watch("vm.selectedLocations", function (updatedLocationArray) {
			if (vm.locations.length > 0) {
				updateSelectedLocationsAfterExternalUpdate(vm.locations[0], updatedLocationArray);
			}
		});

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
				vm.selectedLocations.push(location.Id);
			} else {
				unselectLocation(location);
			}
		};

		function unselectLocation(location) {
			var index = vm.selectedLocations.indexOf(location.Id);
			if (index != -1) {
				vm.selectedLocations.splice(index, 1);
			}
		};

		function updateSelectedLocationsAfterExternalUpdate(locationNode, updatedLocationArray) {
			var index = vm.selectedLocations.indexOf(locationNode.Id);
			locationNode.selected = (index != -1);

			if (locationNode.Children !== undefined) {
				locationNode.Children.forEach(function(child) {
					updateSelectedLocationsAfterExternalUpdate(child, updatedLocationArray);
				});
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
				selectedLocations: '='
			},
			templateUrl: "js/seatManagement/html/locationpicker.html",
		};
	};

	angular.module('wfm.seatPlan').directive('locationPicker', directive);


}());