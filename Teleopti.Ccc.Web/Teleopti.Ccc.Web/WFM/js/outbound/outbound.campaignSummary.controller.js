(function() {

	angular.module('wfm.outbound')
		.controller('OutboundSummaryCtrl', [
			'$scope', '$state', '$stateParams', 'outboundService',
			summaryCtrl
		]);

    function summaryCtrl($scope, $state, $stateParams, outboundService) {
        $scope.isLoadFinished = false;
        $scope.listCampaignFinished = false;
       
    	outboundService.load(function handleSuccess(isLoad) {    		
            init();
            $scope.$watch('activePhaseCode', function(newValue, oldValue) {               
                clearCampaignList();
                outboundService.listFilteredCampaigns(newValue, function success(data) {
                    $scope.Campaigns = data.CampaignsWithoutWarning;
                    $scope.WarningCampaigns = data.CampaignsWithWarning;                  
                    $scope.listCampaignFinished = true;
                    $scope.isLoadFinished = true;
                });
            });
        });


        $scope.activePhaseCode = 4;

        $scope.gotoCreateCampaign = function() {
            $state.go('outbound-create');
        };
		
        $scope.generateChart = function (campaign) {
	        campaign.viewScheduleDiffToggle = false;
	        outboundService.getCampaignVisualization(campaign.Id, function success(data) {
		        campaign.chartData = data;
		        campaign.chart = outboundService.makeGraph(null, '#Chart_' + campaign.Id, campaign.viewScheduleDiffToggle, data);

		        console.log('after drawing', campaign);
	        });	       
        };

        $scope.toggleChartDisplay = function (campaign) {
        	campaign.viewScheduleDiffToggle = !campaign.viewScheduleDiffToggle;

	        console.log('before toggling', campaign);
	        outboundService.makeGraph(campaign.chart, '#Chart_' + campaign.Id, campaign.viewScheduleDiffToggle, campaign.chartData);
	    };

        $scope.show = function (campaign) {
            $state.go('outbound-edit', { Id: campaign.Id });
        };               

        function init() {         
            outboundService.getCampaignStatistics(null, function success(data) {
                $scope.phaseStatistics = data;
            });
        }

        function clearCampaignList() {
            $scope.listCampaignFinished = false;
            $scope.Campaigns = [];
            $scope.WarningCampaigns = [];
        }
    }


})();