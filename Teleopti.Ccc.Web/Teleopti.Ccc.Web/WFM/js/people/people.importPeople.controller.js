'use strict';

(function() {
	angular.module('wfm.people')
		.controller('ImportPeopleCtrl', [
			'Upload', 'uiGridExporterConstants', '$timeout', 'PeopleSearch', PeopleImportController
		]);

	function PeopleImportController(Upload, uiGridExporterConstants, $timeout, SearchSvrc) {
		var vm = this;

		vm.files = [];
		vm.rejFiles = [];
		vm.dataWithError = [];
		vm.rawUsersData = [];

		vm.log = '';

		vm.isImportReady = false;

		vm.gridOptionForImport = {
			exporterCsvFilename: 'invalidUsers.csv',
			exporterOlderExcelCompatibility: true,
			enableSorting: false,
			disableColumnMenu: true,
			disableHiding: true,
			columnDefs: [
				{ displayName: 'Firstname', field: 'Firstname', disableHiding: true },
				{ displayName: 'Lastname', field: 'Lastname' },
				{ displayName: 'WindowsUser', field: 'WindowsUser' },
				{ displayName: 'ApplicationUserId', field: 'ApplicationUserId' },
				{ displayName: 'Password', field: 'Password' },
				{ displayName: 'Role', field: 'Role' },
				{ displayName: 'ErrorMessage', field: 'ErrorMessage' }
			],
			onRegisterApi: function(gridApi) {
				vm.gridForImportApi = gridApi;
			},
			data: 'vm.dataWithError'

		};

		vm.gridOptionForImport.importerDataAddCallback = function(grid, newObjects) {
			vm.rawUsersData = newObjects;
			var data = { Users: vm.rawUsersData }
			SearchSvrc.importUsers.post(data)
				.$promise.then(function(result) {
					vm.isImportReady = true;
					vm.dataWithError = result.InvalidUsers;
					vm.successfulCount = result.SuccessfulCount;
					vm.invalidCount = result.InvalidCount;

					$timeout(function() {
						vm.gridForImportApi.core.handleWindowResize();
						vm.export();
					});


				});
		};

		vm.toggleImportPeople = function() {
			vm.parentVm.toggleImportPeople();
		};

		vm.upload = function(files) {
			if (files && files.length) {
				for (var i = 0; i < files.length; i++) {
					var file = files[i];
					vm.gridForImportApi.importer.importFile(file);
				}
			}
		};

		vm.export = function() {
			if (vm.isImportReady) {
				vm.gridForImportApi.exporter.csvExport(uiGridExporterConstants.ALL, uiGridExporterConstants.ALL);
			}


		};
	};

}());