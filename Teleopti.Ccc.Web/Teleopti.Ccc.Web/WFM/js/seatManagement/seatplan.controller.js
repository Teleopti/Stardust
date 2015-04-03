'use strict';

angular.module('wfm.seatPlan')
	.controller('SeatPlanCtrl', ['seatPlanService',
	function (seatPlanService) {

		var vm = this;

		var startDate = moment().add(1, 'months').startOf('month').toDate();
		var endDate = moment().add(2, 'months').startOf('month').toDate();

		vm.period = { startDate: startDate, endDate: endDate };
		vm.locations = [];
		vm.teams = [];

		vm.locations.push(seatPlanService.locations.get());
		vm.teams.push(seatPlanService.teams.get());

		vm.getLocationDisplayText = function (location) {
			if (location.Name == undefined) {
				return "No Locations available.";
			}
			return location.Name + " (seats: {0})".replace("{0}", location.Seats.length);

		};

		vm.getTeamDisplayText = function (teamHierarchyNode) {
			if (teamHierarchyNode.NumberOfAgents) {
				return teamHierarchyNode.Name + " (agents: {0})".replace("{0}", teamHierarchyNode.NumberOfAgents);
			} else {
				return teamHierarchyNode.Name;
			}

		};

		vm.addSeatPlan = function () {
			var selectedTeams = [];
			if (vm.teams.length > 0) {
				getSelectedTeams(vm.teams[0], selectedTeams);
			}

			var selectedLocations = [];
			if (vm.locations.length > 0) {
				getSelectedLocations(vm.locations[0], selectedLocations);
			}

			var addSeatPlanCommand = {
				StartDate: vm.period.startDate,
				EndDate: vm.period.endDate,
				Teams: selectedTeams,
				Locations: selectedLocations
			};

			//seatPlanService.seatPlan.add(addSeatPlanCommand).$promise.then(function (result) {
			seatPlanService.addSeatPlan(addSeatPlanCommand).$promise.then(function (result) {
				//Robtodo:notice on successful submit
				displaySuccessNotification("Seat Plan Saved");
			});
		};

		vm.selectTeam = function (team) {
			team.selected = team.NumberOfAgents && team.NumberOfAgents > 0 ? !team.selected : team.selected;
		};

		vm.selectLocation = function (location) {
			location.selected = location.Seats && location.Seats.length > 0 ? !location.selected : location.selected;
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