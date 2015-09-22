(function(){
    'use strict';

    var rtaModule = angular.module('wfm.rta');
    rtaModule.service('RtaAgentsService', [ function () {
        	var service = {};
        	var agents = [{ Name: 'Herman', Team: 'Preferences', State: 'Good', Id: 0 }, { Name: 'Amanda', Team: 'Preferences', State: 'Good', Id: 1 }, { Name: 'Johan', Team: 'Preferences', State: 'Okey', Id: 2 }
          	, { Name: 'Pelle', Team: 'Preferences', State: 'Bad', Id: 0 }, { Name: 'Karin', Team: 'Preferences', State: 'Good', Id: 1 }, { Name: 'Erik', Team: 'Preferences', State: 'Bad', Id: 2 },
          	{ Name: 'Lena', Team: 'Preferences', State: 'Okey', Id: 0 }, { Name: 'Isac', Team: 'Preferences', State: 'Bad', Id: 1 }, { Name: 'Bo', Team: 'Preferences', State: 'Bad', Id: 2 }];

        	service.getAgents = function (teamId) {
        		return agents;
        	};

        	return service;
        }]);

})();