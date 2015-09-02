(function() {
    'use strict';

    angular.module('wfm.outbound')
        .controller('OutboundEditCtrl', [
            '$scope', '$state', '$stateParams', '$timeout', 'outboundService', 'outboundNotificationService', 'outboundViewUtilityService',
            editCtrl
        ]);


    function editCtrl($scope, $state, $stateParams, $timeout, outboundService, outboundNotificationService, viewUtilityService) {

        init();
        var originalCampaign;
        var muteDirtyWorkingHoursWatcher;

        $scope.init = init;
        $scope.editCampaign = editCampaign;
        $scope.reset = reset;
	     $scope.remove = remove;
	     $scope.backToList = backToList;
	     $scope.showRemoveCampaignConfirmDialog = false;
	     $scope.removeCampaign = removeCampaign;

        function editCampaign() {
            if (!$scope.isInputValid()) {
                $scope.flashErrorIcons();
                return;
            }
            outboundService.editCampaign($scope.campaign, function (campaign) {
                outboundNotificationService.notifyCampaignUpdateSuccess(angular.copy(campaign));
                init();
            }, function (error) {
                outboundNotificationService.notifyCampaignUpdateFailure(error);
            });
        }

        function reset() {
            $scope.campaign = angular.copy(originalCampaign);
            setPristineForms();
        }

		function removeCampaign() {
			$scope.showRemoveCampaignConfirmDialog = true;
		}
        function remove() {
	        $scope.showRemoveCampaignConfirmDialog = false;
			outboundService.removeCampaign($scope.campaign, function() {

			});
		}

        function init() {
            $scope.campagin = null;
            $scope.isCampaignLoaded = function () { return angular.isDefined($scope.campaign); };

            var currentCampaignId = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? $stateParams.Id : null;
            if (currentCampaignId == null) return;

            outboundService.getCampaign(currentCampaignId, function (campaign) {
                originalCampaign = campaign;
                $scope.campaign = angular.copy(campaign);

                $scope.$emit('outbound.campaign.selected', { Id: campaign.Id });

                viewUtilityService.expandAllSections($scope);
                viewUtilityService.registerCampaignForms($scope);
                viewUtilityService.registerPersonHourFeedback($scope, outboundService);
                viewUtilityService.setupValidators($scope);

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
            if ($scope.campaignGeneralForm) $scope.campaignGeneralForm.$setPristine();
            if ($scope.campaignWorkloadForm) $scope.campaignWorkloadForm.$setPristine();
            if ($scope.campaignSpanningPeriodForm) $scope.campaignSpanningPeriodForm.$setPristine();

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
