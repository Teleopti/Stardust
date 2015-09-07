
(function () {

	angular.module('wfm.seatPlan').controller('TeamPickerCtrl', teamPickerDirectiveController);
	teamPickerDirectiveController.$inject = ['$scope', 'seatPlanService'];

	function teamPickerDirectiveController($scope, seatPlanService) {

		var vm = this;
		vm.teams = [];

		seatPlanService.teams.get().$promise.then(function (teams) {
			teams.show = true;
			vm.teams.push(teams);

			$scope.$watch("vm.selectedTeams", function (updatedTeams) {
				if (vm.teams.length > 0) {
					updateSelectedTeamsAfterExternalUpdate(vm.teams[0], updatedTeams);
					updateBuAndSiteStatus(vm.teams);
				}
			});
		});
		
		vm.getTeamDisplayText = function (teamHierarchyNode) {
			return teamHierarchyNode.Name;
		};

		vm.toggleNodeSelection = function (teamHierarchyObj) {
			if (isTeam(teamHierarchyObj)) {
				teamHierarchyObj.selected = !teamHierarchyObj.selected;
				toggleTeamSelection(teamHierarchyObj);
			} else {
				setAllChildrenToOpposite(teamHierarchyObj, !teamHierarchyObj.selected);
			}
			updateBuAndSiteStatus(vm.teams);
		};

		function toggleTeamSelection(teamHierarchyObj) {
			if (teamHierarchyObj.selected) {
				vm.selectedTeams.push(teamHierarchyObj.Id);
			} else {
				unselectTeam(teamHierarchyObj);
			}
		};

		function unselectTeam(teamHierarchyObj) {
			var index = vm.selectedTeams.indexOf(teamHierarchyObj.Id);
			if (index != -1) {
				vm.selectedTeams.splice(index, 1);
			}
		};

		function isTeam(team) {
			return (team.Children === undefined);
		};

		function setAllChildrenToOpposite(teamHierarchyObj, value) {

			if (isTeam(teamHierarchyObj)) {
				teamHierarchyObj.selected = value;
				toggleTeamSelection(teamHierarchyObj);
			} else {
				teamHierarchyObj.Children.forEach(function (child) {
					setAllChildrenToOpposite(child, value);
				});
			}
		}

		function updateBuAndSiteStatus(rootTeamHierarchyObjs) {

			function setBuAndSiteStatus(hierarchyObj) {
				if (hierarchyObj.Children == null) return;

				var isAnyChildrenSelected = false;
				hierarchyObj.Children.forEach(function (child) {
					setBuAndSiteStatus(child);
					if (child.selected === true) isAnyChildrenSelected = true;
				});
				hierarchyObj.selected = isAnyChildrenSelected;
			}

			rootTeamHierarchyObjs.forEach(function (rootTeamHierarchyObj) {
				setBuAndSiteStatus(rootTeamHierarchyObj);
			});
		};

		function getSelectedTeams(node, teams) {

			if (isTeam(node) && node.selected) {
				teams.push(node.Id);
			}

			if (node.Children) {
				for (var i in node.Children) {
					getSelectedTeams(node.Children[i], teams);
				}
			}
		};

		function updateSelectedTeamsAfterExternalUpdate(node, updatedTeams) {
			var index = updatedTeams.indexOf(node.Id);

			if (isTeam(node)) {
				node.selected = index != -1;
			} else {
				node.Children.forEach(function (child) {
					updateSelectedTeamsAfterExternalUpdate(child, updatedTeams);
				});
			}
		};

	};
}());



(function () {

	var directive = function () {

		return {
			controller: 'TeamPickerCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				selectedTeams: '='
			},
			templateUrl: "js/seatManagement/html/teampicker.html",
		};
	};

	angular.module('wfm.seatPlan').directive('teamPicker', directive);

}());