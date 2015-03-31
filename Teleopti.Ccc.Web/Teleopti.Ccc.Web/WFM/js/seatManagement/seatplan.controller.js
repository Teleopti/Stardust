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
			if (this.teams.length > 0) {
				getSelectedTeams(this.teams[0], selectedTeams);
			}

			var selectedLocations = [];
			if (this.locations.length > 0) {
				getSelectedLocations(this.locations[0], selectedLocations);
			}

			var addSeatPlanCommand = {
				StartDate: this.period.startDate,
				EndDate: this.period.endDate,
				Teams: selectedTeams,
				Locations: selectedLocations
			};

			SeatPlanService.seatPlan.add(addSeatPlanCommand).$promise.then(function (result) {
				alert('seat plan submitted');
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
					getSelectedTeams(node.Children[i], teams);
				}
			}
		};

		function getSelectedLocations(node, locations) {
			if (node.selected && node.Seats && node.Seats.length > 0) {
				locations.push(node.Id);
			}
			if (node.Children) {
				for (var i in node.Children) {
					getSelectedLocations(node.Children[i], locations);
				}
			}

		};

	}]
);