(function() {
    'use strict';

    angular
        .module('wfm.businessunits')
        .controller('BusinessUnitsCtrl', BusinessUnitsCtrl);

    BusinessUnitsCtrl.$inject = [ '$filter', '$window', 'BusinessUnitsService'];

    function BusinessUnitsCtrl($filter,  $window, BusinessUnitsService) {
        var vm = this;
				vm.show = false;
				vm.data = {
					selectedBu: null
				};

				vm.changeBusinessUnit = function (selectedBu) {
					BusinessUnitsService.setBusinessUnit(selectedBu.Id);
					$window.location.reload();
				};

        vm.loadBusniessUnit = function(){
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
        };
    }
})();
