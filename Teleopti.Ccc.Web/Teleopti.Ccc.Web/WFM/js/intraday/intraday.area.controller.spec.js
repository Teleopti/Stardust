'use strict';
describe('IntradayAreaCtrl', function () {
	var $httpBackend,
	$controller,
	$filter,
	scope,
	$translate,
	$interval,
	NoticeService;

	var skillAreas = [];
	var skills = [];
	var skillAreaInfo;
	var trafficAndPerformanceData;
	var staffingData;

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

				trafficAndPerformanceData = {
					LatestActualIntervalEnd: "0001-01-01T16:00:00",
					LatestActualIntervalStart:"0001-01-01T15:45:00",
					StatisticsDataSeries:{
						AbandonedRate:[null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
						AverageHandleTime:[null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
						AverageSpeedOfAnswer:[null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
						ForecastedAverageHandleTime:[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 199.8660714285, 201.3520471464],
						ForecastedCalls:[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.0112, 0.3224, 0.5169, 0.7337, 0.9672],
						OfferedCalls:[null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
						ServiceLevel:[null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null],
						Time:["0001-01-01T00:00:00", "0001-01-01T00:15:00", "0001-01-01T00:30:00", "0001-01-01T00:45:00"]
					},
					StatisticsSummary:{
						AbandonRate: 0.05594855305466238,
						AbandonedCalls: 87,
						AnsweredCalls: 1468,
						AnsweredCallsWithinSL: 1305,
						AverageHandleTime: 245.63987138263664,
						AverageSpeedOfAnswer: 22,
						ForecastedActualCallsDiff: 11.686895162492235,
						ForecastedActualHandleTimeDiff: 26.25497332067074,
						ForecastedAverageHandleTime: 194.55857058299338,
						ForecastedCalls: 1392.2850999999998,
						ForecastedHandleTime:270880.99889999995,
						HandleTime: 381970,
						OfferedCalls: 1555,
						ServiceLevel:	0.8392282958199357,
						SpeedOfAnswer:32296
					}
				};

				staffingData = {
					DataSeries: {
						ForecastedStaffing: [1,2,3],
						Time: ["2016-08-30T00:00:00", "2016-08-30T00:15:00"]
					}
				};
			});

			beforeEach(inject(function (_$httpBackend_, _$controller_, _$rootScope_, _$filter_, _$translate_,_$interval_, _NoticeService_) {
				$controller = _$controller_;
				$httpBackend = _$httpBackend_;
				$filter = _$filter_;
				$translate = _$translate_;
				scope = _$rootScope_.$new();
				$interval = _$interval_;
				NoticeService = _NoticeService_;

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

				$httpBackend.whenGET("../api/intraday/monitorskillareastatistics/fa9b5393-ef48-40d1-b7cc-09e797589f81")
				.respond(function () {
					return [200, trafficAndPerformanceData];
				});

				$httpBackend.whenGET("../api/intraday/monitorskillstatistics/5f15b334-22d1-4bc1-8e41-72359805d30f")
				.respond(function () {
					return [200, trafficAndPerformanceData];
				});

				$httpBackend.whenGET("../api/intraday/monitorskillstaffing/5f15b334-22d1-4bc1-8e41-72359805d30f")
				.respond(function () {
					return [200, staffingData];
				});

				$httpBackend.whenGET("../api/intraday/monitorskillareastaffing/fa9b5393-ef48-40d1-b7cc-09e797589f81")
				.respond(function () {
					return [200, staffingData];
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

			xit('should stop polling when page is about to destroy', function() {
				createController(false);
				$interval(function(){
					scope.$emit('$destroy');
				}, 60000);
				$interval.flush(60000);
				$httpBackend.verifyNoOutstandingRequest();
			});

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
				// $httpBackend.flush();

				expect(scope.selectedItem).toEqual(scope.skills[0]);
			});

			it('should monitor first skill area if there are any', function () {
				createController(false);

				scope.skillAreaSelected(scope.skillAreas[0]);
				// $httpBackend.flush();

				expect(scope.selectedItem).toEqual(scope.skillAreas[0]);
			});

			it('should have permission to modify skill area', function () {
				createController(false);

				expect(scope.HasPermissionToModifySkillArea).toEqual(true);
			});

			it('should poll data for skill when selecting that skill', function () {
				createController(false);

				scope.skillSelected(scope.skills[0]);
				$httpBackend.flush();

				expect(scope.viewObj.hasMonitorData).toEqual(true);
			});

			it('should poll data for skill area when selecting that area', function(){
				createController(false);

				scope.skillAreaSelected(scope.skillAreas[0]);
				$httpBackend.flush();

				expect(scope.viewObj.hasMonitorData).toEqual(true);
			});

			it('should only poll traffic skill data when traffic tab and skill is selected', function () {
				createController(false);
				scope.activeTab = 0;

				scope.skillSelected(scope.skills[0]);
				$httpBackend.flush();
				scope.pollActiveTabDataHelper();
				$httpBackend.flush();

				expect(scope.viewObj.hasMonitorData).toEqual(true);
			});

			it('should only poll performance skill data when performance tab and skill is selected', function (){
				createController(false);
				scope.activeTab = 1;

				scope.skillSelected(scope.skills[0]);
				$httpBackend.flush();
				scope.pollActiveTabDataHelper();
				$httpBackend.flush();

				expect(scope.viewObj.hasMonitorData).toEqual(true);
			});

			it('should only poll staffing skill data when staffing tab and skill is selected', function (){
				createController(false);
				scope.activeTab = 2;

				scope.skillSelected(scope.skills[0]);
				$httpBackend.flush();
				scope.pollActiveTabDataHelper();
				$httpBackend.flush();

				expect(scope.viewObj.hasMonitorData).toEqual(true);
			});

			it('should only poll traffic skill area data when traffic tab and skill area is selected', function () {
				createController(false);
				scope.activeTab = 0;

				scope.skillAreaSelected(scope.skillAreas[0]);
				$httpBackend.flush();
				scope.pollActiveTabDataHelper();
				$httpBackend.flush();

				expect(scope.viewObj.hasMonitorData).toEqual(true);
			});

			it('should only poll performance skill area data when performance tab and skill area is selected', function (){
				createController(false);
				scope.activeTab = 1;

				scope.skillAreaSelected(scope.skillAreas[0]);
				$httpBackend.flush();
				scope.pollActiveTabDataHelper();
				$httpBackend.flush();

				expect(scope.viewObj.hasMonitorData).toEqual(true);
			});

			it('should only poll staffing skill area data when staffing tab and skill area is selected', function (){
				createController(false);
				scope.activeTab = 2;

				scope.skillAreaSelected(scope.skillAreas[0]);
				$httpBackend.flush();
				scope.pollActiveTabDataHelper();
				$httpBackend.flush();

				expect(scope.viewObj.hasMonitorData).toEqual(true);
			});

			xit('should get latest actual interval when selecting performance tab', function () {
				createController(false);
				scope.activeTab = 1;

				scope.pollActiveTabDataHelper();
				$httpBackend.flush();
				// console.log('viewObj ',scope.viewObj.latestActualInterval);
				// console.log('latest ', scope.latestActualInterval);
				expect(scope.latestActualInterval).not.toBe(undefined);
			});

		});
