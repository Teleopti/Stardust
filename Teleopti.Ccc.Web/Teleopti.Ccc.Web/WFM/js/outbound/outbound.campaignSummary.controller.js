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
	                $scope.Campaigns = data;
	                $scope.listCampaignFinished = true;
	                $scope.isLoadFinished = true;
                });
            });
    	});

    	$scope.replan = function (campaign) {
    		campaign.isLoadingData = true;
    		outboundChartService.replan(campaign.Id, function () {
			    refreshGraphData(campaign, $scope);
			    refreshCampaignStatistics($scope);
		    });
		}

    	

    	$scope.removeManualPlan = function(campaign) {
			var dates = [];
			campaign.selectedDates.forEach(function(date, index) {
				dates[index] = { Date: date };
			});
			var removeManualPlan = {
				CampaignId: campaign.Id,
				Dates: dates
			};

			campaign.isLoadingData = true;
			outboundChartService.removeManualPlan(removeManualPlan, function() {
				refreshGraphData(campaign, $scope);
				refreshCampaignStatistics($scope);
			});
		    $scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
			campaign.manualPlanInput = null;
		}

		$scope.addManualPlan = function (campaign) {
    		campaign.manualPlan.CampaignId = campaign.Id;
    		campaign.manualPlan.ManualProductionPlan = [];
    		campaign.selectedDates.forEach(function (date, index) {
    			campaign.manualPlan.ManualProductionPlan[index] = {
    				Date: {Date: date},
    				Time: campaign.manualPlanInput
    			}
    		});
    		campaign.isLoadingData = true;
    		outboundChartService.updateManualPlan(campaign.manualPlan, function (data, manualPlan) {
    			refreshGraphData(campaign, $scope);
    			refreshCampaignStatistics($scope);
		    });
    		$scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
    		campaign.manualPlanInput = null;
	    }

		$scope.getGraphData = function(campaign) {
			outboundChartService.getCampaignVisualization(campaign.Id, function(data, translations, manualPlan, closedDays) {
				campaign.graphData = data;
				campaign.rawManualPlan = manualPlan;
				campaign.translations = translations;
				campaign.closedDays = closedDays;
			});
		}

		$scope.isManualProductionPlanInvalid = function (campaign) {
			return !(angular.isDefined(campaign.manualPlanInput) && campaign.manualPlanInput != null && campaign.manualPlanInput >= 0);
		}
		
		$scope.switchManualPlan = function (campaign) {
			campaign.manualPlanswitch = !campaign.manualPlanswitch;
			campaign.backlogSwitch = false;
			campaign.manualPlanInput = null;
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
            $state.go('outbound.create');
        };

        $scope.toggleChartDisplay = function (campaign) {
        	campaign.viewScheduleDiffToggle = !campaign.viewScheduleDiffToggle;
	    };

        $scope.show = function (campaign) {
            $state.go('outbound.edit', { Id: campaign.Id });
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

        function refreshGraphData(campaign, scope) {
        	outboundService.getCampaignSummary(campaign.Id, function (_campaign) {
        		angular.extend(campaign, _campaign);
        		outboundChartService.getCampaignVisualization(campaign.Id, function (data, translations, manualPlan, closedDays) {
        			campaign.graphData = data;
        			campaign.rawManualPlan = manualPlan;
        			campaign.translations = translations;
        			campaign.closedDays = closedDays;
        			campaign.isLoadingData = false;
        			scope.$broadcast('campaign.chart.refresh', campaign);
		        });
        	});
        }

        function refreshCampaignStatistics(scope) {
        	outboundService.getCampaignStatistics(null, function success(data) {
        		scope.phaseStatistics = data;
        	});
        }

    }
})();