(function() {
    'use strict';

    angular
        .module('wfm.businessunits')
        .controller('BusinessUnitsController', BusinessUnitsController);

    BusinessUnitsController.$inject = [ '$filter', '$window', 'BusinessUnitsService'];

    function BusinessUnitsController($filter,  $window, BusinessUnitsService) {
        var vm = this;
				vm.show = false;
        vm.buMenu = false;
				vm.data = {
					selectedBu: null
				};

        vm.toggleBuMenu = function () {
          vm.buMenu = !vm.buMenu;
        }

				vm.changeBusinessUnit = function (selectedBu) {
					BusinessUnitsService.setBusinessUnit(selectedBu.Id);
					$window.location.reload();
				};

        (vm.loadBusinessUnit = function(){
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
          })
      })();
    }
})();
