'use strict';

(function () {
	angular.module('wfm.people')
		.controller('PeopleCartCtrl', [
			'$translate', '$stateParams', 'People', PeopleImportController
		]);

	function PeopleImportController($translate, $stateParams, peopleSvc) {
		var vm = this;

		vm.selectedPeopleIds = $stateParams.selectedPeopleIds;
		vm.commandName = $stateParams.commandTag;

		vm.columnMap = [{ tag: "adjustSkill", columns: ["skill", "shiftBag"] }];
		vm.constructColumns = function() {
			angular.forEach(vm.columnMap, function (item) {
				if (item.tag === vm.commandName) {
					vm.gridOptions.columnDefs.concat(item.columns);
				}
			});
		};

		vm.gridOptions = {
			columnDefs: [
				{ displayName: 'FirstName', field: 'FirstName', headerCellFilter: 'translate', cellClass: 'first-name', minWidth: 100 },
				{
					displayName: 'LastName',
					field: 'LastName',
					headerCellFilter: 'translate',
					sort: {
						direction: uiGridConstants.ASC,
						priority: 0,
					},
					minWidth: 100
				}
			],
			
		};
	};

}());