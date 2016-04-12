(function () {

	'use strict';

	angular.module('wfm.teamSchedule')
		.service('CommandCommon', ['$http', '$q', '$filter', '$mdDialog', 'PersonSelection', commandCommon]);

	function commandCommon($http, $q, $filter, $mdDialog, PersonSelection) {

		var checkPersonWriteProtectionUrl = '../api/TeamScheduleCommand/PersonWriteProtectionCheck';

		this.checkPersonWriteProtectionPromise = checkPersonWriteProtectionPromise;

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


		function wrapPersonWriteProtectionCheck(skipDialogIfNormal, commandTitle, action, requirement, selectedDate, getRemoveAbsenceMessage) {

			var getPersons = PersonSelection.getSelectedPersonIdList;
			var date = selectedDate? selectedDate: PersonSelection.scheduleDate;
			
			function getFix(writeProtectedPersons) {
				if (writeProtectedPersons.length > 0)
					return {
						targets: writeProtectedPersons,
						action: function() { PersonSelection.unselectPersonsWithIds(writeProtectedPersons); }
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
						getRemoveAbsenceMessage: getRemoveAbsenceMessage
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

	}


})();