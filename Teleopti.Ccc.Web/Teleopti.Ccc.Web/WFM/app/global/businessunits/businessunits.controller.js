(function() {
    'use strict';

    angular
        .module('wfm.businessunits')
        .controller('BusinessUnitsCtrl', BusinessUnitsCtrl);

    BusinessUnitsCtrl.$inject = ['$scope', '$resource', '$http', '$filter', '$state', '$sessionStorage', '$window', 'BusinessUnitsService'];

    function BusinessUnitsCtrl($scope, $resource, $http, $filter, $state, $sessionStorage, $window, BusinessUnitsService) {
        var vm = this;

				vm.show = false;

				vm.data = {
					selectedBu: null
				};

				vm.changeBusinessUnit = function (selectedBu) {
					BusinessUnitsService.setBusinessUnit(selectedBu.Id);
					$window.location.reload();
				};

				BusinessUnitsService.getAllBusinessUnits().then(function(result) {
					vm.data.businessUnits = result;
					vm.show = (result.length > 1);
					var buid = BusinessUnitsService.getBusinessUnitFromSessionStorage();
					if (buid) {
						var businessUnit = $filter('filter')(result, function(d) { return d.Id === buid; })[0];
						vm.data.selectedBu = businessUnit;
					} else {
						vm.data.selectedBu = result[0];
					}
				});
    }
})();
