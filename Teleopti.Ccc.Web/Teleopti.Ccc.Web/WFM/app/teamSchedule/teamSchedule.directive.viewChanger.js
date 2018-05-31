(function () {
	'use strict';

	angular.module('wfm.teamSchedule').component('viewChanger', {
		controller: ViewChangerController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/view-changer.tpl.html',
		bindings: {
			keyword: '<?',
			selectedDate: '<?',
			selectedGroups: '<?',
			teamNameMap: '<?',
			selectedFavorite: '<?',
			selectedSortOption: '<?',
			staffingEnabled: '<?'
		}
	});

	ViewChangerController.$inject = ['$state', 'ViewStateKeeper'];

	function ViewChangerController($state, ViewStateKeeper) {
		var vm = this;

		vm.viewState = $state.current.name;

		vm.viewStateMap = {
			'day': 'teams.dayView',
			'week': 'teams.weekView'
		};

		vm.$onChanges = function () {
			ViewStateKeeper.save(getParams());
		}

		vm.changeView = function (viewState) {
			$state.go(viewState);
		};

		function getParams() {
			var params = {};
			params.do = true;
			if (angular.isDefined(vm.keyword)) params.keyword = vm.keyword;
			if (angular.isDefined(vm.selectedDate)) params.selectedDate = vm.selectedDate;
			if (angular.isDefined(vm.teamNameMap)) params.teamNameMap = vm.teamNameMap;
			if (angular.isDefined(vm.selectedFavorite)) params.selectedFavorite = vm.selectedFavorite;
			if (angular.isDefined(vm.selectedSortOption)) params.selectedSortOption = vm.selectedSortOption;
			if (angular.isDefined(vm.selectedGroups)) {
				if (vm.selectedGroups.mode === 'BusinessHierarchy') {
					params.selectedTeamIds = vm.selectedGroups.groupIds;
				} else {
					params.selectedGroupPage = {
						pageId: vm.selectedGroups.groupPageId,
						groupIds: vm.selectedGroups.groupIds
					};
				}
			}
			if (angular.isDefined(vm.selectedSortOption)) params.selectedSortOption = vm.selectedSortOption;
			if (angular.isDefined(vm.staffingEnabled)) params.staffingEnabled = vm.staffingEnabled;

			return params;
		}
	}
})();