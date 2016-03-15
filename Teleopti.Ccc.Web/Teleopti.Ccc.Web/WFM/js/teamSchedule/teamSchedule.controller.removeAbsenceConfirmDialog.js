'use strict';

(function () {
	angular.module('wfm.teamSchedule')
		.controller('RemoveAbsenceConfirmDialogController', function ($scope, $uibModalInstance, data) {
			$scope.header = data.header;
			$scope.message = data.message;
			$scope.removeEntireCrossDayAbsence = data.removeEntireCrossDayAbsence;

			$scope.toggleOpton = function () {
				$scope.removeEntireCrossDayAbsence = !$scope.removeEntireCrossDayAbsence;
			}

			$scope.no = function () {
				$uibModalInstance.dismiss($scope.removeEntireCrossDayAbsence);
			};

			$scope.yes = function () {
				$uibModalInstance.close($scope.removeEntireCrossDayAbsence);
			};
		});
}());