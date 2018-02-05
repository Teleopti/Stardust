(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('viewChanger', viewChangerDirective);

	function viewChangerDirective() {
		return {
			restrict: 'E',
			templateUrl: 'app/teamSchedule/html/view-changer.tpl.html',
			replace: true,
			controllerAs: 'vc',
			scope: {
				keyword: '=?',
				selectedDate: '=?',
				selectedGroups: '=?',
				teamNameMap: '=?',
				selectedFavorite: '=?',
				selectedSortOption: '=?',
				staffingEnabled: '=?'
			},
			bindToController: true,
			controller: viewChangerController
		};
	}

	viewChangerController.$inject = ['$state'];

	function viewChangerController($state) {
		var vc = this;

		vc.viewState = $state.current.name;

		vc.viewStateMap = {
			'day': 'teams.dayView',
			'week': 'teams.weekView'
		};

		vc.changeView = function (viewState) {
			var params = {};
			params.do = true;
			if (angular.isDefined(vc.keyword)) params.keyword = vc.keyword;
			if (angular.isDefined(vc.selectedDate)) params.selectedDate = vc.selectedDate;
			if (angular.isDefined(vc.teamNameMap)) params.teamNameMap = vc.teamNameMap;
			if (angular.isDefined(vc.selectedFavorite)) params.selectedFavorite = vc.selectedFavorite;
			if (angular.isDefined(vc.selectedSortOption)) params.selectedSortOption = vc.selectedSortOption;
			if (angular.isDefined(vc.selectedGroups)) {
				if (vc.selectedGroups.mode === 'BusinessHierarchy') {
					params.selectedTeamIds = vc.selectedGroups.groupIds;
				} else {
					params.selectedGroupPage = {
						pageId: vc.selectedGroups.groupPageId,
						groupIds: vc.selectedGroups.groupIds
					};
				}
			}
			if (angular.isDefined(vc.selectedSortOption)) params.selectedSortOption = vc.selectedSortOption;
			if (angular.isDefined(vc.staffingEnabled)) params.staffingEnabled = vc.staffingEnabled;
			$state.go(viewState, params);
		};
	}
})();