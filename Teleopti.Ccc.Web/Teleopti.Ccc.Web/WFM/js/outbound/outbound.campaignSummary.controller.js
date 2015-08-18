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

    	$scope.clearManualPlan = function (campaign) {
    		campaign.selectedDates = [];
		    campaign.manualPlanInput = null;
    	}

    	$scope.removeManualPlan = function (campaign) {
		    var dates = [];
		    campaign.selectedDates.forEach(function(date,index) {
			    dates[index] = { Date: date };
		    });
		    var removeManualPlan = {
			    CampaignId: campaign.Id,
			    Dates: dates
			    
	    };
		    outboundChartService.removeManualPlan(removeManualPlan, function (data, manualPlan) {
		    	outboundService.getCampaignSummary(campaign.Id, function (_campaign) {
		    		angular.extend(campaign, _campaign);
		    		campaign.graphData = data;
		    		campaign.rawManualPlan = manualPlan;
		    	});
		    });
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
    		
    		outboundChartService.updateManualPlan(campaign.manualPlan, function (data, manualPlan) {
			    outboundService.getCampaignSummary(campaign.Id, function(_campaign) {
			    	angular.extend(campaign, _campaign);
			    	campaign.graphData = data;
			    	campaign.rawManualPlan = manualPlan;
			    });
			   
		    }, function (error) {
    		});
	    }

		$scope.getGraphData = function(campaign) {
			outboundChartService.getCampaignVisualization(campaign.Id, function(data, translations, manualPlan) {
				campaign.graphData = data;
				campaign.rawManualPlan = manualPlan;
				campaign.translations = translations;
			});
		}

		$scope.isManualProductionPlanInvalid = function (campaign) {
			return !(angular.isDefined(campaign.manualPlanInput) && campaign.manualPlanInput != null && campaign.manualPlanInput >= 0);
		}

		$scope.switchBacklog = function(campaign) {
			campaign.backlogSwitch = !campaign.backlogSwitch;
			campaign.manualPlanswitch = false;
			campaign.switchSwitch = !campaign.switchSwitch;
		}
		
		$scope.switchManualPlan = function (campaign) {
			campaign.manualPlanswitch = !campaign.manualPlanswitch;
			campaign.backlogSwitch = false;
			campaign.switchSwitch = !campaign.switchSwitch;
		}

		$scope.delThisDate = function(campaign,d) {
			var index = campaign.selectedDates.indexOf(d);
			campaign.selectedDates.splice(index, 1);

		}

		$scope.addBacklog = function(campaign) {
			campaign.backlogSwitch = false;
			campaign.switchSwitch = false;
			campaign.backlog.Id = campaign.Id;
			campaign.selectedDates = [];
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