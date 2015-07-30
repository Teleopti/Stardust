(function() {

	angular.module('wfm.outbound')
		.controller('OutboundSummaryCtrl', [
			'$scope', '$state', '$stateParams', 'outboundService', 'outboundChartService', '$filter',
			summaryCtrl
		]);

	function summaryCtrl($scope, $state, $stateParams, outboundService, outboundChartService, $filter) {
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

    	$scope.isOverStaffing=function(d) {
    		var result = d.filter(function (e) {
			    return (e.TypeOfRule == 'OutboundOverstaffRule') ? true : false;
    		}).length > 0;
		    return result;

	    }

    	$scope.hidePlannedAnalyzeButton = function (d) {
		   return ($filter('showPhase')(d) == 'Planned') ? false : true;
    	};

    	$scope.hideWhenDone=function(d) {
    		return ($filter('showPhase')(d) == 'Done') ? true : false;
	    }

        $scope.activePhaseCode = 4;

        $scope.gotoCreateCampaign = function() {
            $state.go('outbound-create');
        };
		
        $scope.generateChart = function (campaign) {           
            if (campaign.chart) return;
            outboundChartService.getCampaignVisualization(campaign.Id, function success(data) {
                campaign.viewScheduleDiffToggle = false;
	        	campaign.chartData = data;
	        	campaign.chart = outboundChartService.makeGraph(null, campaign , campaign.viewScheduleDiffToggle, data);
	        });	       
        };

        $scope.toggleChartDisplay = function (campaign) {
        	campaign.viewScheduleDiffToggle = !campaign.viewScheduleDiffToggle;
        	outboundChartService.makeGraph(campaign.chart, campaign, campaign.viewScheduleDiffToggle, campaign.chartData);
	    };

        $scope.show = function (campaign) {
            $state.go('outbound-edit', { Id: campaign.Id });
        };

	    $scope.displayLoading = displayLoading;

        function init() {         
            outboundService.getCampaignStatistics(null, function success(data) {
                $scope.phaseStatistics = data;
            });
        }

        function displayLoading(campaign) {
            return !angular.isDefined(campaign.chart);
        }

        function clearCampaignList() {
            $scope.listCampaignFinished = false;
            $scope.Campaigns = [];
            $scope.WarningCampaigns = []; 
        }
    }


})();