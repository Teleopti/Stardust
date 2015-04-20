'use strict';

angular.module('wfm.seatPlan')
	.controller('SeatPlanCtrl', ['seatPlanService','growl',
	function (seatPlanService, growl) {

		var vm = this;

		var startDate = moment.utc().add(1, 'months').startOf('month').toDate();
		var endDate = moment.utc().add(2, 'months').startOf('month').toDate();

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

			console.log(addSeatPlanCommand);

			if (selectedTeams.length == 0 || selectedLocations.length == 0) {

				onSelectedTeamsLocationsEmpty("teams or locations are unselected");

			} else {

				seatPlanService.addSeatPlan(addSeatPlanCommand).$promise.then(function(result) {
					onSuccessAddSeatPlan("Seat plan added successfully");
				});

			}
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

		function onSuccessAddSeatPlan(message) {
			growl.success("<i class='mdi mdi-thumb-up'></i> "+message+".", {
				ttl: 5000,
				disableCountDown: true
			});
		};

		function onSelectedTeamsLocationsEmpty(message) {
			growl.error("<i class='mdi  mdi-alert-octagon'></i> " + message + ".", {
				ttl: 5000,
				disableCountDown: true
			});
		};

	}]
);