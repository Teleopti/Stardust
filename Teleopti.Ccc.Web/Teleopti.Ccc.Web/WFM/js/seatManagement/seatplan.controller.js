'use strict';

angular.module('wfm.seatPlan')
	.controller('SeatPlanCtrl', ['$scope', '$state', 'SeatPlanService',
	function ($scope, $state, SeatPlanService) {

		var startDate = moment().add(1, 'months').startOf('month').toDate();
		var endDate = moment().add(2, 'months').startOf('month').toDate();
		$scope.period = { startDate: startDate, endDate: endDate };
		$scope.locations = [];
		$scope.teams = [];
		
		$scope.locations.push(SeatPlanService.locations.get());
		$scope.teams.push(SeatPlanService.teams.get());

		$scope.getLocationDisplayText = function (location) {
			if (location.Name == undefined) {
				return "No Locations available.";
			}
			return location.Name + " (seats: {0})".replace("{0}", location.Seats.length);
			
		};

		$scope.getTeamDisplayText = function (teamHierarchyNode) {
			if (teamHierarchyNode.NumberOfAgents) {
				return teamHierarchyNode.Name + " (agents: {0})".replace("{0}", teamHierarchyNode.NumberOfAgents);
			} else {
				return teamHierarchyNode.Name;
			}
			
		};

		$scope.addSeatPlan = function() {
			var selectedTeams = [];
			if ($scope.teams.length > 0) {
				var node = $scope.teams[0];
				getSelectedTeams(node, selectedTeams);
			}

			var selectedLocations = [];
			if ($scope.locations.length > 0) {
				var node = $scope.locations[0];
				getSelectedLocations(node, selectedLocations);
			}

			var addSeatPlanCommand = {
				StartDate: $scope.period.startDate,
				EndDate: $scope.period.endDate,
				Teams: selectedTeams,
				Locations: selectedLocations
			};

			SeatPlanService.seatPlan.add(addSeatPlanCommand).$promise.then(function (result) {
				NoticeCtrl.triggerCallbacks();
			});
		};

		$scope.selectTeam = function(team) {
			team.selected = team.NumberOfAgents && team.NumberOfAgents>0 ? !team.selected : team.selected;
		};

		function getSelectedTeams(node, teams) {

			if (node.NumberOfAgents && node.NumberOfAgents > 0 && node.selected) {
				teams.push(node.Id);
			}

			if (node.Children) {
				for (var i in node.Children) {
					$scope.getSelectedTeams(node.Children[i], teams);
				}
			}
		};

		function getSelectedLocations(node, locations) {
			if (node.selected && node.Seats && node.Seats.length > 0) {
				locations.push(node.Id);
			}
			if (node.Children) {
				for (var i in node.Children) {
					$scope.getSelectedLocations(node.Children[i], locations);
				}
			}

		};

	}]
);