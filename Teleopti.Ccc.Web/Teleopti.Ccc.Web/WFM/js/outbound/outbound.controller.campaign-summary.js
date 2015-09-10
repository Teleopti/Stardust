(function() {

	angular.module('wfm.outbound')
		.controller('OutboundSummaryCtrl', [
			'$scope',
			'$state',
			'$stateParams',
			'outboundService',
			'outboundChartService',
			'$filter',
			'outboundNotificationService',
			'Toggle',
			summaryCtrl
		]);

	function summaryCtrl($scope, $state, $stateParams, outboundService, outboundChartService, $filter, outboundNotificationService, Toggle) {
        $scope.isLoadFinished = false;
        $scope.listCampaignFinished = false;

        init();

        $scope.ganttOptions = {
        	headers: ['month', 'day'],
        	fromDate: '2015-9-1',
        	toDate: '2015-10-31'
        }


        $scope.ganttData = [
		 {
		 	name: "Create concept",
		 	from: '2015-9-9',
		 	to: '2015-10-15',
		 	tasks: [
			  {
			  	name: "Create concept",
			  	content: "<i class=\"fa fa-cog\" ng-click=\"scope.handleTaskIconClick(task.model)\"></i> {{task.model.name}}",
			  	color: "#09F",
			  	from: "2015-9-9",
			  	to: "2015-10-15",
			  	id: "58916e1c-1c2e-3d39-f5d4-8246b604fed41"
			  }
		 	],
		 	id: "81aea986-5ece-977d-942b-c6dc447baaed2"
		 }, {
		 	name: "Create concept234",
		 	from: '2015-9-10',
		 	to: '2015-10-16',
		 	tasks: [
			  {
			  	name: "Create concept2",
			  	content: "<i class=\"fa fa-cog\" ng-click=\"scope.handleTaskIconClick(task.model)\"></i> {{task.model.name}}",
			  	color: "#F1C232",
			  	from: "2015-9-10",
			  	to: "2015-10-1",
			  	id: "58916e1c-1c2e-3d39-f5d4-8246b604fed43"
			  },
			   {
			   	name: "Create concept2",
			   	content: "<i class=\"fa fa-cog\" ng-click=\"scope.handleTaskIconClick(task.model)\"></i> {{task.model.name}}",
			   	color: "#F1C232",
			   	from: "2015-10-10",
			   	to: "2015-10-16",
			   	id: "58916e1c-1c2e-3d39-f5d4-8246b604fed431"
			   }
		 	],
		 	id: "81aea986-5ece-977d-942b-c6dc4472baaed4"
		 }
        ];

        function init() {

        	outboundService.load(function handleSuccess(isLoad) {
        		outboundService.getCampaignStatistics(null, function success(data) {
        			$scope.phaseStatistics = data;
        		});
        		$scope.$watch('activePhaseCode', function (newValue, oldValue) {
        			clearCampaignList();
        			outboundService.listFilteredCampaigns(newValue, function success(data) {
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
				outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
			});
		    $scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
			campaign.manualPlanInput = null;
    	}

    	$scope.removeBacklog = function (campaign) {
    		var dates = [];
    		campaign.selectedDates.forEach(function (date, index) {
    			dates[index] = { Date: date };
    		});
    		var removeActualBacklog = {
    			CampaignId: campaign.Id,
    			Dates: dates
    		};

    		campaign.isLoadingData = true;
    		outboundChartService.removeActualBacklog(removeActualBacklog, function () {
    			refreshGraphData(campaign, $scope);
    			refreshCampaignStatistics($scope);
    			outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
    		});
    		$scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
    		campaign.backlogInput = null;
    	}

    	$scope.addBacklog = function (campaign) {
    		campaign.backlog.CampaignId = campaign.Id;
    		campaign.backlog.ActualBacklog = [];
    		campaign.selectedDates.forEach(function (date, index) {
    			campaign.backlog.ActualBacklog[index] = {
    				Date: { Date: date },
    				Time: campaign.backlogInput
    			}
    		});
    		campaign.isLoadingData = true;
    		outboundChartService.updateBacklog(campaign.backlog, function () {
    			refreshGraphData(campaign, $scope);
    			refreshCampaignStatistics($scope);
    			outboundNotificationService.notifyCampaignUpdateSuccess(campaign);
    		});
    		$scope.$broadcast('campaign.chart.clear.selection', { Id: campaign.Id });
    		campaign.backlogInput = null;
    	}

		$scope.addManualPlan = function (campaign) {
    		campaign.manualPlan.CampaignId = campaign.Id;
    		campaign.manualPlan.ManualProductionPlan = [];
			campaign.selectedDates.filter(function (d) {			
    			return campaign.selectedDatesClosed.indexOf(d) < 0;
		    }).forEach(function (date, index) {
    			campaign.manualPlan.ManualProductionPlan[index] = {
    				Date: {Date: date},
    				Time: campaign.manualPlanInput
    			}
    		});
    		campaign.isLoadingData = true;
    		outboundChartService.updateManualPlan(campaign.manualPlan, function () {
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

		$scope.showDateSelectionHint = function(campaign) {
			return !campaign.selectedDates || campaign.selectedDates.length == 0 && (campaign.manualPlanswitch || campaign.backlogSwitch);
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
        	outboundService.getCampaignStatistics(null, function success(data) {
        		scope.phaseStatistics = data;
        	});
        }

    }
})();