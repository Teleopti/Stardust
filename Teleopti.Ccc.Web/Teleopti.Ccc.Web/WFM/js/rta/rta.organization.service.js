(function () {
	'use strict';

	var rtaModule = angular.module('wfm.rta');
	rtaModule.service('RtaOrganizationService', [
	   '$filter', function ($filter) {
	   	var service = {};
	   	var organization = [
			{Name: 'London', siteId: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c', teams: [{ teamName: 'Preferences', teamId: 42 }, { teamName: 'NoPreferences', teamId: 43 }]},
			{ Name: 'Paris', siteId: '6a21c802-7a34-4917-8dfd-9b5e015ab461', teams: [{ teamName: 'Team1', teamId: 40 },{ teamName: 'Team2', teamId: 41 } ] },
			{ Name: 'Stores', siteId: '413157c4-74a9-482c-9760-a0a200d9f90f', teams: [{ teamName: 'London North', teamId: 50 },{ teamName: 'London South', teamId: 51 } ] },
			{ Name: 'BTS', siteId: '7a6c0754-4de8-48fb-8aee-a39a00b9d1c3', teams: [{ teamName: 'BTS', teamId: 600 }] }];


	   	service.getSiteName = function (siteIdParam) {
	   		var siteName = null;
	   		var result = $filter('filter')(organization, { siteId: siteIdParam });

	   		if (result.length > 0) {
	   			siteName = result[0].Name;
	   		}

	   		return siteName;
	   	};

	   	service.getTeamName = function (teamIdParam) {
	   		var teamName = null;
	   		var result = [];

	   		organization.forEach(function (site) {
	   			result = result.concat($filter('filter')(site.teams, { teamId: teamIdParam }));
	   		});

	   		if (result.length > 0) {
	   			teamName = result[0].teamName;
	   		}

	   		return teamName;
	   	};

	   	service.getSites = function () {
	   		return organization;
	   	};

	   	service.getTeams = function (siteId) {
	   		var selectedTeams = [];
	   		organization.forEach(function (site) {
	   			if (site.siteId === siteId) {
	   				site.teams.forEach(function (team) {
	   					selectedTeams.push(team);
	   				});
	   			}
	   		});
	   		return selectedTeams;
	   	}
	   	return service;

	   }]);
})();