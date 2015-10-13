(function() {

	angular.module('wfm.outbound')
		.controller('OutboundListCardsCtrl', [
			'$scope',
			'$state',
			'$stateParams',
			'outboundService',
			'outboundChartService',
			'$filter',
			'outboundNotificationService',
			'OutboundToggles',
			outboundListCardsCtrl
		]);

	function outboundListCardsCtrl($scope, $state, $stateParams, outboundService, outboundChartService, $filter, outboundNotificationService, OutboundToggles) {
		
		$scope.initialized = false;
		$scope.isGanttEnabled = false;

		var scheduleLoader;
		var getCampaignStatistics;
		var listFilteredCampaigns;

		$scope.$watch(function () {
			return OutboundToggles.ready;
		}, function (value) {
			if (value) {
				$scope.isGanttEnabled = OutboundToggles.isGanttEnabled();

				if (!$scope.isGanttEnabled) {
					scheduleLoader = $scope.isGanttEnabled ? outboundService.loadWithinPeriod : outboundService.load;
					getCampaignStatistics = $scope.isGanttEnabled ? outboundService.getCampaignStatisticsWithinPeriod : outboundService.getCampaignStatistics;
					listFilteredCampaigns = $scope.isGanttEnabled ? outboundService.listFilteredCampaignsWithinPeriod : outboundService.listFilteredCampaigns;
					init();
				}			
			}
		});

		$scope.isLoadFinished = false;
        $scope.listCampaignFinished = false;

        function init() {

        	scheduleLoader(function handleSuccess(isLoad) {
        		getCampaignStatistics(null, function success(data) {
        			$scope.phaseStatistics = data;
        		});
        		$scope.$watch('activePhaseCode', function (newValue, oldValue) {
        			clearCampaignList();
        			listFilteredCampaigns(newValue, function success(data) {
        				$scope.Campaigns = data;
        				$scope.listCampaignFinished = true;
        				$scope.isLoadFinished = true;
        			});
        		});
        	});
        }

    	$scope.replan = function (campaign) {
    		campaign.isLoadingData = true;
    		outboundChartService.replan(campaign.Id, function () {
			    refreshGraphData(campaign, $scope);
			    refreshCampaignStatistics($scope);
			    outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
		    });
		}
    	

    	$scope.removeManualPlan = function(campaign) {			
			campaign.isLoadingData = true;
			outboundChartService.removeManualPlan({
				campaignId: campaign.Id,
				selectedDates: campaign.selectedDates
			}, function() {
				refreshGraphData(campaign, $scope);
				refreshCampaignStatistics($scope);
				outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
			});
		    $scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
			campaign.manualPlanInput = null;
    	}

    	$scope.removeBacklog = function (campaign) {    	
    		campaign.isLoadingData = true;
    		outboundChartService.removeActualBacklog({
    			campaignId: campaign.Id,
				selectedDates: campaign.selectedDates
		    }, function () {
    			refreshGraphData(campaign, $scope);
    			refreshCampaignStatistics($scope);
    			outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
    		});
    		$scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
    		campaign.backlogInput = null;
    	}

    	$scope.addBacklog = function (campaign) {    	
    		campaign.isLoadingData = true;
		    outboundChartService.updateBacklog({
			    campaignId: campaign.Id,
			    selectedDates: campaign.selectedDates,
			    manualBacklogInput: campaign.backlogInput
			}, function () {
    			refreshGraphData(campaign, $scope);
    			refreshCampaignStatistics($scope);
    			outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
    		});
    		$scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
    		campaign.backlogInput = null;
    	}

		function excludeClosedDays(days, closedDays) {
			return days.filter(function(d) {
				return closedDays.indexOf(d) < 0;
			});
		}

		$scope.addManualPlan = function (campaign) {
    		campaign.isLoadingData = true;
    		outboundChartService.updateManualPlan({
    			campaignId: campaign.Id,
    			selectedDates: excludeClosedDays(campaign.selectedDates, campaign.selectedDatesClosed),
				manualPlanInput: campaign.manualPlanInput
		    }, function () {
    			refreshGraphData(campaign, $scope);
    			refreshCampaignStatistics($scope);
    			outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
		    });
    		$scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
    		campaign.manualPlanInput = null;
	    }

		$scope.getGraphData = function(campaign) {
			outboundChartService.getCampaignVisualization(campaign.Id, function(data, translations, manualPlan, closedDays, backlog) {
				campaign.graphData = data;
				campaign.rawManualPlan = manualPlan;
				campaign.isManualBacklog = backlog;
				campaign.translations = translations;
				campaign.closedDays = closedDays;
			});
		}

		$scope.isManualProductionPlanInvalid = function (campaign) {
			return !(angular.isDefined(campaign.manualPlanInput) && campaign.manualPlanInput != null && campaign.manualPlanInput >= 0 && campaign.selectedDates && campaign.selectedDates.length > 0);
		}

		$scope.isManualBacklogInvalid = function (campaign) {
			return !(angular.isDefined(campaign.backlogInput) && campaign.backlogInput != null && campaign.backlogInput >= 0 && campaign.selectedDates && campaign.selectedDates.length > 0);
		}

		$scope.showDateSelectionHint = function (campaign) {
			if (campaign.isLoadingData) return false;
			if (campaign.selectedDates && campaign.selectedDates.length > 0) return false;
			if (campaign.manualPlanswitch) return !!campaign.manualPlanInput;
			if (campaign.backlogSwitch) return !!campaign.backlogInput;
			return false;						
		}
		
		$scope.switchManualPlan = function (campaign) {
			campaign.manualPlanswitch = !campaign.manualPlanswitch;
			campaign.backlogSwitch = false;
			campaign.manualPlanInput = null;
		}

		$scope.switchBacklog = function(campaign) {
			campaign.backlogSwitch = !campaign.backlogSwitch;
			campaign.manualPlanswitch = false;
			campaign.backlogInput = null;
		}

		$scope.isOverStaffing = function(d) {
			var result = d.filter(function(e) {
				return (e.TypeOfRule == 'CampaignOverstaff') ? true : false;
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

		$scope.$on('$destroy', function() {
			if (c3.restoreFix) c3.restoreFix();
		});

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
        		outboundChartService.getCampaignVisualization(campaign.Id, function (data, translations, manualPlan, closedDays, backlog) {        		
        			campaign.graphData = data;
        			campaign.rawManualPlan = manualPlan;
        			campaign.isManualBacklog = backlog;
        			campaign.translations = translations;
        			campaign.closedDays = closedDays;
        			campaign.isLoadingData = false;
        			scope.$broadcast('campaign.chart.refresh', campaign);
		        });
        	});
        }

        function refreshCampaignStatistics(scope) {
        	getCampaignStatistics(null, function success(data) {
		        scope.phaseStatistics = data;
	        });
        }

    }
})();