(function () {
	'use strict';

	angular.module('wfm.reports').controller('LeaderBoardController', LeaderBoardCtrl);

	LeaderBoardCtrl.$inject = ['LeaderBoardService'];


	function LeaderBoardCtrl(LeaderBoardSvc) {
		var vm = this;

		vm.fakeData = { "Agents": [{ "AgentName": "John Smith", "Gold": 10, "Silver": 6, "Bronze": 3 }, { "AgentName": "Jon Kleinsmith", "Gold": 10, "Silver": 3, "Bronze": 4 }, { "AgentName": "Stephen Bay", "Gold": 4, "Silver": 6, "Bronze": 4 }, { "AgentName": "Robert Klashner", "Gold": 4, "Silver": 2, "Bronze": 2 }, { "AgentName": "Carlos Oliveira", "Gold": 3, "Silver": 0, "Bronze": 0 }, { "AgentName": "Pierre Baldi", "Gold": 2, "Silver": 3, "Bronze": 5 }, { "AgentName": "Juancho Banaag", "Gold": 2, "Silver": 2, "Bronze": 6 }, { "AgentName": "Prashant Arora", "Gold": 2, "Silver": 1, "Bronze": 5 }, { "AgentName": "Kari Nies", "Gold": 2, "Silver": 0, "Bronze": 0 }, { "AgentName": "Daniel Billsus", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Michael Kantor", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Bill Gates", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Candy Mamer", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Tim McMahon", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Sharad Mehrotra", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "George Lueker", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Steve Novack", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Dmitry Pavlov", "Gold": 0, "Silver": 3, "Bronze": 7 }, { "AgentName": "Ashley Andeen", "Gold": 0, "Silver": 1, "Bronze": 1 }, { "AgentName": "John Smith", "Gold": 10, "Silver": 6, "Bronze": 3 }, { "AgentName": "Jon Kleinsmith", "Gold": 10, "Silver": 3, "Bronze": 4 }, { "AgentName": "Stephen Bay", "Gold": 4, "Silver": 6, "Bronze": 4 }, { "AgentName": "Robert Klashner", "Gold": 4, "Silver": 2, "Bronze": 2 }, { "AgentName": "Carlos Oliveira", "Gold": 3, "Silver": 0, "Bronze": 0 }, { "AgentName": "Pierre Baldi", "Gold": 2, "Silver": 3, "Bronze": 5 }, { "AgentName": "Juancho Banaag", "Gold": 2, "Silver": 2, "Bronze": 6 }, { "AgentName": "Prashant Arora", "Gold": 2, "Silver": 1, "Bronze": 5 }, { "AgentName": "Kari Nies", "Gold": 2, "Silver": 0, "Bronze": 0 }, { "AgentName": "Daniel Billsus", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Michael Kantor", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Bill Gates", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Candy Mamer", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Tim McMahon", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Sharad Mehrotra", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "George Lueker", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Steve Novack", "Gold": 1, "Silver": 0, "Bronze": 0 }, { "AgentName": "Dmitry Pavlov", "Gold": 0, "Silver": 3, "Bronze": 7 }, { "AgentName": "Ashley Andeen", "Gold": 0, "Silver": 1, "Bronze": 1 }] };

		vm.searchOptions = {
			keyword: '',
			pageSize: 10
		};
	}
})();