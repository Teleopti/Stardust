'use strict';

(function() {
	angular.module('wfm.people')
		.controller('ImportPeopleCtrl', [
			'$translate','Upload', 'uiGridExporterConstants', '$timeout', 'People', PeopleImportController
		]);

	function PeopleImportController($translate, Upload, uiGridExporterConstants, $timeout, peopleSvc) {
		var vm = this;

		vm.files = [];
		vm.rejFiles = [];
		vm.dataWithError = [];
		vm.rawUsersData = [];

		vm.log = '';

		vm.isSuccessful = false;
		vm.isFailed = false;
		vm.hasParsingError = false;
		var columnHeaders = ['Firstname',
							'Lastname',
							'WindowsUser',
							'ApplicationUserId',
							'Password',
							'Role'];

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
			importerProcessHeaders: function (grid, headerRow) {
				var headers = [];

				headerRow.forEach(function (value) {
					if(columnHeaders.indexOf(value.toString()) != -1){
						headers.push(value.toString());
					} else {
						headers.push(null);
					}
				});
				return headers;
			},
			data: 'vm.dataWithError'
		};


		
		vm.gridOptionForImport.importerDataAddCallback = function (grid, newObjects) {
			vm.rawUsersData = newObjects;
			if (vm.rawUsersData.length > 0) {
				var rawUser = vm.rawUsersData[0];
				var columnNotExist = [];
				angular.forEach(columnHeaders, function(col) {
					if (!rawUser.hasOwnProperty(col)) {
						vm.hasParsingError = true;
						columnNotExist.push(col);
					}
				});
				if (vm.hasParsingError) {

					vm.errorMsg = "Following columns are required:" + columnNotExist.join(',');
				}
			}
			if (!vm.hasParsingError) {
				var data = { Users: vm.rawUsersData }
				peopleSvc.importUsers.post(data)
					.$promise.then(function(result) {
							vm.isSuccessful = true;
							vm.dataWithError = result.InvalidUsers;
							vm.hasDataWithError = vm.dataWithError.length > 0;
							vm.successfulCount = result.SuccessfulCount;
							vm.invalidCount = result.InvalidCount;
							if (vm.hasDataWithError) {
								$timeout(function() {
									vm.gridForImportApi.core.handleWindowResize();
									vm.export();
								});
							}

						},
						function(error) {
							vm.isFailed = true;
							vm.errorMsg = error.data.Message;
						});
			}
		};

		vm.toggleImportPeople = function() {
			vm.parentVm.toggleImportPeople();
		};

		vm.upload = function (files) {
			vm.isSuccessful = false;
			vm.isFailed = false;
			vm.hasParsingError = false;
			vm.errorMsg = '';
			if (files && files.length) {
				for (var i = 0; i < files.length; i++) {
					var file = files[i];
					peopleSvc.uploadUserFromFile(file).success(function (data) {
						var blob = new Blob([data], {
							type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
						});
						if (blob.size != 0) {
							saveAs(blob, 'dataWithError' + '.xlsx');
						}

					}).error(function () {
						//Some error log
					});

				}
			}
		};

		


		vm.export = function() {
			if (vm.isSuccessful && vm.hasDataWithError) {
				vm.gridForImportApi.exporter.csvExport(uiGridExporterConstants.ALL, uiGridExporterConstants.ALL);
			}


		};
	};

}());