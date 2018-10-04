"use strict";
describe("ForecastingSkillCreateController", function () {
	var $q, $rootScope, $httpBackend;

	var mockSkillService = {
		activities: {
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([
					{
						Id: "ActivityId-1",
						Name: "Chat"
					},
					{
						Id: "ActivityId-2",
						Name: "E-mail"
					}
				]);
				return { $promise: queryDeferred.promise };
			}
		},
		timezones: {
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					DefaultTimezone: "W. Europe Standard Time",
					Timezones: [
						{
							Id: "W. Europe Standard Time",
							Name:
								"(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
						}
					]
				});
				return { $promise: queryDeferred.promise };
			}
		},
		queues: {
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([
					{
						Id: "QueueId-1",
						Name: "Queue 1",
						LogObjectName: "ACD",
						Description: "Queue 1"
					}
				]);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	var mockNoticeService = {};

	beforeEach(function () {
		module("wfm.forecasting");
		module("externalModules");
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	it("should have correct default values", inject(function ($controller) {
		var scope = $rootScope.$new();
		var vm = $controller("ForecastingSkillCreateController", {
			$scope: scope,
			NoticeService: mockNoticeService,
			SkillService: mockSkillService
		});
		scope.$digest();

		expect(vm.model.serviceLevelPercent).toBe(80);
		expect(vm.model.serviceLevelSeconds).toBe(20);
		expect(vm.model.shrinkage).toBe(0);
		expect(vm.model.selectedTimezone.Id).toBe("W. Europe Standard Time");
		expect(vm.model.workingHours[0].WeekDaySelections.length).toBe(7);
		expect(vm.model.workingHours[0].StartTime.getTime()).toBe(
			new Date(2000, 1, 1, 0, 0, 0, 0).getTime()
		);
		expect(vm.model.workingHours[0].EndTime.getTime()).toBe(
			new Date(2000, 1, 2, 0, 0, 0, 0).getTime()
		);
	}));

	it("should display queues", inject(function ($controller) {
		var scope = $rootScope.$new();
		var vm = $controller("ForecastingSkillCreateController", {
			$scope: scope,
			NoticeService: mockNoticeService,
			SkillService: mockSkillService
		});
		scope.$digest();

		expect(vm.gridOptions.data.length).toBe(1);
		expect(vm.gridOptions.data[0].Id).toBe("QueueId-1");
		expect(vm.gridOptions.data[0].Name).toBe("Queue 1");
		expect(vm.gridOptions.data[0].LogObjectName).toBe("ACD");
		expect(vm.gridOptions.data[0].Description).toBe("Queue 1");
	}));
});