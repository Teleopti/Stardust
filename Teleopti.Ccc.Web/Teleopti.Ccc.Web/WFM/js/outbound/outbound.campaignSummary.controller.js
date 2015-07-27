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
        $scope.generateChart = function(campaign) {
	        outboundService.getCampaignVisualization(campaign.Id, function success(data) {
		        console.log(data);
	        });
        	//setTimeout(function () {
        	//	chart.transform('pie');
        	//}, 1000);
	        var chart = c3.generate({
	        	bindto: '#Chart_' + campaign.Id,
	        	data: {
	        		x: 'x',
	        		columns: [
					    ['x', '2015-07-20', '2015-07-21', '2015-07-22', '2015-07-23', '2015-07-24', '2015-07-25', '2015-07-26', '2015-07-27', '2015-07-28', '2015-07-29'],
					    ['backlog', 90, 80, 70, 59, 49, 39, 29, 19, 9, 0],
					    ['underSLA', 0, 0, 4, 0, 0, 0, 0, 0, 0, 0],
					    ['Overstaffing', 0, 0, 0, 5, 0, 0, 0, 0, 0, 0],
					    ['Scheduled', 10, 10, 6, 10, 10, 0, 0, 0, 0, 0],
					    ['Planned', 0, 0, 0, 0, 0, 10, 10, 10, 10, 9],
					    ['leftovers', 0, 0, 0, 0, 0, 0, 0, 0, 0, 3],
						//['leftovers2', 0, 0, 0, 0, 0, 0, 0, 0, 0, 3],
					    ['trends', 100, 100, 96, 105, 100, 100, 100, 100, 100, 100]
	        		],
	        		//names: {
	        		//	data1:'backlog'
	        		//},
	        		type: 'bar',
	        		types: {
	        			trends: 'line'
	        		},
	        		groups: [
					    ['Planned', 'Scheduled', 'Overstaffing', 'underSLA', 'backlog', 'leftovers', 'leftovers2']
	        		],
	        		colors: {
	        			Planned: '#66C2FF',
	        			Scheduled: '#C2E085',
	        			Overstaffing: '#f44336',
	        			underSLA: '#FFBD00',
	        			backlog: '#1F77B4',
	        			leftovers: '#9467BD',
	        			trends: '#FF7F0E'
	        			//another:'#BCBD22'
	        		},
	        		order: 'null'

	        	},
	        	zoom: {
	        		enabled: true
	        		//rescale: true
	        	},
	        	axis: {
	        		x: {
	        			type: 'timeseries',
	        			tick: {
	        				format: '%m-%d'
	        				//format: function (x) { return x.getFullYear(); }
	        				//values: ['2015-07-20', '2015-07-21', '2015-07-22', '2015-07-23', '2015-07-24', '2015-07-25', '2015-07-26', '2015-07-27', '2015-07-28', '2015-07-29']
	        			}
	        		},
	        		y: {
	        			tick: {
	        				format: function (d) { return d + " h"; }
	        			},
	        			label: {
	        				text: 'y label'
	        				//position: 'outer-middle'
	        			}
	        		}
	        	}
	        });
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