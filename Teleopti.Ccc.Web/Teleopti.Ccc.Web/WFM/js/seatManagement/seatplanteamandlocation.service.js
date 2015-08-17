'use strict';

(function () {

	angular.module('wfm.seatPlan').service('seatplanTeamAndLocationService', seatplanTeamAndLocationService);

	seatplanTeamAndLocationService.$inject = ['seatPlanTranslateFactory'];

	function seatplanTeamAndLocationService(seatPlanTranslateFactory) {

		var service = {
			SelectTeam: selectTeam,
			SelectLocation: selectLocation,
			GetSelectedTeamsFromTeamList: getSelectedTeamsFromTeamList,
			GetSelectedLocationsFromLocationList: getSelectedLocationsFromLocationList,
			GetLocationDisplayText: getLocationDisplayText,
			GetTeamDisplayText: getTeamDisplayText,
		};

		function getLocationDisplayText(location) {
			if (location.Name == undefined) {
				return seatPlanTranslateFactory.TranslatedStrings["NoLocationsAvailable"];
			}
			return location.Name + " (" + seatPlanTranslateFactory.TranslatedStrings["SeatCountTitle"] + ": {0})".replace("{0}", location.Seats.length);
		};

		function getTeamDisplayText(teamHierarchyNode) {
			if (teamHierarchyNode.NumberOfAgents) {
				return teamHierarchyNode.Name + " (" + seatPlanTranslateFactory.TranslatedStrings["AgentCountTitle"] +
					": {0})".replace("{0}", teamHierarchyNode.NumberOfAgents);
			} else {
				return teamHierarchyNode.Name;
			}
		};

		function isTeam(team) {
			return (team.Children === undefined);
		};

		function setAllChildrenToOpposite(teamHierarchyObj, value) {

			if (isTeam(teamHierarchyObj)) {
				teamHierarchyObj.selected = value;
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

		function selectTeam(teamHierarchyObj, rootTeamHierarchyObjs) {
			if (isTeam(teamHierarchyObj)) {
				teamHierarchyObj.selected = !teamHierarchyObj.selected;
			} else {
				setAllChildrenToOpposite(teamHierarchyObj, !teamHierarchyObj.selected);
			}

			updateBuAndSiteStatus(rootTeamHierarchyObjs);
		};

		function selectLocation(location) {
			location.selected = location.Seats && location.Seats.length > 0 ? !location.selected : location.selected;
		};

		function getSelectedTeamsFromTeamList(teamList) {

			function getSelectedTeams(teams, result) {

				teams.forEach(function (team) {
					if (team.selected && isTeam(team)) {
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

		function getSelectedLocationsFromLocationList(locationList) {

			function getSelectedLocations(locations, result) {
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

		return service;

	};
}());


(function () {

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
		setupTranslatedString("DayOff");

		return {
			TranslatedStrings: translatedStrings
		}
	};

}());