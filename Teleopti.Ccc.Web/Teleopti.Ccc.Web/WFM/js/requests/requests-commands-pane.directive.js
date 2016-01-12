(function() {
	'use strict';

	angular.module('wfm.requests')
		.controller('requestsCommandsPaneCtrl', requestsCommandsPaneCtrl)
		.directive('requestsCommandsPane', requestsCommandsPaneDirective)


	requestsCommandsPaneCtrl.$inject = ['requestsDefinitions', 'requestsDataService', 'requestCommandParamsHolder'];

	function requestsCommandsPaneCtrl(requestsDefinitions, requestsDataService, requestCommandParamsHolder) {
		var vm = this;

		vm.approveRequests = approveRequests;
		vm.denyRequests = denyRequests;
		vm.disableCommands = disableCommands;

		function approveRequests() {
			var selectedRequestIds = requestCommandParamsHolder.getSelectedRequestsIds();
			if (!selectedRequestIds || selectedRequestIds.length === 0) return;
			if (vm.beforeCommand && !vm.beforeCommand()) return;

			var commandInProgress = requestsDataService.approveRequestsPromise(selectedRequestIds);
			if (vm.afterCommandSuccess) {
				commandInProgress.success(function (changedRequestIds) {					
					vm.afterCommandSuccess({
						commandType: requestsDefinitions.REQUEST_COMMANDS.Approve, 
						changedRequestsCount: changedRequestIds.length,
						requestsCount: selectedRequestIds.length
					});
				});
			}
			if (vm.afterCommandError) {				
				commandInProgress.error(vm.afterCommandError);
			}
		}

		function denyRequests() {
			var selectedRequestIds = requestCommandParamsHolder.getSelectedRequestsIds();
			if (!selectedRequestIds || selectedRequestIds.length === 0) return;
			if (vm.beforeCommand && !vm.beforeCommand()) return;
			var commandInProgress = requestsDataService.denyRequestsPromise(selectedRequestIds);
			if (vm.afterCommandSuccess) {
				commandInProgress.success(function (changedRequestIds) {
					vm.afterCommandSuccess({
						commandType: requestsDefinitions.REQUEST_COMMANDS.Deny,
						changedRequestsCount: changedRequestIds.length,
						requestsCount: selectedRequestIds.length
					});
				});
			}
			if (vm.afterCommandError) {				
				commandInProgress.error(vm.afterCommandError);
			}				
		}

		function disableCommands() {
			var selectedRequestIds = requestCommandParamsHolder.getSelectedRequestsIds();
			if (vm.commandsDisabled) return true;
			return !selectedRequestIds || selectedRequestIds.length === 0;
		}
	}

	function requestsCommandsPaneDirective() {
		return {
			controller: 'requestsCommandsPaneCtrl',
			controllerAs: 'requestsCommandsPane',
			bindToController: true,
			scope: {
				beforeCommand: '&?',
				afterCommandSuccess: '&?',
				afterCommandError: '&?',
				commandsDisabled: '=?'
			},
			restrict: 'E',
			templateUrl: 'js/requests/html/requests-commands-pane.tpl.html'			
		};
	}


})();