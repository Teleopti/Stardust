(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('commandConfirmDialog',
		['$scope', '$mdDialog', '$translate', commandConfirmDialog]);


	function commandConfirmDialog($scope, $mdDialog, $translate) {

		var ctrl = this;		

		init();
		
		function init() {
			$scope.commandTitle = ctrl.commandTitle;
			$scope.needFix = false;
			$scope.showFixDetail = false;
			$scope.toggleFixDetail = toggleFixDetail;
			$scope.translatedMessage = message(ctrl.getTargets().length);
			$scope.hasError = false;

			if (ctrl.fix) {
				$scope.needFix = true;
				$scope.translatedWarningMessage = warningMessage(ctrl.fix.targets.length);
				$scope.fix = getFix(ctrl.fix.action);
				$scope.fixTargets = ctrl.fix.targets;
			} else if (ctrl.require) {
				checkRequire(ctrl.getTargets().length);
			}
			
			$scope.apply = getApply(ctrl.command);
			$scope.hide = function () { $mdDialog.hide(); };
			$scope.cancel = function () { $mdDialog.cancel(); };

		}

		function message(targetsLength) {
			return $translate.instant('ChangeWillAffectAgents').replace('{0}', targetsLength);
		}

		function warningMessage(targetsLength) {
			return $translate.instant('PersonsAreWriteProtected').replace('{0}', targetsLength);
		}

		function errorMessage(msg) {
			return $translate.instant(msg);
		}

		function getApply(action) {
			return function() {
				action();
				$mdDialog.hide();
			};
		}

		function checkRequire(targetsLength) {
			if (ctrl.require) {
				if (!ctrl.require.check(targetsLength)) {
					$scope.translatedErrorMessage = errorMessage(ctrl.require.message);
					$scope.hasError = true;
					$scope.needFix = false;
				}
			} else {
				if (targetsLength <= 0) {
					$scope.translatedErrorMessage = errorMessage('MustSelectAtLeastOneAgent');
					$scope.hasError = true;
					$scope.needFix = false;
				}
			}
		}

		function getFix(action) {
			return function() {
				action();
				$scope.needFix = false;
				$scope.translatedMessage = message(ctrl.getTargets().length);
				checkRequire(ctrl.getTargets().length);
			};
		}

		function toggleFixDetail() {			
			$scope.showFixDetail = !$scope.showFixDetail;
		}
	}



})();