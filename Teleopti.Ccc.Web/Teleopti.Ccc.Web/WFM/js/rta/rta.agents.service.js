﻿(function(){
    'use strict';

    var rtaModule = angular.module('wfm.rta');
    rtaModule.service('RtaAgentsService', [
        '$resource', 'RtaService', '$filter', function ($resource, RtaService, $filter) {
        	var service = {};
        	var agents = [{ Name: 'Herman', Team: 'Preferences', State: 'Good', Id: 0 }, { Name: 'Amanda', Team: 'Preferences', State: 'Good', Id: 1 }, { Name: 'Johan', Team: 'Preferences', State: 'Okey', Id: 2 }
          	, { Name: 'Pelle', Team: 'Preferences', State: 'Bad', Id: 0 }, { Name: 'Karin', Team: 'Preferences', State: 'Good', Id: 1 }, { Name: 'Erik', Team: 'Preferences', State: 'Bad', Id: 2 },
          	{ Name: 'Lena', Team: 'Preferences', State: 'Okey', Id: 0 }, { Name: 'Isac', Team: 'Preferences', State: 'Bad', Id: 1 }, { Name: 'Bo', Team: 'Preferences', State: 'Bad', Id: 2 }];

        	service.getAgents = function (teamId) {
        		return agents;
        	};

        	return service;
        }]);

    rtaModule.service('RtaOrganizationService', [
        '$filter',function($filter) {
            var service = {};
            var organization = [{ siteName: 'London', siteId: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c', teams: [{ teamName: 'Preferences', teamId: 42 }, 
				{ teamName: 'NoPreferences', teamId: 43 }] }, { siteName: 'Paris', siteId: '6a21c802-7a34-4917-8dfd-9b5e015ab461', teams: [{ teamName: 'Agile', teamId: 40 }] }];


            service.getSiteName = function (siteIdParam) {
            	var siteName = null;
            	var result = $filter('filter')(organization, { siteId: siteIdParam });

            	if(result.length > 0){
            		siteName = result[0].siteName;
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