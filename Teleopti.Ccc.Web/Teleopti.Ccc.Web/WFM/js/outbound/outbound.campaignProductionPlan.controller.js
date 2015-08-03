(function () {
	'use strict';

	angular.module('wfm.outbound')
        .controller('OutboundProductionPlanCtrl', [
            '$scope', '$state', '$stateParams', '$q', '$translate', '$filter', 'outboundService', 'outboundChartService',
            productionPlanCtrl
        ]);

	function productionPlanCtrl($scope, $state, $stateParams, $q, $translate, $filter, outboundService, outboundChartService) {

        // [ToDo Yanyi] Will refactor this later.

		$scope.backToList = function() {
			$state.go('outbound');
		};

	    $scope.isReady = false;
	    $scope.recordItems = [];

	    var weekDays = outboundService.createEmptyWorkingPeriod().WeekDaySelections;
	    
	    var translations = [];
	    var i;
	    for (i = 0; i < weekDays.length; i++) {
	        translations.push($translate($filter('showWeekdays')(weekDays[i])));
	    }

	    var weekDayHeaders = [];

	    $q.all(translations).then(function (ts) {	       
	        for (i = 0; i < weekDays.length; i++) {
	            weekDayHeaders[i] = ts[i];
	        }
	        $scope.weekDays = weekDayHeaders;

	        var currentCampaignId = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? $stateParams.Id : null;

	        outboundChartService.getCampaignVisualization(currentCampaignId, function success(data) {
	            var records = outboundChartService.zip({
	                dates: data.dates,
	                plans: data.plans
	            });
	            records.shift();
	            records.shift();
	            $scope.recordItems = records;

	            $scope.isReady = true;
	        });


	    });

	    $scope.$watch(function () {
	        
	        return $scope.recordItems.filter(function(i) {
	            return i.isSelected;
	        });
	    }, function (newValue) {


	        $scope.selectedRecordItems = newValue;
	    },true);


	}



})();