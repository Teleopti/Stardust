'use strict';
describe('ForecastingAdvancedCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend;

	var mockForecastingService = function () {
		this.evaluate = function (data, successCb, errorCb) {
			successCb({
				Days: [
					{ date: "2012-03-05T00:00:00", vh: 1930, vb: 2085 }
				],
				WorkloadId: data.WorkloadId,
				Name: "test workload Name",
				ForecastMethodRecommended: { Id: 14, PeriodEvaluateOnStart: "2012-03-05T00:00:00", PeriodEvaluateOnEnd: "2013-03-04T00:00:00", PeriodUsedToEvaluateStart: "2010-03-05T00:00:00" }
			});
		};

		this.queueStatistics = function (data, successCb, errorCb) {
			successCb({
				QueueStatisticsDays: [
					{ date: "2010-03-09T00:00:00", vh: 2245, vh2: 2245 }
				],
				WorkloadId: data.WorkloadId
			});
		};
	}

	beforeEach(function () {
		module('wfm.forecasting');
		module('externalModules');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	it("should display evaluation result", inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('ForecastingAdvancedCtrl', { $scope: scope, forecastingService: new mockForecastingService(), $stateParams: { workloadId: 3, workloadName: "test workload Name" } });

		expect(scope.evaluationChartData).toBe(undefined);
		scope.$digest();
		scope.init();
		expect(scope.evaluationChartData.length).toBe(1);
		expect(scope.evaluationChartData[0].vh).toBe(1930);
	}));

	it("should display queue statistics", inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('ForecastingAdvancedCtrl', { $scope: scope, forecastingService: new mockForecastingService(), $stateParams: { workloadId: 3, workloadName: "test workload Name" } });

		expect(scope.queueStatisticsChartData).toBe(undefined);
		scope.$digest();
		scope.init();
		expect(scope.queueStatisticsChartData.length).toBe(1);
		expect(scope.queueStatisticsChartData[0].vh).toBe(2245);
	}));
});
