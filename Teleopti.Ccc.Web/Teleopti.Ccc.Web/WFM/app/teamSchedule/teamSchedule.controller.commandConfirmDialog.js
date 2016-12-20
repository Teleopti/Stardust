(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('CommandConfirmDialogController',
		['$scope', '$mdDialog', '$translate', CommandConfirmDialogController]);


	function CommandConfirmDialogController($scope, $mdDialog, $translate) {
		//I am afraid this is dialog not used at all. 
		var vm = this;

		init();
		
		function init() {
			vm.commandTitle = $scope.commandTitle;
			vm.needFix = false;
			vm.showFixDetail = false;
			vm.toggleFixDetail = toggleFixDetail;
			vm.translatedMessage = message($scope.getTargets().length);
			vm.hasError = false;
			vm.isRemoveCommand = $scope.commandTitle.indexOf('Remove') > -1;
			vm.isRemoveAbsence = $scope.commandTitle === 'RemoveAbsence';
			vm.getCommandMessage = $scope.getCommandMessage;
			if (vm.isRemoveAbsence) {
				vm.removeFullDayAbsence = false;
				vm.toggleRemoveFullDayAbsence = function () {					
					vm.removeFullDayAbsence = !vm.removeFullDayAbsence;
				};
			}

			if ($scope.fix) {
				vm.needFix = true;
				vm.translatedWarningMessage = warningMessage($scope.fix.targets.length);
				vm.fix = getFix($scope.fix.action);
				vm.fixTargets = $scope.fix.targets;
			} else if ($scope.require) {
				checkRequire($scope.getTargets().length);
			}
			
			vm.apply = getApply($scope.command);
			vm.hide = function () { vm.showModal = false; };
			vm.cancel = function () { vm.showModal = false; };

		}

		function message(targetsLength) {
		    return $translate.instant('YouHaveSelectedXAgent').replace('{0}', targetsLength);
		}

		function warningMessage(targetsLength) {
			return $translate.instant('PersonsAreWriteProtected').replace('{0}', targetsLength);
		}

		function errorMessage(msg) {
			return $translate.instant(msg);
		}

		function getApply(action) {
			return function () {
				if (vm.isRemoveAbsence) {
					action(vm.removeFullDayAbsence);
				} else {
					action();
				}

				vm.showModal = false;
			};
		}

		function checkRequire(targetsLength) {
			if ($scope.require) {
				if (!$scope.require.check(targetsLength)) {
					vm.translatedErrorMessage = errorMessage($scope.require.message);
					vm.hasError = true;
					vm.needFix = false;
				}
			} else {
				if (targetsLength <= 0) {
					vm.translatedErrorMessage = errorMessage('MustSelectAtLeastOneAgent');
					vm.hasError = true;
					vm.needFix = false;
				}
			}
		}

		function getFix(action) {
			return function () {
				action(vm.removeFullDayAbsence);
				vm.needFix = false;
				vm.translatedMessage = message($scope.getTargets().length);
				checkRequire($scope.getTargets().length);
			};
		}

		function toggleFixDetail() {			
			vm.showFixDetail = !vm.showFixDetail;
		}
	}
})();