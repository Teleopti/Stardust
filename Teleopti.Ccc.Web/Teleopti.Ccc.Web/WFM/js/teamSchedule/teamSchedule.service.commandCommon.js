(function () {

	'use strict';

	angular.module('wfm.teamSchedule')
		.service('CommandCommon', ['$http', '$q', '$filter', '$mdDialog', 'PersonSelection', commandCommon]);

	function commandCommon($http, $q, $filter, $mdDialog, PersonSelection) {

		var checkPersonWriteProtectionUrl = '../api/TeamScheduleCommand/PersonWriteProtectionCheck';

		this.checkPersonWriteProtectionPromise = checkPersonWriteProtectionPromise;
		this.showCommandFailureDetailsDialog = showCommandFailureDetailsDialog;

		this.wrapPersonWriteProtectionCheck = wrapPersonWriteProtectionCheck;

		function checkPersonWriteProtectionPromise(date, agentIds) {
			var dateString = $filter('date')(date, 'yyyy-MM-dd');
			var deferred = $q.defer();
			$http.post(checkPersonWriteProtectionUrl, {
				date: dateString,
				agentIds: agentIds
			}).success(function (data) {
				deferred.resolve(data);
			}).error(function (e) {
				deferred.reject(e);
			});
			return deferred.promise;
		}


		function wrapPersonWriteProtectionCheck(skipDialogIfNormal, commandTitle, action, requirement, selectedDate, getCommandMessage) {

			var getPersons = PersonSelection.getSelectedPersonIdList;
			var date = selectedDate;

			function getFix(writeProtectedPersons) {
				if (writeProtectedPersons.length > 0)
					return {
						targets: writeProtectedPersons,
						action: function () {
							PersonSelection.unselectPersonsWithIds(writeProtectedPersons);
						}
					};
				else
					return null;
			}

			function precheck() {
				if (!requirement) return true;
				return requirement.check();
			}

			function showDialog(fix) {
				if (fix == null && skipDialogIfNormal && precheck()) {
					return action();
				}

				$mdDialog.show({
					controller: 'commandConfirmDialog',
					templateUrl: 'js/teamSchedule/html/commandConfirmDialog.tpl.html',
					parent: angular.element(document.body),
					clickOutsideToClose: true,
					bindToController: true,
					locals: {
						commandTitle: commandTitle,
						fix: fix,
						getTargets: getPersons,
						command: action,
						require: requirement,
						getCommandMessage: getCommandMessage
					}
				});
			}

			function wrapped() {
				checkPersonWriteProtectionPromise(date, getPersons()).then(function (writeProtectedPersons) {
					showDialog(getFix(writeProtectedPersons));
				});
			}

			return wrapped;
		}

		function showCommandFailureDetailsDialog(title, details) {
			return $mdDialog.show({
				controller: commandFailureDetailsDialogCtrl,
				templateUrl: 'js/teamSchedule/html/commandFailureDetailsDialog.tpl.html',
				parent: angular.element(document.body),
				clickOutsideToClose: true,
				bindToController: true,
				locals: {
					dialogTitle: title,
					details: details
				}
			});
		}

	}

	commandFailureDetailsDialogCtrl.$inject = ['$scope', '$mdDialog', 'PersonSelection'];
	function commandFailureDetailsDialogCtrl($scope, $mdDialog, PersonSelection) {
		var ctrl = this;
		$scope.dialogTitle = ctrl.dialogTitle;
		$scope.details = [];
		$scope.cancel = function () { $mdDialog.cancel(); };
		init.apply(ctrl);

		function init() {
			angular.forEach(this.details, function (detail) {
				var uniqueMessages = [];
				angular.forEach(detail.Messages, function (message) {
					if (uniqueMessages.indexOf(message) === -1) {
						uniqueMessages.push(message);
					}
				});
				$scope.details.push({
					PersonName: PersonSelection.personInfo[detail.PersonId].name,
					Messages: uniqueMessages
				});
			});
		}
	}

})();