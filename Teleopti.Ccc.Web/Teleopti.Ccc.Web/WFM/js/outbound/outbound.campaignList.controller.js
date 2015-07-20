(function() {

    angular.module('wfm.outbound')
		.controller('OutboundListCtrl', [
			'$scope', '$state', '$stateParams', 'outboundService',
			listCtrl
		])

    function listCtrl($scope, $state, $stateParams, outboundService) {

        init();

        $scope.gotoCreateCampaign = function () {
            $state.go('outbound-create');
        };

        $scope.show = function (campaign) {
            if (angular.isDefined(campaign)) $scope.currentCampaignId = campaign.Id;
            $state.go('outbound.edit', { Id: $scope.currentCampaignId });
        };

        function init() {
            $scope.campaigns = [];

            $scope.currentCampaignId = null;
            $scope.$on('outbound.campaign.selected', function (e, data) {
                $scope.currentCampaignId = data.Id;
            });

            outboundService.listCampaign(null, function success(campaigns) {
                $scope.campaigns = campaigns;
            });
        }
    }



})();