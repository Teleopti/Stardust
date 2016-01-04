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

	var mockForecastingService = {
		scenarios: {
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
		},
		skills: {
			query: function () {
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
		},
		status: {
			get: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({});
				return { $promise: queryDeferred.promise };
			}
		}
	}

	beforeEach(function() {
		module('wfm.forecasting');
		module('externalModules');
	});

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
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
		$controller('ForecastingStartCtrl', { $scope: scope, Forecasting: mockForecastingService, Toggle: mockToggleService });

		scope.$digest();

		expect(scope.scenarios[0].Name).toBe("Default");
		expect(scope.scenarios[0].Id).toBe("1");
		expect(scope.scenarios[1].Name).toBe("High");
		expect(scope.scenarios[1].Id).toBe("2");
	}));

	fit('Should list workloads', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, Forecasting: mockForecastingService, Toggle: mockToggleService });

		scope.$digest();

		expect(scope.workloads[0].Name).toBe("Channel sales - Joint marketing");
		expect(scope.workloads[0].Id).toBe("d80b16de-bc21-471f-98ed-9b5e015abf6c");
		expect(scope.workloads[0].ChartId).toBe("chartd80b16de-bc21-471f-98ed-9b5e015abf6c");
		expect(scope.workloads[0].Scenario.Name).toBe("Default");
	}));
});
