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
		vm.isProcessing = false;
		vm.errorMsg = '';
		vm.toggleImportPeople = function() {
			vm.parentVm.toggleImportPeople();
		};
		vm.hasInvalidData = false;

		vm.upload = function (files) {
			vm.isSuccessful = false;
			vm.isFailed = false;
			vm.hasParsingError = false;
			vm.errorMsg = '';
			if (files && files.length) {
				for (var i = 0; i < files.length; i++) {
					var file = files[i];
					vm.isProcessing = true;
					peopleSvc.uploadUserFromFile(file).then(function (response) {
						vm.isProcessing = false;
						vm.isSuccessful = true;
						var isXlsx = response.headers()['content-type'] == 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
						var blob = new Blob([response.data], {
							type: response.headers()['content-type']
						});
						if (blob.size != 0) {
							vm.hasInvalidData = true;
							var extension = isXlsx ? '.xlsx' : '.xls';
							saveAs(blob, 'invalidUsers' + extension);
						}

					}, function (response) {
						vm.isProcessing = false;
						vm.isFailed = true;
						//Some error log
						arrayBuffer2String(response.data,
								function (string) {
									$timeout(function() {
										vm.errorMsg = string; 
									});
								}
							);
					});

				}
			}
		};
		function arrayBuffer2String(buf, callback) {
			var bb = new Blob([buf]);
			var f = new FileReader();
			f.onloadend = function (e) {
				callback(e.target.result);
			}
			f.readAsText(bb);
		}
	};

}());