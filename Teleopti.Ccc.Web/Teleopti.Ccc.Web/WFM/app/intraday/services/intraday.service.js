(function() {
	'use strict';
	angular.module('wfm.intraday').service('intradayService', ['$resource', '$http', '$window', intradayService]);

	function intradayService($resource, $http, $window) {
		this.getSkills = $resource(
			'../api/intraday/skills',
			{},
			{
				query: {
					method: 'GET',
					params: {},
					isArray: true,
					cancellable: true
				}
			}
		);

		this.getSkillGroupMonitorStatistics = $resource(
			'../api/intraday/monitorskillareastatistics/:id',
			{ id: '@id' },
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillMonitorStatistics = $resource(
			'../api/intraday/monitorskillstatistics/:id',
			{ id: '@id' },
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillMonitorStatisticsByDayOffset = $resource(
			'../api/intraday/monitorskillstatistics/:id/:dayOffset',
			{
				id: '@id',
				dayOffset: '@dayOffset'
			},
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillGroupMonitorStatisticsByDayOffset = $resource(
			'../api/intraday/monitorskillareastatistics/:id/:dayOffset',
			{
				id: '@id',
				dayOffset: '@dayOffset'
			},
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillGroupMonitorPerformance = $resource(
			'../api/intraday/monitorskillareaperformance/:id',
			{ id: '@id' },
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillMonitorPerformance = $resource(
			'../api/intraday/monitorskillperformance/:id',
			{ id: '@id' },
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillMonitorPerformanceByDayOffset = $resource(
			'../api/intraday/monitorskillperformance/:id/:dayOffset',
			{
				id: '@id',
				dayOffset: '@dayOffset'
			},
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillGroupMonitorPerformanceByDayOffset = $resource(
			'../api/intraday/monitorskillareaperformance/:id/:dayOffset',
			{
				id: '@id',
				dayOffset: '@dayOffset'
			},
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillGroupStaffingData = $resource(
			'../api/intraday/monitorskillareastaffing/:id',
			{ id: '@id' },
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillStaffingData = $resource(
			'../api/intraday/monitorskillstaffing/:id',
			{ id: '@id' },
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillStaffingDataByDayOffset = $resource(
			'../api/intraday/monitorskillstaffing/:id/:dayOffset',
			{
				id: '@id',
				dayOffset: '@dayOffset'
			},
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getSkillGroupStaffingDataByDayOffset = $resource(
			'../api/intraday/monitorskillareastaffing/:id/:dayOffset',
			{
				id: '@id',
				dayOffset: '@dayOffset'
			},
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getLatestStatisticsTimeForSkillGroup = $resource(
			'../api/intraday/lateststatisticstimeforskillarea/:id',
			{ id: '@id' },
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getLatestStatisticsTimeForSkill = $resource(
			'../api/intraday/lateststatisticstimeforskill/:id',
			{ id: '@id' },
			{
				query: {
					method: 'GET',
					params: {},
					isArray: false,
					cancellable: true
				}
			}
		);

		this.getIntradayExportForSkillGroup = function(data, successCb, errorCb) {
			$http({
				url: '../api/intraday/exportskillareadatatoexcel',
				method: 'POST',
				data: data,
				responseType: 'arraybuffer',
				headers: {
					Accept:
						'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
				}
			})
				.success(successCb)
				.error(errorCb);
		};

		this.getIntradayExportForSkill = function(data, successCb, errorCb) {
			$http({
				url: '../api/intraday/exportskilldatatoexcel',
				method: 'POST',
				data: data,
				responseType: 'arraybuffer',
				headers: {
					Accept:
						'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
				}
			})
				.success(successCb)
				.error(errorCb);
		};

		this.saveIntradayState = function(state) {
			$window.localStorage.intradayModuleState = angular.toJson(state);
		};

		this.loadIntradayState = function() {
			return angular.fromJson($window.localStorage.intradayModuleState);
		};
	}
})();
