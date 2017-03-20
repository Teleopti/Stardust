(function() {
	'use strict';

	angular.module('wfm.people')
		.directive('importPeople', ImportPeopleDirective)
		.controller('ImportPeopleCtrl', ['$timeout', 'PeopleService', 'Toggle', '$translate',  ImportPeopleController]);

	function ImportPeopleDirective() {
		return {
			controller: 'ImportPeopleCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				importOptions: '=?'
			},
			templateUrl: 'app/people/html/importPeople.html'
		};
	};

	function ImportPeopleController($timeout, peopleSvc, toggles, $translate) {
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

		vm.closeImportPeople = function() {
			vm.importOptions.showImportModal = false;
		};

		vm.hasInvalidData = false;

		vm.getFileTemplate = function() {
			if (toggles.Wfm_People_ImportAndCreateAgentFromFile_42528) {
				peopleSvc.downloadFileTemplateAgent()
					.then(function (response) {
						saveFile(response, 'agent_template.xls');
					});
			} else {
				peopleSvc.downloadFileTemplate()
					.then(function (response) {
						saveFile(response, 'user_template.xls');
					});
			}
		};

		vm.successCount = 0;
		vm.warningCount = 0;
		vm.failedCount = 0;
		vm.upload = function (files) {
			if (files && files.length) {
				for (var i = 0; i < files.length; i++) {
					var file = files[i];
					vm.isProcessing = true;
					vm.successCount = 0;
					vm.failedCount = 0;
					vm.hasInvalidData = false;

					if(vm.importOptions.importType == 'user') {
						peopleSvc.uploadUserFromFile(file).then(function (response) {
							vm.isProcessing = false;
							vm.isSuccessful = true;
							var isXlsx = response.headers()['content-type'] == 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
							var blob = new Blob([response.data], {
								type: response.headers()['content-type']
							});
							var processResult = response.headers()['message'].match(/[0-9]+/g);;
							vm.successCount = processResult[0];
							vm.failedCount = processResult[1];
							
							if (vm.failedCount > 0) {
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
					}else if(vm.importOptions.importType == 'agent') {
						peopleSvc.uploadAgentFromFile(file).then(function (response) {
							vm.isProcessing = false;
							vm.isSuccessful = true;
							var isXlsx = response.headers()['content-type'] == 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
							var blob = new Blob([response.data], {
								type: response.headers()['content-type']
							});
							var processResult = response.headers()['message'].match(/[0-9]+/g);;
							vm.successCount = processResult[0];
							vm.failedCount = processResult[1];
							if (processResult[2] !== undefined) {
								vm.warningCount = processResult[2];
							}
							if (vm.failedCount + vm.warningCount > 0 ) {
								vm.hasInvalidData = true;
								var extension = isXlsx ? '.xlsx' : '.xls';
								saveAs(blob, 'invalidAgents' + extension);
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
			}
		};

		vm.getSuccessMessage = function (successCount) {		
			return $translate.instant('SuccessfullyImportedAgents').replace('{0}', successCount);					
		}

		vm.getFailureMessage = function (failureCount) {
			return $translate.instant('FailedToImportAgents').replace('{0}', failureCount);
		}

		vm.getWarningMessage = function(warningCount) {
			return $translate.instant('WarnImportedAgents').replace('{0}', warningCount);
		}

		function arrayBuffer2String(buf, callback) {
			var bb = new Blob([buf]);
			var f = new FileReader();
			f.onloadend = function (e) {
				callback(e.target.result);
			}
			f.readAsText(bb);
		}

		function saveFile(response, filename) {
			var blob = new Blob([response.data], {
				type: response.headers()['content-type']
			});
			saveAs(blob, filename);
		}
	}
}());