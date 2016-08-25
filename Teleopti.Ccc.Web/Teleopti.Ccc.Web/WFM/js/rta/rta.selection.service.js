(function() {
	'use strict';

	angular.module('wfm.rta').service('RtaSelectionService', [
		function() {
			// this.goToSites = function() {
			// 	$state.go('rta');
			// };

      this.toggleSelection = function(id, selectedItemIds) {
        var index = selectedItemIds.indexOf(id);
        if (index > -1) {
          selectedItemIds.splice(index, 1);
        } else {
          selectedItemIds.push(id);
        }
        return selectedItemIds;
      };
		}
	]);
})();
