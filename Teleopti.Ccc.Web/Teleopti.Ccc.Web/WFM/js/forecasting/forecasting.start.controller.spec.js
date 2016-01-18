'use strict';
describe('ForecastingStartCtrl', function() {
	var $q,
		$rootScope,
		$httpBackend;

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
							Workloads: [{ Id: "d80b16de-bc21-471f-98ed-9b5e015abf6c", Name: "Channel sales - Joint marketing", Accuracies: null }]
						},
						{
							Id: "3d5dd51a-8713-42e9-9f33-9b5e015ab71c",
							Workloads: [{ Id: "d80b16de-bc21-471f-98ed-9b5e015abf6d", Name: "Channel sales2 - Joint marketing2", Accuracies: null }]
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
		this.result = function(data, successCb, errorCb) {
			successCb({
				Days: [
					{ date: "2016-02-01T00:00:00", vc: 332.2292728719883, vtc: 332.2292728719883, vtt: 186.3655753, vttt: 186.3655753 }
				],
				WorkloadId: data.WorkloadId
			});
		};
	};

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
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService() });

		scope.$digest();

		expect(scope.scenarios[0].Name).toBe("Default");
		expect(scope.scenarios[0].Id).toBe("1");
		expect(scope.scenarios[1].Name).toBe("High");
		expect(scope.scenarios[1].Id).toBe("2");
	}));

	it('Should list workloads', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService() });

		scope.$digest();

		expect(scope.workloads[0].Name).toBe("Channel sales - Joint marketing");
		expect(scope.workloads[0].Id).toBe("d80b16de-bc21-471f-98ed-9b5e015abf6c");
		expect(scope.workloads[0].ChartId).toBe("chartd80b16de-bc21-471f-98ed-9b5e015abf6c");
		expect(scope.workloads[0].Scenario.Name).toBe("Default");
	}));

	it('Should display forecast result', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService() });
		
		scope.$digest();
		scope.workloads[0].Refresh = function (days) {
			expect(days[0].vc).toBe(332.2292728719883);
			expect(days[0].vtc).toBe(332.2292728719883);
			expect(days[0].vtt).toBe(186.3655753);
			expect(days[0].vttt).toBe(186.3655753);
		};
		scope.getForecastResult(scope.workloads[0]);
	}));

	it('open modify dialog', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService() });

		scope.$digest();
		scope.workloads[0].selectedDays = function () {
			return [
				{
					id: "vc",
					index: 17,
					value: 301,
					x: new Date("Thu Feb 18 2016 01:00:00 GMT+0100 (W. Europe Standard Time)")
				},
				{
					id: "vtc",
					index: 17,
					name: "Total calls < ",
					value: 382.5876745833356,
					x: new Date("Thu Feb 18 2016 01:00:00 GMT+0100 (W. Europe Standard Time)")
				}
			];
		};

		scope.displayModifyModal(scope.workloads[0]);

		expect(scope.modalModifyLaunch).toBe(true);
		expect(scope.sumOfCallsForSelectedDays).toBe('301.0');
		expect(scope.modalModifyInfo.selectedScenario.Id).toBe('1');
		expect(scope.modalModifyInfo.selectedDayCount).toBe(1);
		expect(scope.modalModifyInfo.selectedDaySpan).toBe('2/18/16');
		expect(scope.sumOfCallsForSelectedDaysWithCampaign).toBe('301.0');
	}));

	it('open forecast dialog for all', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService() });

		scope.$digest();

		scope.displayForecastingModal();

		expect(scope.modalForecastingInfo.forecastForAll).toBe(true);
		expect(scope.modalForecastingInfo.forecastForOneWorkload).toBe(false);
	}));

	it('open forecast dialog for one workload', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService() });

		scope.$digest();

		scope.displayForecastingModal(scope.workloads[0]);

		expect(scope.modalForecastingInfo.forecastForAll).toBe(false);
		expect(scope.modalForecastingInfo.forecastForOneWorkload).toBe(true);
		expect(scope.modalForecastingInfo.selectedWorkload).toBe(scope.workloads[0]);
		expect(scope.modalForecastingInfo.selectedScenario).toBe(scope.workloads[0].Scenario);
	}));

	it('change campaign percentage', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService() });

		scope.$digest();
		scope.workloads[0].selectedDays = function () {
			return [
				{
					id: "vc",
					index: 17,
					value: 301,
					x: new Date("Thu Feb 18 2016 01:00:00 GMT+0100 (W. Europe Standard Time)")
				},
				{
					id: "vtc",
					index: 17,
					name: "Total calls < ",
					value: 382.5876745833356,
					x: new Date("Thu Feb 18 2016 01:00:00 GMT+0100 (W. Europe Standard Time)")
				}
			];
		};
		scope.displayModifyModal(scope.workloads[0]);

		expect(scope.sumOfCallsForSelectedDaysWithCampaign).toBe('301.0');

		scope.modalModifyInfo.campaignPercentage = 10;
		scope.campaignPercentageChanged({
			campaignPercentageInput: {$error: {} }
		});
		expect(scope.sumOfCallsForSelectedDaysWithCampaign).toBe('331.1');
	}));


	it('newly created workload should be on the top', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService(), $stateParams: { workloadId: "d80b16de-bc21-471f-98ed-9b5e015abf6d" } });

		scope.$digest();

		expect(scope.workloads[0].Name).toBe("Channel sales2 - Joint marketing2");
		expect(scope.workloads[0].Id).toBe("d80b16de-bc21-471f-98ed-9b5e015abf6d");
		expect(scope.workloads[0].ChartId).toBe("chartd80b16de-bc21-471f-98ed-9b5e015abf6d");
		expect(scope.workloads[0].Scenario.Name).toBe("Default");
	}));

	it('should limit forecast period to be less one year', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ForecastingStartCtrl', { $scope: scope, forecastingService: new mockForecastingService() });
		var startDate = moment().utc().add(1, 'months').startOf('month').toDate();
		var endDate = moment().utc().add(13, 'months').startOf('month').toDate();
		scope.period = { startDate: startDate, endDate: endDate };
		expect(scope.moreThanOneYear()).toBe(true);

		endDate = moment().utc().add(13, 'months').startOf('month').add(-1, 'days').toDate();
		scope.period = { startDate: startDate, endDate: endDate };
		expect(scope.moreThanOneYear()).toBe(false);
	}));
});
