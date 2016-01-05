'use strict';
describe('ForecastingStartCtrl', function() {
	var $q,
		$rootScope,
		$httpBackend;

	var mockToggleService = {
		isFeatureEnabled: {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					IsEnabled: true
				});
				return { $promise: queryDeferred.promise };
			}
		},
	}

	var mockForecastingService = function() {
		this.scenarios = {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve(
				[
					{
						Id: "1",
						Name: "Default"
					}, {
						Id: "2",
						Name: "High"
					}
				]);
				return { $promise: queryDeferred.promise };
			}
		};
		this.skills = {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve(
				{
					Skills:
					[
						{
							Id: "3d5dd51a-8713-42e9-9f33-9b5e015ab71b",
							Name: "Channel sales",
							Workloads: [{ Id: "d80b16de-bc21-471f-98ed-9b5e015abf6c", Name: "Joint marketing", Accuracies: null }]
						}
					],
					IsPermittedToModifySkill: true
				});
				return { $promise: queryDeferred.promise };
			}
		};
		this.status = {
			get: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({});
				return { $promise: queryDeferred.promise };
			}
		};
		this.forecast = function(data, successCb, errorCb, finalCb) {
			return data;
		};
		this.result = function(data, successCb, errorCb) {
			successCb({
				Days: [
					{ date: "2016-02-01T00:00:00", vc: 332.2292728719883, vtc: 332.2292728719883, vtt: 186.3655753, vttt: 186.3655753 }
				],
				WorkloadId: data.WorkloadId
			});
		};
	}

	beforeEach(function() {
		module('wfm.forecasting');
		module('externalModules');
	});

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	it("forecasting period should default to next month", inject(function($controller) {
		var scope = $rootScope.$new();

		expect(scope.period).toBe(undefined);
		$controller('ForecastingStartCtrl', { $scope: scope, $state: {} });
		expect(scope.period.startDate.toString()).toBe(moment().utc().add(1, 'months').startOf('month').toDate().toString());
		expect(scope.period.endDate.toString()).toBe(moment().utc().add(2, 'months').startOf('month').toDate().toString());
	}));

	it('Should cancel forecasting modal', inject(function($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope });
		scope.cancelForecastingModal();
		expect(scope.modalForecastingLaunch).toBe(false);
	}));

	it('Should cancel modify modal', inject(function($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope });
		scope.cancelModifyModal();
		expect(scope.modalModifyLaunch).toBe(false);
	}));

	it('Should list scenarios', inject(function($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService(), Toggle: mockToggleService });

		scope.$digest();

		expect(scope.scenarios[0].Name).toBe("Default");
		expect(scope.scenarios[0].Id).toBe("1");
		expect(scope.scenarios[1].Name).toBe("High");
		expect(scope.scenarios[1].Id).toBe("2");
	}));

	it('Should list workloads', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService(), Toggle: mockToggleService });

		scope.$digest();

		expect(scope.workloads[0].Name).toBe("Channel sales - Joint marketing");
		expect(scope.workloads[0].Id).toBe("d80b16de-bc21-471f-98ed-9b5e015abf6c");
		expect(scope.workloads[0].ChartId).toBe("chartd80b16de-bc21-471f-98ed-9b5e015abf6c");
		expect(scope.workloads[0].Scenario.Name).toBe("Default");
	}));

	fit('Should get forecast result', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService(), Toggle: mockToggleService });
		
		scope.$digest();
		scope.workloads[0].Refresh = function (days) {
			expect(days[0].vc).toBe(332.2292728719883);
			expect(days[0].vtc).toBe(332.2292728719883);
			expect(days[0].vtt).toBe(186.3655753);
			expect(days[0].vttt).toBe(186.3655753);
		};
		scope.getForecastResult(scope.workloads[0]);
	}));
});
