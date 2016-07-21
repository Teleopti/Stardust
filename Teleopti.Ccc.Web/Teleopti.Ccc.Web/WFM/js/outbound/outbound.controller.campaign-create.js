(function() {
    'use strict';

    angular.module('wfm.outbound')
        .controller('OutboundCreateCtrl', [
            '$scope', '$state', 'outboundService', 'outboundNotificationService', 'outboundViewUtilityService',
            createCtrl
        ]);

    function createCtrl($scope, $state, outboundService, outboundNotificationService, viewUtilityService) {
	   
		outboundService.checkPermission($scope).then(init);
        $scope.init = init;
        $scope.addCampaign = addCampaign;
        $scope.backToList = backToList;

        function addCampaign() {
        	$scope.isFormValidForPage = $scope.isFormValid();
	        checkIsWorkingHoursValid();
	      
	        if (!$scope.isFormValidForPage || !$scope.isWorkingHoursValidForPage ) return;

            $scope.isCreating = true;
	        outboundService.addCampaign($scope.campaign, function (campaign) {
	            outboundNotificationService.notifyCampaignCreationSuccess(angular.copy(campaign));
                backToList();
	        }, function (error) {
	            outboundNotificationService.notifyCampaignCreationFailure(error);
	        });
        }

        function checkIsWorkingHoursValid() {
        	$scope.isWorkingHoursValidForPage = $scope.isWorkingHoursValid();
        }

        function init() {
            $scope.campaign = {
            	Activity: {},
				SpanningPeriod: {
					startDate: new Date(),
					endDate: new Date()
				},              
                WorkingHours: []
            };
	        $scope.isCreating = false;
            $scope.campaignCreated = false;
	        viewUtilityService.setupValidators($scope);
	        viewUtilityService.registerPersonHourFeedback($scope, outboundService);
	        $scope.checkIsWorkingHoursValid = checkIsWorkingHoursValid;
        }

        function backToList() {
            $state.go('outbound.summary');
        }
    }
})();