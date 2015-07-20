(function() {

    angular.module('wfm.outbound')
		.controller('OutboundSummaryCtrl', [
			'$scope', '$state', '$stateParams', 'outboundService',
			summaryCtrl
		])

    function summaryCtrl($scope, $state, $stateParams, outboundService) {

    	init();
    	$scope.activePhaseCode = 4;

	    $scope.$watch('activePhaseCode', function(newValue, oldValue) {
	    	outboundService.listFilteredCampaigns(newValue, function success(data) {
	    		$scope.Campaigns = data.CampaignsWithoutWarning;
	    		$scope.CampaignsLength = data.CampaignsWithoutWarning.length;
	    		$scope.WarningCampaigns = data.CampaignsWithWarning;
	    		$scope.WarningCampaignsLength = data.CampaignsWithWarning.length;
		    });
	    });

        $scope.gotoCreateCampaign = function () {
            $state.go('outbound-create');
        };
        $scope.generateChart = function () {
            var chart = c3.generate({
                bindto: '#chart',
                data: {
                    columns: [
					  ['data1', 30, 200, 100, 300, 150, 250],
					  ['data2', 50, 20, 10, 40, 15, 25]
                    ]
                }
            });
        };

        function init() {

            outboundService.getCampaignStatistics(null, function success(data) {
            	$scope.phaseStatistics = data;
            });

        }
    }



})();