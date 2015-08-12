(function() {

	angular.module('wfm.outbound')
		.controller('OutboundSummaryCtrl', [
			'$scope', '$state', '$stateParams', 'outboundService', 'outboundChartService',  '$filter',
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

		$scope.getGraphData = function(campaign) {
			outboundChartService.getCampaignVisualization(campaign.Id, function(data, translations) {
				campaign.graphData = data;
				campaign.translations = translations;
			});
		}

		$scope.showManualPlanContainer = function (campaign) {
			campaign.showManualPlanContainer = true;
			//campaign.manualPlanSwitch = true;
		}

		$scope.delThisDate = function(campaign,d) {
			var index = campaign.manualPlan.selectedDates.indexOf(d);
			campaign.manualPlan.selectedDates.splice(index, 1);
		}

		$scope.addManualPlan = function (campaign) {
			campaign.showManualPlanContainer = false;
			campaign.manualPlan.Id = campaign.Id;
			console.log(campaign.manualPlan);
			//outboundService.sendManualPlan(campaign.manualPlan, function(campaign) {
			//	outboundNotificationService.notifyManualPlanModifySuccess(angular.copy(campaign));
			//  updateChartPlanDate
			//}, function(error) {
			//	outboundNotificationService.notifyManualPlanModifyFailure(error);
			//});
			campaign.manualPlan = {
				selectedDates:[]
			};
		}

		$scope.isOverStaffing = function(d) {
			var result = d.filter(function(e) {
				return (e.TypeOfRule == 'OutboundOverstaffRule') ? true : false;
			}).length > 0;
			return result;

		};

    	$scope.hideWhenDone=function(d) {
    		return ($filter('showPhase')(d) == 'Done') ? true : false;
	    }

        $scope.activePhaseCode = 4;

        $scope.gotoCreateCampaign = function() {
            $state.go('outbound-create');
        };

        $scope.toggleChartDisplay = function (campaign) {
        	campaign.viewScheduleDiffToggle = !campaign.viewScheduleDiffToggle;
        	//outboundChartService.makeGraph(campaign.chart, campaign, campaign.viewScheduleDiffToggle, campaign.chartData);
	    };

        $scope.show = function (campaign) {
            $state.go('outbound-edit', { Id: campaign.Id });
        };

		$scope.goToProductionPlan = function(campaign) {
			$state.go('outbound-production-plan', { Id: campaign.Id });
		};

	    $scope.displayLoading = displayLoading;

        function init() {         
            outboundService.getCampaignStatistics(null, function success(data) {
                $scope.phaseStatistics = data;
            });
        }

        function displayLoading(campaign) {
            return !angular.isDefined(campaign.graphData);
        }

        function clearCampaignList() {
            $scope.listCampaignFinished = false;
            $scope.Campaigns = [];
            $scope.WarningCampaigns = []; 
        }
    }


})();