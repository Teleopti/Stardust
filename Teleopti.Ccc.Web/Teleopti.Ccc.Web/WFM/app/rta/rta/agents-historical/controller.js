(function () {
	'use strict';
	angular.module('wfm.rta').controller('RtaAgentsHistoricalController', RtaAgentsHistoricalController);
	RtaAgentsHistoricalController.$inject = [];

	function RtaAgentsHistoricalController() {
		var vm = this;

		vm.agents = [
			{Name: 'John Smith', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Pierre Baldi', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Daniel Billsus', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'Prashant Arora', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Ashley Andeen', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Juancho Banaag', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Stephen Bay', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Michael Kantor', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'Bill Gates', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'John King', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Jon Kleinsmith', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Robert Klashner', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Candy Mamer', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'Tim McMahon', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'Sharad Mehrotra', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'George Lueker', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'Steve Novack', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'Kari Nies', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'Dmitry Pavlov', SiteAndTeamName: 'London/Team Preferences'}
			, {Name: 'Carlos Oliveira', SiteAndTeamName: 'Paris/Team 1'}
			, {Name: 'Johan Damm', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Herman Dahlén', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Karim Chabane', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Ben Willmott', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Yuanyuan Hu', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Pengxia Li', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Bernie Cong', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Billy Chang', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Jennie Lee', SiteAndTeamName: 'East Asia/Guangzhou'}
			, {Name: 'Mindy Wong', SiteAndTeamName: 'East Asia/Guangzhou'}
		];

		vm.agents.forEach(function (a) {
			a.smilies = makeSmilies()
		});

		vm.smilies = makeSmilies();
		
		function makeSmilies() {
			return [
				{e: "😃", header: '-6', color: randomColor(), width: '40px'},
				{e: "😃", header: '-5', color: randomColor(), width: '40px'},
				{e: "😃", header: '-4', color: randomColor(), width: '40px'},
				{e: "😃", header: '-3', color: randomColor(), width: '40px'},
				{e: "😃", header: '-2', color: randomColor(), width: '40px'},
				{e: "😃", header: '-1', color: randomColor(), width: '40px'},
				{e: "😃", header: '2017-12-18', color: randomColor(), width: '100px'}
			];
		};

		function randomColor() {
			var random = Math.random() * (100 - 0) + 0;
			if (random < 50)
				return '0 0 0 green';
			if (random < 55)
				return '0 0 0 yellow';
			if (random < 60)
				return '0 0 0 orange';
			if (random < 70)
				return '0 0 0 lightgray';
			return '0 0 0 red';
		};
	}
})();
