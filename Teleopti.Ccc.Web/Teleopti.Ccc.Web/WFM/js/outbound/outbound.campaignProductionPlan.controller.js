(function () {
	'use strict';

	angular.module('wfm.outbound')
        .controller('OutboundProductionPlanCtrl', [
            '$scope', '$state', '$stateParams',  'outboundChartService', 'outboundTranslationService',
            productionPlanCtrl
        ]);

	function productionPlanCtrl($scope, $state, $stateParams, outboundService, outboundTranslationService) {

        // [ToDo Yanyi] Will refactor this futher later.

		$scope.backToList = function() {
			$state.go('outbound');
		};

	    $scope.isReady = false;
	    $scope.recordItems = [];

	    outboundTranslationService.translateWeekdays(function () {
	        var currentCampaignId = (angular.isDefined($stateParams.Id) && $stateParams.Id != "") ? $stateParams.Id : null;
	        this.weekDays = this.weekdays.map(function (d) { return d.weekdayName; });
	        var self = this;

	        outboundService.getCampaignVisualization(currentCampaignId, function success(data) {
	            var records = outboundService.zip({
	                dates: data.dates,
	                plans: data.plans
	            });
	            records.shift();
	            records.shift();
	            $scope.recordItems = records;

	            var dayOfWeekFirstDay = moment(records[0].dates).day();
	            $scope.gridOffset = calculateGridFirstRowOffset(dayOfWeekFirstDay, self.weekdays);

	            $scope.isReady = true;
	        });
	    }, $scope);

	    $scope.$watch(function () {	        
	        return $scope.recordItems.filter(function(i) {
	            return i.isSelected;
	        });
	    }, function (newValue) {
	        $scope.selectedRecordItems = newValue;
	    },true);

        function calculateGridFirstRowOffset(dayOfWeekFirstDay, weekdays) {
            for (var offset = 0; offset < 7; offset ++) {
                if (dayOfWeekFirstDay == weekdays.weekday) return offset;
            }
            return 0;
        }

	}



})();