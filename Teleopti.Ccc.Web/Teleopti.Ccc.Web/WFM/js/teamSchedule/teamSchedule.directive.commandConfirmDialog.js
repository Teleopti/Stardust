(function() {
	'use strict';

	angular.module('wfm.teamSchedule').controller('commandConfirmDialog',
		['$scope', '$mdDialog', '$translate', 'PersonSelection', 'CommandCommon', commandConfirmDialog]);


	function commandConfirmDialog($scope, $mdDialog, $translate, PersonSelection, CommandCommon) {

		var ctrl = this;		

		init();
		
		function init() {
			$scope.commandTitle = ctrl.commandTitle;
			$scope.needFix = false;
			$scope.showFixDetail = false;
			$scope.toggleFixDetail = toggleFixDetail;
			$scope.translatedMessage = message(ctrl.getTargets().length);

			if (ctrl.fix) {
				$scope.needFix = true;
				$scope.translatedWarningMessage = warningMessage(ctrl.fix.targets.length);
				$scope.fix = getFix(ctrl.fix.action);
				$scope.fixTargets = ctrl.fix.targets;			
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

		function getApply(action) {
			return function() {
				action();
				$mdDialog.hide();
			};
		}

		function getFix(action) {
			return function() {
				action();
				$scope.needFix = false;
				$scope.translatedMessage = message(ctrl.getTargets().length);
			};
		}

		function toggleFixDetail() {			
			$scope.showFixDetail = !$scope.showFixDetail;
		}
	}



})();