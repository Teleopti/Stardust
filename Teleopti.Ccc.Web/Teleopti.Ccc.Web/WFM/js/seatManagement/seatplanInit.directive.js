(function () {

	angular.module('wfm.seatPlan').controller('SeatPlanInitCtrl', seatPlanInitDirectiveController);
	seatPlanInitDirectiveController.$inject = ['seatPlanService', 'growl', '$translate'];

	function seatPlanInitDirectiveController(seatPlanService, growl, translate) {

		var vm = this;

		vm.locations = [];
		vm.teams = [];

		seatPlanService.locations.get().$promise.then(function (locations) {
			locations.show = true;
			vm.locations.push(locations);
		});

		seatPlanService.teams.get().$promise.then(function (teams) {
			teams.show = true;
			vm.teams.push(teams);
		});

		vm.loadDefaultDates = function () {
			if (vm.start != null) {

				vm.period = {
					startDate: moment.utc(vm.start).toDate(),
					endDate: moment.utc(vm.end).toDate()
				};
			} else {

				var startDate = moment.utc().add(1, 'months').startOf('month').toDate();
				var endDate = moment.utc().add(2, 'months').startOf('month').toDate();
				vm.period = { startDate: startDate, endDate: endDate };

			}
		};

		vm.setupTranslatedStrings = function() {
			
			vm.translatedStrings = {};

			vm.setupTranslatedString("NoLocationsAvailable");
			vm.setupTranslatedString("SeatCountTitle");
			vm.setupTranslatedString("AgentCountTitle");
			vm.setupTranslatedString("TeamsOrLocationsAreUnselected");
			vm.setupTranslatedString("SeatPlanSubmittedOK");
		}

		vm.setupTranslatedString = function(key) {
			translate(key).then(function (result) {
				vm.translatedStrings[key] = result;
			});
		};


		vm.setupTranslatedStrings();
		vm.loadDefaultDates();

		vm.getLocationDisplayText = function (location) {
			if (location.Name == undefined) {
				return vm.translatedStrings["NoLocationsAvailable"];
			}
			return location.Name + " ("+ vm.translatedStrings["SeatCountTitle"]+": {0})".replace("{0}", location.Seats.length);
		};

		vm.getTeamDisplayText = function (teamHierarchyNode) {
			return teamHierarchyNode.Name;
		};

		vm.addSeatPlan = function () {

			vm.processingSeatPlan = true;

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

			if (selectedTeams.length == 0 || selectedLocations.length == 0) {
				onSelectedTeamsLocationsEmpty(vm.translatedStrings["TeamsOrLocationsAreUnselected"]);
				vm.processingSeatPlan = false;
			}
			else {
				seatPlanService.addSeatPlan(addSeatPlanCommand).$promise.then(function (result) {
					onSuccessAddSeatPlan(vm.translatedStrings["SeatPlanSubmittedOK"]);
					vm.processingSeatPlan = false;
					vm.onSeatPlanComplete();
				});

			}
		};

		vm.isTeam = function(team) {
			return (team.Children === undefined);
		};

		vm.selectTeam = function (teamHierarchyObj) {
			if (vm.isTeam(teamHierarchyObj)) {
				teamHierarchyObj.selected = !teamHierarchyObj.selected;
			}
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
			growl.success("<i class='mdi mdi-thumb-up'></i> " + message + ".", {
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

	};

}());


(function () {

	var directive = function () {

		return {
			controller: 'SeatPlanInitCtrl',
			controllerAs: 'vm',
			bindToController: true,
			require: ['seatPlanInit', '^teleoptiCard'],
			scope: {
				start: '@',
				end: '@',
				onSeatPlanComplete:'&'
			},
			templateUrl: "js/seatManagement/html/seatplaninit.html",
			
		};
	};

	angular.module('wfm.seatPlan').directive('seatPlanInit', directive);

	
}());