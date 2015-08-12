'use strict';

(function() {

	angular.module('wfm.seatPlan').service('seatplanTeamAndLocationService', seatplanTeamAndLocationService);

	seatplanTeamAndLocationService.$inject = ['seatPlanTranslateFactory'];

	function seatplanTeamAndLocationService(seatPlanTranslateFactory) {

		var getLocationDisplayText = function (location) {
			if (location.Name == undefined) {
				return seatPlanTranslateFactory.TranslatedStrings["NoLocationsAvailable"];
			}
			return location.Name + " (" + seatPlanTranslateFactory.TranslatedStrings["SeatCountTitle"] + ": {0})".replace("{0}", location.Seats.length);
		};

		var getTeamDisplayText = function (teamHierarchyNode) {
			return teamHierarchyNode.Name;
		};

		var isTeam = function (team) {
			return (team.Children === undefined);
		};

		var selectTeam = function (teamHierarchyObj) {
			if (isTeam(teamHierarchyObj)) {
				teamHierarchyObj.selected = !teamHierarchyObj.selected;
			}
		};

		var selectLocation = function (location) {
			location.selected = location.Seats && location.Seats.length > 0 ? !location.selected : location.selected;
		};

		var getSelectedTeamsFromTeamList = function(teamList) {
		
			function getSelectedTeams(teams, result) {

				teams.forEach(function (team) {
					if (team.selected) {
						result.push(team.Id);
					}
					if (team.Children != null && team.Children.length > 0) {
						getSelectedTeams(team.Children, result);
					}
				});
			}

			var selectedTeams = [];
			getSelectedTeams(teamList, selectedTeams);
			return selectedTeams;
		};
		
		var getSelectedLocationsFromLocationList = function(locationList) {

			function getSelectedLocations(locations,result) {
				locations.forEach(function (location) {
					if (location.selected) {
						result.push(location.Id);
					}
					if (location.Children != null && location.Children.length > 0) {
						getSelectedLocations(location.Children, result);
					}
				});
			}

			var selectedLocations = [];
			getSelectedLocations(locationList, selectedLocations);
			return selectedLocations;
		};

		return {
			SelectTeam: selectTeam,
			SelectLocation: selectLocation,
			GetSelectedTeamsFromTeamList: getSelectedTeamsFromTeamList,
			GetSelectedLocationsFromLocationList: getSelectedLocationsFromLocationList,
			GetLocationDisplayText: getLocationDisplayText,
			GetTeamDisplayText: getTeamDisplayText
		};

	};
}());


(function() {

	angular.module('wfm.seatPlan').factory('seatPlanTranslateFactory', seatPlanTranslateFactory);

	seatPlanTranslateFactory.$inject = ['$translate'];

	function seatPlanTranslateFactory(translate) {

		var translatedStrings = {};

		var setupTranslatedString = function (key) {
			translate(key).then(function (result) {
				translatedStrings[key] = result;
			});
		};

		setupTranslatedString("NoLocationsAvailable");
		setupTranslatedString("SeatCountTitle");
		setupTranslatedString("AgentCountTitle");
		setupTranslatedString("TeamsOrLocationsAreUnselected");
		setupTranslatedString("SeatPlanSubmittedOK");

		return {
			TranslatedStrings: translatedStrings
		}
	};

}());