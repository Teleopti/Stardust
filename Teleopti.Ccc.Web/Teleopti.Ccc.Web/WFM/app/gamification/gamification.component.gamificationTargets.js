(function (angular) { 'use strict';

	angular.module('wfm.gamification')
		.component('gamificationTargets', {
			templateUrl: 'app/gamification/html/g.component.gamificationTargets.tpl.html',
			controller: [
				'$log',
				'$scope',
				'$timeout',
				'$translate',
				'GamificationDataService',
				GamificationTargetsController
			]
		});

	function GamificationTargetsController($log, $scope, $timeout, $translate, dataService) {
		var ctrl = this;

		var selectedText = '';

		function TeamsResult(result) {
			Object.defineProperty(this, '_result', { value: result });
			this.current = result;
		}

		TeamsResult.prototype.filterByName = function (text) {
			var query = new RegExp(text, 'i');
			this.current = this._result.filter(function (team) {
				return team.name.search(query) != -1;
			});
		};

		var filter = '';

		Object.defineProperty(ctrl, 'filter', {
			get: function () { return filter; },
			set: function (newValue) {
				filter = newValue;
				ctrl.teamsResult.filterByName(filter);
			}
		});

		ctrl.selectClosed = function () {
			var newText = ctrl.sitePickerSelectedText();
			if (newText !== selectedText) {
				selectedText = newText;
				loadTeams();
			}
		};

		ctrl.sitePickerSelectedText = function () {
			if (ctrl.selectedSites.length === 0) {
				return $translate.instant('SelectOneOrMoreSites');
			}
			return ctrl.selectedSites.sort(function (a, b) {
				if (a.position < b.position) {
					return -1;
				}
				return 1;
			})
			.map(function (site) { return site.name; })
			.join(', ');
		};

		ctrl.onAppliedSettingChange = function (teamIds, newValue) {
			dataService.updateAppliedSetting(teamIds, newValue)
				.then(onSuccess, onError);

			function onSuccess(saved) {
				$log.debug(saved);
			}

			function onError(response) {
				$log.error(response);
			}
		};

		ctrl.selectedSites = [];

		function loadTeams() {
			startSpinner();
			var siteIds = ctrl.selectedSites.map(function (site) { return site.id; });
			dataService.fetchTeams(siteIds).then(function (teams) {
				ctrl.teamsResult = new TeamsResult(teams);
				stopSpinner();
			});
		}

		function startSpinner() { ctrl.isBusy = true; }
		function stopSpinner() { ctrl.isBusy = false; }

		dataService.fetchSites().then(function (sites) {
			ctrl.sites = sites;
		});

		$scope.$on('gamification.selectTargetsTab', function (event, args) {
			fetchSettingList();
		});

		function fetchSettingList() {
			return dataService.fetchSettingList().then(function (list) {
				if (!angular.isArray(list)) return;
				ctrl.settings = list;
				updateAppliedSetting();
			});
		}

		function updateAppliedSetting() {
			if (ctrl.teamsResult !== undefined) {
				ctrl.teamsResult.current.forEach(function (obj) {
					var isContain = false;
					ctrl.settings.forEach(function (setting) {
						if (setting.id === obj.appliedSettingId) isContain = true; 
					});
					if (!isContain) obj.appliedSettingId = '00000000-0000-0000-0000-000000000000';
				});
			}
		}
	}

})(angular);