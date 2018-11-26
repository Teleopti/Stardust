'use strict';

describe('IntradayAreaController', function() {
	var defaultToggles = {
		WFM_Remember_My_Selection_In_Intraday_47254: true,
		togglesLoaded: {
			then: function(cb) {
				cb();
			}
		}
	};
	var $httpBackend,
		$controller,
		$filter,
		scope,
		$translate,
		$interval,
		NoticeService,
		currentUserInfo = new FakeCurrentUserInfo();

	var vm;
	var skillGroups = [];
	var skills = [];
	var skillRetError;
	var skillsWithFirstUnsupported = [];
	var skillGroupInfo;
	var trafficAndPerformanceData;
	var trafficAndPerformanceDataTomorrow;
	var performanceData;
	var performanceDataTomorrow;
	var staffingData;
	var staffingDataTomorrow;
	var emptyStaffingData;
	var timeData;
	var isUnsupportedSkillTest = false;

	beforeEach(function() {
		module('wfm.intraday');
		module(function($provide) {
			$provide.service('Toggle', function() {
				return defaultToggles;
			});
		});
		module(function($provide) {
			$provide.service('CurrentUserInfo', function() {
				return currentUserInfo;
			});
		});
		window.localStorage.clear();
	});

	beforeEach(function() {
		skills = [
			{
				Id: '5f15b334-22d1-4bc1-8e41-72359805d30f',
				Name: 'skill x',
				DoDisplayData: true,
				ShowAbandonRate: true,
				ShowReforecastedAgents: true
			},
			{
				Id: '502632DC-7A0C-434D-8A75-3153D5160787',
				Name: 'skill y',
				DoDisplayData: true,
				ShowAbandonRate: false,
				ShowReforecastedAgents: false
			},
			{
				Id: '6152d498-9168-4070-9004-a58cefab5017',
				Name: 'Skill BackOffice',
				Activity: 'activity3',
				SkillType: 'SkillTypeBackoffice',
				DoDisplayData: true,
				ShowAbandonRate: false,
				ShowReforecastedAgents: false
			}
		];

		skillRetError = {
			Id: 'returnmockerror',
			Name: 'errorskill',
			DoDisplayData: true,
			ShowAbandonRate: true,
			ShowReforecastedAgents: true
		};

		skillsWithFirstUnsupported = [
			{ Id: '63b0ac3d-06b5-42d7-bb0a-d7b2fd272196', Name: 'Unsupported skill1', DoDisplayData: false },
			{ Id: '22b0ac3d-06b5-42d7-bb0a-d7b2fd272196', Name: 'Supported skill1', DoDisplayData: true }
		];

		skillGroups = [
			{ Id: 'fa9b5393-ef48-40d1-b7cc-09e797589f81', Name: 'my skill area 1', Skills: skills },
			{ Id: '836cebb6-cee8-41a1-bb62-729f4b3a63f4', Name: 'my skill area 2', Skills: skills },
			{
				Id: '3f43f39b-f01b-47e7-b59f-35fc09fd5e41',
				Name: 'my unsupported skill area 1',
				Skills: skillsWithFirstUnsupported,
				UnsupportedSkills: skillsWithFirstUnsupported
			}
		];

		skillGroupInfo = { HasPermissionToModifySkillArea: true, SkillAreas: skillGroups };

		trafficAndPerformanceData = {
			LatestActualIntervalEnd: '0001-01-01T16:00:00',
			LatestActualIntervalStart: '0001-01-01T15:45:00',
			IncomingTrafficHasData: true,
			DataSeries: {
				AbandonedRate: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageHandleTime: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					170.8660714285,
					170.9112252
				],
				AverageSpeedOfAnswer: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ForecastedAverageHandleTime: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					199.8660714285,
					201.3520471464
				],
				ForecastedCalls: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0.0112,
					0.3224,
					0.5169,
					0.7337,
					0.9672
				],
				CalculatedCalls: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ServiceLevel: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				Time: ['0001-01-01T00:00:00', '0001-01-01T00:15:00', '0001-01-01T00:30:00', '0001-01-01T00:45:00']
			},
			Summary: {
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
				ForecastedHandleTime: 270880.99889999995,
				HandleTime: 381970,
				CalculatedCalls: 1555,
				ServiceLevel: 0.8392282958199357,
				SpeedOfAnswer: 32296
			}
		};
		trafficAndPerformanceDataTomorrow = {
			LatestActualIntervalEnd: '0001-01-02T16:00:00',
			LatestActualIntervalStart: '0001-01-02T15:45:00',
			IncomingTrafficHasData: true,
			DataSeries: {
				AbandonedRate: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageHandleTime: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageSpeedOfAnswer: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ForecastedAverageHandleTime: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					199.8660714285,
					201.3520471464
				],
				ForecastedCalls: [
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0.0112,
					0.3224,
					0.5169,
					0.7337,
					0.9672
				],
				CalculatedCalls: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ServiceLevel: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				Time: ['0001-01-02T00:00:00', '0001-01-02T00:15:00', '0001-01-02T00:30:00', '0001-01-02T00:45:00']
			},
			Summary: {
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
				ForecastedHandleTime: 270880.99889999995,
				HandleTime: 381970,
				CalculatedCalls: 1337,
				ServiceLevel: 0.9,
				SpeedOfAnswer: 32296
			}
		};

		performanceData = {
			LatestActualIntervalEnd: '0001-01-01T16:00:00',
			LatestActualIntervalStart: '0001-01-01T15:45:00',
			PerformanceHasData: true,
			DataSeries: {
				AbandonedRate: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageSpeedOfAnswer: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				EstimatedServiceLevels: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ServiceLevel: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				Time: ['0001-01-01T00:00:00', '0001-01-01T00:15:00', '0001-01-01T00:30:00', '0001-01-01T00:45:00']
			},
			Summary: {
				AbandonRate: 0.05594855305466238,
				AverageSpeedOfAnswer: 22,
				ServiceLevel: 0.8392282958199357,
				EstimatedServiceLevel: 0.8735783757893
			}
		};

		performanceDataTomorrow = {
			LatestActualIntervalEnd: '0001-01-01T16:00:00',
			LatestActualIntervalStart: '0001-01-01T15:45:00',
			PerformanceHasData: true,
			DataSeries: {
				AbandonedRate: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				AverageSpeedOfAnswer: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				EstimatedServiceLevels: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				ServiceLevel: [
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				],
				Time: ['0001-01-01T00:00:00', '0001-01-01T00:15:00', '0001-01-01T00:30:00', '0001-01-01T00:45:00']
			},
			Summary: {
				AbandonRate: 0.05594855305466238,
				AverageSpeedOfAnswer: 22,
				ServiceLevel: 0.9,
				EstimatedServiceLevel: 0.8735783757893
			}
		};

		timeData = { latestIntervalTime: { StartTime: {}, EndTime: {} } };
		staffingData = {
			StaffingHasData: true,
			DataSeries: {
				ForecastedStaffing: [1, 2, 3],
				UpdatedForecastedStaffing: [2, 3, 4],
				ActualStaffing: [2, 3, 4],
				Time: ['2016-08-30T00:00:00', '2016-08-30T00:15:00'],
				ScheduledStaffing: [1, 2, 3]
			}
		};

		staffingDataTomorrow = {
			StaffingHasData: true,
			DataSeries: {
				ForecastedStaffing: [3, 2, 1],
				UpdatedForecastedStaffing: [2, 3, 4],
				ActualStaffing: [2, 3, 4],
				Time: ['2016-08-30T00:00:00', '2016-08-30T00:15:00'],
				ScheduledStaffing: [1, 2, 3]
			}
		};

		emptyStaffingData = { DataSeries: null };
	});

	function initBackend() {
		skillGroups.forEach(function(item) {
			$httpBackend.whenGET('../api/intraday/monitorskillperformance/returnmockerror/0').respond(function() {
				return [403, performanceData];
			});

			$httpBackend.whenGET('../api/intraday/lateststatisticstimeforskill/returnmockerror').respond(function() {
				return [200, timeData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareaperformance/' + item.Id + '/0').respond(function() {
				return [200, performanceData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareaperformance/' + item.Id + '/1').respond(function() {
				return [200, performanceDataTomorrow];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareastatistics/' + item.Id + '/0').respond(function() {
				return [200, trafficAndPerformanceData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareastatistics/' + item.Id + '/1').respond(function() {
				return [200, trafficAndPerformanceDataTomorrow];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id).respond(function() {
				return [200, staffingData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id + '/0').respond(function() {
				return [200, staffingData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id + '/1').respond(function() {
				return [200, staffingDataTomorrow];
			});
			$httpBackend
				.whenGET(
					'../api/intraday/monitorskillareastatisticsbydate?dateUtc=0001-01-01T14:45:00.000Z&id=' + item.Id
				)
				.respond(function() {
					return [200, trafficAndPerformanceData];
				});

			$httpBackend
				.whenGET(
					'../api/intraday/monitorskillareastatisticsbydate?dateUtc=0001-01-02T14:45:00.000Z&id=' + item.Id
				)
				.respond(function() {
					return [200, trafficAndPerformanceDataTomorrow];
				});
			$httpBackend
				.whenGET(
					'../api/intraday/monitorskillareastaffing?DateTime=0001-01-01T14:45:00.000Z&SkillAreaId=' +
						item.Id +
						'&UseShrinkage=false'
				)
				.respond(function() {
					return [200, staffingData];
				});

			$httpBackend
				.whenGET(
					'../api/intraday/monitorskillareastaffing?DateTime=0001-01-02T14:45:00.000Z&SkillAreaId=' +
						item.Id +
						'&UseShrinkage=false'
				)
				.respond(function() {
					return [200, staffingDataTomorrow];
				});
			$httpBackend.whenGET('../api/intraday/lateststatisticstimeforskillarea/' + item.Id).respond(function() {
				return [200, timeData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id).respond(function() {
				return [200, emptyStaffingData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareastaffing/' + item.Id).respond(function() {
				return [200, staffingData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareaperformance/' + item.Id).respond(function() {
				return [200, performanceData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillareastatistics/' + item.Id).respond(function() {
				return [200, trafficAndPerformanceData];
			});
			$httpBackend.whenDELETE('../api/skillarea/delete/' + item.Id).respond(200, {});
		});
		skills.forEach(function(item) {
			$httpBackend.whenGET('../api/intraday/monitorskillstatistics/' + item.Id + '/0').respond(function() {
				return [200, trafficAndPerformanceData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillstatistics/' + item.Id + '/1').respond(function() {
				return [200, trafficAndPerformanceDataTomorrow];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillstaffing/' + item.Id + '/0').respond(function() {
				return [200, staffingData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillstaffing/' + item.Id + '/1').respond(function() {
				return [200, staffingDataTomorrow];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillperformance/' + item.Id + '/0').respond(function() {
				return [200, performanceData];
			});

			$httpBackend.whenGET('../api/intraday/monitorskillperformance/' + item.Id + '/1').respond(function() {
				return [200, performanceDataTomorrow];
			});
			$httpBackend.whenGET('../api/intraday/lateststatisticstimeforskill/' + item.Id).respond(function() {
				return [200, timeData];
			});
			$httpBackend.whenGET('../api/intraday/lateststatisticstimeforskill/' + item.Id).respond(function() {
				return [200, timeData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillstaffing/' + item.Id).respond(function() {
				return [200, staffingData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillperformance/' + item.Id).respond(function() {
				return [200, performanceData];
			});
			$httpBackend.whenGET('../api/intraday/monitorskillstatistics/' + item.Id).respond(function() {
				return [200, trafficAndPerformanceData];
			});
		});
	}

	beforeEach(inject(function(
		_$httpBackend_,
		_$controller_,
		_$rootScope_,
		_$filter_,
		_$translate_,
		_$interval_,
		_NoticeService_
	) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		$filter = _$filter_;
		$translate = _$translate_;
		scope = _$rootScope_.$new();
		$interval = _$interval_;
		NoticeService = _NoticeService_;

		$httpBackend.whenGET('../api/skillgroup/skillgroups').respond(function() {
			return [200, skillGroupInfo];
		});

		$httpBackend.whenGET('../api/skillgroup/skills').respond(function() {
			if (isUnsupportedSkillTest) {
				return [200, skillsWithFirstUnsupported];
			} else {
				return [200, skills];
			}
		});

		$httpBackend.whenGET('../ToggleHandler/AllToggles').respond(function() {
			return [200];
		});
		initBackend();
	}));

	function FakeCurrentUserInfo() {
		this.CurrentUserInfo = function() {
			return { DefaultTimeZone: 'America/Denver' };
		};
	}

	var createController = function(isNewlyCreatedSkillArea, toggleObj) {
		vm = $controller('IntradayAreaController', { $scope: scope, $translate: $translate });
		vm.toggles = Object.assign(defaultToggles, toggleObj);
		vm.onStateChanged(undefined, { name: 'intraday.legacy' }, { isNewSkillArea: isNewlyCreatedSkillArea });
		scope.$digest();
		$httpBackend.flush();
	};

	it('should stop polling when page is about to destroy', function() {
		createController(false);
		$interval(function() {
			scope.$emit('$destroy');
		}, 5000);
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should display list of skill areas', function() {
		createController(false);

		expect(vm.skillGroups[0].Id).toEqual('fa9b5393-ef48-40d1-b7cc-09e797589f81');
		expect(vm.skillGroups[0].Name).toEqual('my skill area 1');
		expect(vm.skillGroups[0].Skills[0].Id).toEqual('5f15b334-22d1-4bc1-8e41-72359805d30f');
		expect(vm.skillGroups[0].Skills[0].Name).toEqual('skill x');
	});

	it('should display list of skills', function() {
		createController(false);

		expect(vm.skills[0].Id).toEqual('5f15b334-22d1-4bc1-8e41-72359805d30f');
		expect(vm.skills[0].Name).toEqual('skill x');
	});

	it('should monitor first skill area if there are any', function() {
		createController(false);

		vm.clickedSkillGroupInPicker(vm.skillGroups[0]);

		expect(vm.moduleState.selectedSkillGroup).toEqual(vm.skillGroups[0]);
	});

	it('should have permission to modify skill area', function() {
		createController(false);

		expect(vm.HasPermissionToModifySkillGroup).toEqual(true);
	});

	it('should poll data for skill when selecting that skill', function() {
		createController(false);
		vm.moduleState.activeTab = 0;

		vm.clickedSkillInPicker(vm.skills[0]);
		$httpBackend.flush();
		expect(vm.viewObj.hasMonitorData).toEqual(true);
	});

	it('should poll data for skill area when selecting that area', function() {
		createController(false);
		vm.moduleState.activeTab = 0;
		vm.clickedSkillGroupInPicker(vm.skillGroups[0]);
		vm.skillGroups[0];
		$httpBackend.flush();

		expect(vm.viewObj.hasMonitorData).toEqual(true);
	});

	it('should only poll traffic skill data when traffic tab and skill is selected', function() {
		createController(false);
		vm.moduleState.activeTab = 0;

		vm.clickedSkillInPicker(vm.skills[0]);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedCallsObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll performance skill data when performance tab and skill is selected', function() {
		createController(false);
		vm.moduleState.activeTab = 1;

		vm.clickedSkillInPicker(vm.skills[0]);
		$httpBackend.flush();

		expect(vm.viewObj.serviceLevelObj.series.length).toBeGreaterThan(5);
	});

	it('should show estimated service level in performance tab and skill is selected', function() {
		createController(false);
		vm.moduleState.activeTab = 1;

		vm.clickedSkillInPicker(vm.skills[0]);
		$httpBackend.flush();

		expect(vm.viewObj.estimatedServiceLevelObj.series.length).toBeGreaterThan(0);
		expect(vm.viewObj.summary.summaryEstimatedServiceLevel).toBeGreaterThan(0);
	});

	it('should only poll staffing skill data when staffing tab and skill is selected', function() {
		createController(false);
		vm.moduleState.activeTab = 2;

		vm.clickedSkillInPicker(vm.skills[0]);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedStaffing.series.length).toBeGreaterThan(3);
	});

	it('should only poll traffic skill area data when traffic tab and skill area is selected', function() {
		createController(false);
		vm.moduleState.activeTab = 0;

		vm.clickedSkillGroupInPicker(vm.skillGroups[0]);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedCallsObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll performance skill area data when performance tab and skill area is selected', function() {
		createController(false);
		vm.moduleState.activeTab = 1;

		vm.clickedSkillGroupInPicker(vm.skillGroups[0]);
		$httpBackend.flush();

		expect(vm.viewObj.serviceLevelObj.series.length).toBeGreaterThan(5);
	});

	it('should only poll staffing skill area data when staffing tab and skill area is selected', function() {
		createController(false);
		vm.moduleState.activeTab = 2;

		vm.clickedSkillGroupInPicker(vm.skillGroups[0]);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedStaffing.series.length).toBeGreaterThan(3);
	});

	it('should show optimal staffing', function() {
		vm.moduleState.activeTab = 2;

		vm.clickedSkillGroupInPicker(vm.skillGroups[0]);
		// $httpBackend.flush();

		expect(vm.viewObj.actualStaffingSeries.length).toBeGreaterThan(3);
	});

	it('should show scheduled staffing', function() {
		vm.moduleState.activeTab = 2;

		vm.clickedSkillGroupInPicker(vm.skillGroups[0]);
		// $httpBackend.flush();

		expect(vm.viewObj.scheduledStaffing.length).toBeGreaterThan(3);
	});

	it('should show ESL in performance', function() {
		createController(false);
		vm.moduleState.activeTab = 1;

		vm.clickedSkillGroupInPicker(vm.skillGroups[0]);
		$httpBackend.flush();

		expect(vm.viewObj.estimatedServiceLevelObj.series.length).toBeGreaterThan(0);
		expect(vm.viewObj.summary.summaryEstimatedServiceLevel).toBeGreaterThan(0);
	});

	it('should get traffic data corresponding to chosenOffset', function() {
		skillGroupInfo.SkillGroups = [];
		isUnsupportedSkillTest = false;

		createController(false);
		vm.moduleState.activeTab = 0;
		vm.clickedSkillInPicker(skills[0]);

		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.moduleState.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');
	});

	it('should get performance data corresponding to chosenOffset', function() {
		skillGroupInfo.SkillGroups = [];
		isUnsupportedSkillTest = false;

		createController(false);
		vm.moduleState.activeTab = 1;
		vm.clickedSkillInPicker(skills[0]);
		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.moduleState.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryServiceLevel).toEqual('90.0');
	});

	it('should get staffing data corresponding to chosenOffset', function() {
		skillGroupInfo.SkillGroups = [];
		isUnsupportedSkillTest = false;

		createController(false);
		vm.moduleState.activeTab = 2;
		vm.clickedSkillInPicker(skills[0]);
		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.moduleState.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.forecastedStaffing.series[1]).toEqual(3);
	});

	it('should get traffic data for skillgroup corresponding to chosenOffset', function() {
		createController(false);
		vm.moduleState.activeTab = 0;
		vm.clickedSkillGroupInPicker(skillGroups[0]);
		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.moduleState.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');
	});

	it('should get staffing data for skillgroup corresponding to chosenOffset', function() {
		createController(false);
		vm.moduleState.activeTab = 2;
		vm.clickedSkillGroupInPicker(skillGroups[0]);
		vm.changeChosenOffset(1);
		$httpBackend.flush();

		expect(vm.moduleState.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.forecastedStaffing.series[1]).toEqual(3);
		jasmine.clock().uninstall();
	});

	it('should get traffic data for skillgroup corresponding to chosenOffset', function() {
		createController(false);
		vm.moduleState.activeTab = 0;
		vm.clickedSkillGroupInPicker(skillGroups[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		vm.changeChosenOffset(1);
		$httpBackend.flush();
		expect(vm.moduleState.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryCalculatedCalls).toEqual('1,337.0');

		jasmine.clock().uninstall();
	});

	it('should get performance data for skillgroup corresponding to chosenOffset', function() {
		createController(false);
		vm.moduleState.activeTab = 1;
		vm.clickedSkillGroupInPicker(skillGroups[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		vm.changeChosenOffset(1);
		$httpBackend.flush();
		expect(vm.moduleState.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.summary.summaryServiceLevel).toEqual('90.0');

		jasmine.clock().uninstall();
	});

	it('should get staffing data for skillgroup corresponding to chosenOffset', function() {
		createController(false);
		vm.moduleState.activeTab = 2;
		vm.clickedSkillGroupInPicker(skillGroups[0]);

		var today = moment('0001-01-01T15:45:00').toDate();
		jasmine.clock().mockDate(today);
		vm.changeChosenOffset(1);
		$httpBackend.flush();
		expect(vm.moduleState.chosenOffset.value).toEqual(1);
		expect(vm.viewObj.forecastedStaffing.series[1]).toEqual(3);
		jasmine.clock().uninstall();
	});

	it('should not show abandon rate data and email-like skill chosen', function() {
		createController(false);
		vm.moduleState.activeTab = 1;

		vm.clickedSkillInPicker(skills[1]);

		$httpBackend.flush();

		expect(vm.viewObj.abandonedRateObj.series.length).toEqual(1);
		expect(vm.viewObj.summary.summaryAbandonedRate).toEqual(undefined);
	});

	it('should not show reforcasted agents data when email-like skill chosen', function() {
		createController(false);
		vm.moduleState.activeTab = 2;
		vm.clickedSkillInPicker(skills[1]);
		$httpBackend.flush();
		expect(vm.viewObj.forecastedStaffing.updatedSeries.length).toEqual(1);
	});

	it('should display date in timezone for current user', function() {
		createController(false);
		vm.clickedSkillInPicker(skills[1]);
		var today = moment('2018-01-02T03:00:00').toDate();
		jasmine.clock().mockDate(today);
		$httpBackend.flush();
		var currentDateDenver = moment('2018-01-01T00:00:00')
			.format('dddd, LL')
			.toLowerCase();
		expect(vm.getLocalDate(0).toLowerCase()).toEqual(currentDateDenver);
		jasmine.clock().uninstall();
	});

	it('should round AHT data series for incomming traffic chart', function() {
		createController(false);
		vm.moduleState.activeTab = 0;
		vm.clickedSkillInPicker(vm.skills[0]);
		$httpBackend.flush();

		var ahtSeries = vm.viewObj.actualAverageHandleTimeObj.series;
		var forcastedAhtSeries = vm.viewObj.forecastedAverageHandleTimeObj.series;
		expect(ahtSeries[ahtSeries.length - 1]).toEqual(170.9);
		expect(forcastedAhtSeries[forcastedAhtSeries.length - 1]).toEqual(201.4);
	});

	it('should remember selected tab if toggle is on', function() {
		createController(false, { WFM_Remember_My_Selection_In_Intraday_47254: true });
		vm.moduleState.activeTab = 1;
		vm.saveState();
		vm.moduleState.activeTab = 0;
		vm.loadState();

		expect(vm.moduleState.activeTab).toEqual(1);
	});

	it('should remember selected day offset if toggle is on', function() {
		createController(false, { WFM_Remember_My_Selection_In_Intraday_47254: true });
		vm.changeChosenOffset(-5, true);
		vm.moduleState.chosenOffset.value = 0;
		vm.loadState();

		expect(vm.moduleState.chosenOffset.value).toEqual(-5);
	});

	it('should NOT remember selected day offset if toggle is off', function() {
		createController(false, { WFM_Remember_My_Selection_In_Intraday_47254: false });
		vm.changeChosenOffset(-5, true);
		vm.changeChosenOffset(0, true);
		vm.loadState();

		expect(vm.moduleState.chosenOffset.value).toEqual(0);
	});

	it('should see staffing data when switching tab on other day', function() {
		createController(false);
		vm.changeChosenOffset(1);
		vm.clickedSkillInPicker(skills[0]);

		vm.moduleState.activeTab = 0;
		vm.pollActiveTabDataHelper(vm.moduleState.activeTab);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedCallsObj.series.length).toEqual(27);

		vm.moduleState.activeTab = 2;
		vm.pollActiveTabDataHelper(vm.moduleState.activeTab);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedStaffing.updatedSeries.length).toEqual(4);
	});

	it('should be able to select a skill of type back office', function() {
		createController(false);
		vm.clickedSkillInPicker(skills[2]);
		vm.moduleState.activeTab = 0;
		vm.pollActiveTabDataHelper(vm.moduleState.activeTab);
		$httpBackend.flush();

		expect(vm.viewObj.forecastedCallsObj.series.length).toEqual(27);
	});
});
