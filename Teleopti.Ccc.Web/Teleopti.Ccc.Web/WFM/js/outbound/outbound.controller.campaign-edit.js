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
            //if (!$scope.isInputValid()) {
            //    $scope.flashErrorIcons();
            //    return;
            //}
            outboundService.editCampaign($scope.campaign, function (campaign) {
                outboundNotificationService.notifyCampaignUpdateSuccess(angular.copy(campaign));
                init();
            }, function (error) {
                outboundNotificationService.notifyCampaignUpdateFailure(error);
            });
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
           // $scope.isCampaignLoaded = function () { return angular.isDefined($scope.campaign); };

            var currentCampaignId = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? $stateParams.Id : null;
            if (currentCampaignId == null) return;

            outboundService.getCampaign(currentCampaignId, function (campaign) {
	            console.log('get campaign', campaign);
                originalCampaign = campaign;
                $scope.campaign = angular.copy(campaign);

                $scope.$emit('outbound.campaign.selected', { Id: campaign.Id });

                //viewUtilityService.expandAllSections($scope);
               // viewUtilityService.registerCampaignForms($scope);
                
                viewUtilityService.setupValidators($scope);
                viewUtilityService.registerPersonHourFeedback($scope, outboundService);

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
            //if ($scope.campaignGeneralForm) $scope.campaignGeneralForm.$setPristine();
            //if ($scope.campaignWorkloadForm) $scope.campaignWorkloadForm.$setPristine();
        	//if ($scope.campaignSpanningPeriodForm) $scope.campaignSpanningPeriodForm.$setPristine();

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
