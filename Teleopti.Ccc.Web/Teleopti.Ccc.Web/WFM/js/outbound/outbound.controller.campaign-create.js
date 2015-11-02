(function() {
    'use strict';

    angular.module('wfm.outbound')
        .controller('OutboundCreateCtrl', [
            '$scope', '$state', 'outboundService', 'outboundNotificationService', 'outboundViewUtilityService',
            createCtrl
        ]);

    function createCtrl($scope, $state, outboundService, outboundNotificationService, viewUtilityService) {
	   
		outboundService.checkPermission($scope).then(init);
	  
		$scope.preventAutomaticRedirect = false;
        $scope.addCampaign = addCampaign;
        $scope.init = init;
        $scope.backToList = backToList;

        function addCampaign() {
            if (!$scope.isInputValid()) {
                $scope.flashErrorIcons();
                return;
            }

            $scope.isCreating = true;

            outboundService.addCampaign($scope.campaign, function (campaign) {
                outboundNotificationService.notifyCampaignCreationSuccess(angular.copy(campaign));
                init();
                if (!$scope.preventAutomaticRedirect) show(campaign);
            }, function (error) {
                outboundNotificationService.notifyCampaignCreationFailure(error);
            });
        }

        function init() {
            $scope.campaign = {
                Activity: {},
                StartDate: { Date: new Date() },
                EndDate: { Date: new Date() },
                WorkingHours: []
            };

            $scope.isCreating = false;	        

            viewUtilityService.expandAllSections($scope);
            viewUtilityService.registerCampaignForms($scope);
            viewUtilityService.registerPersonHourFeedback($scope, outboundService);
            viewUtilityService.setupValidators($scope);
        }

        function show(campaign) {
            $state.go('outbound.edit', { Id: campaign.Id });
        }

        function backToList() {
            $state.go('outbound.summary');
        }

    }



})();