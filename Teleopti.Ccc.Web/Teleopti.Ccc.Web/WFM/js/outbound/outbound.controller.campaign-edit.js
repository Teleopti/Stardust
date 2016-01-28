(function() {
    'use strict';

    angular.module('wfm.outbound')
        .controller('OutboundEditCtrl', [
            '$scope', '$state', '$stateParams', '$timeout', 'outboundService', 'outboundNotificationService', 'outboundViewUtilityService',
            editCtrl
        ]);


    function editCtrl($scope, $state, $stateParams, $timeout, outboundService, outboundNotificationService, viewUtilityService) {

    	outboundService.checkPermission($scope).then(init);

        var originalCampaign;
        var muteDirtyWorkingHoursWatcher;

        $scope.init = init;
        $scope.editCampaign = editCampaign;
        $scope.reset = reset;
	    $scope.remove = remove;
	    $scope.backToList = backToList;
	    $scope.showRemoveCampaignConfirmDialog = false;
	    $scope.removeCampaign = removeCampaign;
		$scope.cancelRemoveCampaign = cancelRemoveCampaign;

		function editCampaign() {
			$scope.isFormValidForPage = $scope.isFormValid();
			checkIsWorkingHoursValid();
			$scope.isCampaignDurationValidForPage = $scope.isCampaignDurationValid();
			if (!$scope.isFormValidForPage || !$scope.isWorkingHoursValidForPage || !$scope.isCampaignDurationValidForPage) return;
			$scope.isEditing = true;
            outboundService.editCampaign($scope.campaign, function (campaign) {
                outboundNotificationService.notifyCampaignUpdateSuccess(angular.copy(campaign));
                init();
            }, function (error) {
                outboundNotificationService.notifyCampaignUpdateFailure(error);
            });
		}

		function checkIsWorkingHoursValid() {
			$scope.isWorkingHoursValidForPage = $scope.isWorkingHoursValid();
		}

        function reset() {
        	$scope.campaign = angular.copy(originalCampaign);
	        $scope.campaign.spanningPeriodErrors = [];
            setPristineForms();
        }

		function removeCampaign() {
			$scope.showRemoveCampaignConfirmDialog = true;
		}

		function cancelRemoveCampaign() {
			$scope.showRemoveCampaignConfirmDialog = false;
		}

        function remove() {
	        $scope.showRemoveCampaignConfirmDialog = false;
			outboundService.removeCampaign($scope.campaign, function() {
				$scope.backToList();
			}, function(error) {
				outboundNotificationService.notifyCampaignRemoveFailure(error);
			});
		}

        function init() {
            var currentCampaignId = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? $stateParams.Id : null;
            if (currentCampaignId == null) return;
            $scope.isEditing = false;
            outboundService.getCampaign(currentCampaignId, function (campaign) {
                originalCampaign = campaign;
                $scope.campaign = angular.copy(campaign);

                viewUtilityService.setupValidators($scope);
                viewUtilityService.registerPersonHourFeedback($scope, outboundService);
                $scope.checkIsWorkingHoursValid = checkIsWorkingHoursValid;
                setPristineForms();

                $scope.$watch(function () {
                    return $scope.campaign.WorkingHours;
                }, function (newValue, oldValue) {
                    if (newValue !== oldValue && !muteDirtyWorkingHoursWatcher) {
                        $scope.dirtyWorkingHours = true;
                    }
                }, true);

            }, function () {
                outboundNotificationService.notifyCampaignLoadingFailure({ Message: currentCampaignId });
            });
        }

        function setPristineForms() {
	        if ($scope.form) $scope.form.$setPristine();

            muteDirtyWorkingHoursWatcher = true;
            $scope.dirtyWorkingHours = false;
            $timeout(function () {
                muteDirtyWorkingHoursWatcher = false;
            });
        }

        function backToList() {
            $state.go('outbound.summary');
        }
    }
})();
