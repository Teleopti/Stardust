﻿'use strict';
describe('IntradayAreaCtrl', function () {
	var $httpBackend,
		$controller,
		$filter,
		scope,
		$translate;

	var skillAreas = [];
	var skills = [];
	var skillAreaInfo;
	var monitorData;


	beforeEach(module('wfm.intraday'));

	beforeEach(function () {

		skills = [
		{
			Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
			Name: "skill x"
		}];

		skillAreas = [
		{
			Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
			Name: "my skill area 1",
			Skills: skills
		},
		{
			Id: "836cebb6-cee8-41a1-bb62-729f4b3a63f4",
			Name: "my skill area 2",
			Skills: skills
		}];

		skillAreaInfo = {
			HasPermissionToModifySkillArea: true,
			SkillAreas: skillAreas
		};

		monitorData = {
			Summary: {
				ForecastedCalls: "100.0",
				ForecastedAverageHandleTime: "60.0",
				OfferedCalls: "50.0",
				AverageHandleTime: "30.0",
				ForecastedActualCallsDiff: "50.0",
				ForecastedActualHandleTimeDiff: "55.0"
			},
			DataSeries: {
				Time: [],
				ForecastedCalls: [],
				ForecastedAverageHandleTime: [],
				OfferedCalls: [],
				AverageHandleTime: []
			},
			LatestStatsTime: new Date(2016, 1, 1, 13, 0, 0, 0)
		};
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$rootScope_, _$filter_, _$translate_) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		$filter = _$filter_;
		$translate = _$translate_;
		scope = _$rootScope_.$new();

		$httpBackend.whenGET("../api/intraday/skillarea")
			.respond(function () {
				return [200, skillAreaInfo];
			});

		$httpBackend.whenGET("../api/intraday/skills")
			.respond(function () {
				return [200, skills];
			});

		$httpBackend.whenDELETE("../api/intraday/skillarea/836cebb6-cee8-41a1-bb62-729f4b3a63f4")
			.respond(200, {});

		$httpBackend.whenGET("../api/intraday/monitorskillarea/fa9b5393-ef48-40d1-b7cc-09e797589f81")
			.respond(function () {
				return [200, monitorData];
			});

		$httpBackend.whenGET("../api/intraday/monitorskill/5f15b334-22d1-4bc1-8e41-72359805d30f")
			.respond(function () {
				return [200, monitorData];
			});
	}));

	var createController = function (isNewlyCreatedSkillArea) {
		$controller('IntradayAreaCtrl', {
			$scope: scope,
			$translate: $translate
		});

		scope.onStateChanged(undefined, { name: 'intraday.area' }, { isNewSkillArea: isNewlyCreatedSkillArea });
		scope.$digest();
		$httpBackend.flush();
	};

	it('should display list of skill areas', function () {
		createController(false);

		expect(scope.skillAreas[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(scope.skillAreas[0].Name).toEqual("my skill area 1");
		expect(scope.skillAreas[0].Skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skillAreas[0].Skills[0].Name).toEqual("skill x");
	});

	it('should display list of skills', function () {
		createController(false);

		expect(scope.skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skills[0].Name).toEqual("skill x");
	});

	it('should delete selected skill area', function () {
		createController(false);
		scope.deleteSkillArea(scope.skillAreas[1]);

		expect(scope.skillAreas.length).toEqual(2);

		$httpBackend.flush();

		expect(scope.selectedItem).toEqual(null);
		expect(scope.skillAreas.length).toEqual(1);
	});

	it('should monitor first skill if no skill areas', function () {
		skillAreaInfo.SkillAreas = [];
		createController(false);

		scope.skillSelected(scope.skills[0]);
		$httpBackend.flush();

		expect(scope.selectedItem).toEqual(scope.skills[0]);
		expect(scope.latestStatsTime).toEqual($filter('date')(monitorData.LatestStatsTime, 'shortTime'));
		expect(scope.forecastedCalls).toEqual(monitorData.Summary.ForecastedCalls);
		expect(scope.forecastedAverageHandleTime).toEqual(monitorData.Summary.ForecastedAverageHandleTime);
		expect(scope.offeredCalls).toEqual(monitorData.Summary.OfferedCalls);
		expect(scope.averageHandleTime).toEqual(monitorData.Summary.AverageHandleTime);
		expect(scope.forecastActualCallsDifference).toEqual(monitorData.Summary.ForecastedActualCallsDiff);
		expect(scope.forecastActualAverageHandleTimeDifference).toEqual(monitorData.Summary.ForecastedActualHandleTimeDiff);
		expect(scope.timeSeries[0]).toEqual('x');
		expect(scope.forecastedCallsSeries[0]).toEqual('Forecasted_calls');
		expect(scope.actualCallsSeries[0]).toEqual('Actual_calls');
		expect(scope.forecastedAverageHandleTimeSeries[0]).toEqual('Forecasted_AHT');
		expect(scope.actualAverageHandleTimeSeries[0]).toEqual('AHT');
		expect(scope.HasMonitorData).toEqual(true);
	});

	it('should monitor first skill area if there are any', function () {
		createController(false);

		scope.skillAreaSelected(scope.skillAreas[0]);
		$httpBackend.flush();

		expect(scope.selectedItem).toEqual(scope.skillAreas[0]);
		expect(scope.latestStatsTime).toEqual($filter('date')(monitorData.LatestStatsTime, 'shortTime'));
		expect(scope.forecastedCalls).toEqual(monitorData.Summary.ForecastedCalls);
		expect(scope.forecastedAverageHandleTime).toEqual(monitorData.Summary.ForecastedAverageHandleTime);
		expect(scope.offeredCalls).toEqual(monitorData.Summary.OfferedCalls);
		expect(scope.averageHandleTime).toEqual(monitorData.Summary.AverageHandleTime);
		expect(scope.forecastActualCallsDifference).toEqual(monitorData.Summary.ForecastedActualCallsDiff);
		expect(scope.forecastActualAverageHandleTimeDifference).toEqual(monitorData.Summary.ForecastedActualHandleTimeDiff);
		expect(scope.timeSeries[0]).toEqual('x');
		expect(scope.forecastedCallsSeries[0]).toEqual('Forecasted_calls');
		expect(scope.actualCallsSeries[0]).toEqual('Actual_calls');
		expect(scope.forecastedAverageHandleTimeSeries[0]).toEqual('Forecasted_AHT');
		expect(scope.actualAverageHandleTimeSeries[0]).toEqual('AHT');
		expect(scope.HasMonitorData).toEqual(true);
	});

	it('should have permission to modify skill area', function () {
		createController(false);

		expect(scope.HasPermissionToModifySkillArea).toEqual(true);
	});

	it('should show friendly message if no data for skill area', function () {
		createController(false);
		scope.skillAreaSelected(scope.skillAreas[0]);
		monitorData.LatestStatsTime = null;
		$httpBackend.flush();

		expect(scope.HasMonitorData).toEqual(false);
		expect(scope.latestStatsTime).toEqual('--:--');
	});

	it('should switch active chart tab', function () {
		createController(false);
		scope.toggleOthers(1, 'staffing');

		expect(scope.showStaffing).toEqual(true);
		expect(scope.showIncoming).toEqual(false);
	});
});
