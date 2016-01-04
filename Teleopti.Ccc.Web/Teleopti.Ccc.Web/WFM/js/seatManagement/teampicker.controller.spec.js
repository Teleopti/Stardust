'use strict';

describe('seatplan report controller tests', function () {

	var $q,
		$rootScope,
		$httpBackend,
		controller;

	beforeEach(function () {
		module('wfm.seatPlan');
		module('pascalprecht.translate');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		controller = setUpController(_$controller_);		
	}));
	
	it('should update parent node when selecting children', inject(function () {
		var teamFactory = TeamFactory();
		var site = teamFactory.CreateTeam('1', false),
			team1 = teamFactory.CreateTeam('2', false),
			team2 = teamFactory.CreateTeam('3', false);
		site.Children = [team1, team2];

		controller.teams = [site];
		controller.selectedTeams = [];
		controller.toggleNodeSelection(team1);

		site = controller.teams[0];
		expect(site.selected).toEqual(true);
	}));

	it('should cancel parent node when unselect all children', inject(function () {
		var teamFactory = TeamFactory();
		var site = teamFactory.CreateTeam('1', true),
			team1 = teamFactory.CreateTeam('2', true),
			team2 = teamFactory.CreateTeam('3', false);
		site.Children = [team1, team2];

		controller.teams = [site];
		controller.selectedTeams = [];
		controller.toggleNodeSelection(team1);

		site = controller.teams[0];
		expect(site.selected).toEqual(false);
	}));

	it('should select all children when choose a site', inject(function () {
		var teamFactory = TeamFactory();
		var site = teamFactory.CreateTeam('1', false),
			team1 = teamFactory.CreateTeam('2', false),
			team2 = teamFactory.CreateTeam('3', false);
		site.Children = [team1, team2];

		controller.teams = [site];
		controller.selectedTeams = [];
		controller.toggleNodeSelection(site);

		site = controller.teams[0],
		team1 = site.Children[0],
		team2 = site.Children[1];

		expect(site.selected
			&& team1.selected
			&& team2.selected).toEqual(true);
	}));

	it('should select the second generation children when chose root', inject(function () {
		var teamFactory = TeamFactory();

		var bu = teamFactory.CreateTeam('1', false),
			site1 = teamFactory.CreateTeam('2', false),
			site2 = teamFactory.CreateTeam('3', false),
			team1 = teamFactory.CreateTeam('4', false),
			team2 = teamFactory.CreateTeam('5', false);
		site1.Children = [team1, team2],
		bu.Children = [site1, site2];

		controller.teams = [bu];
		controller.selectedTeams = [];
		controller.toggleNodeSelection(bu);

		bu = controller.teams[0],
		site1 = bu.Children[0],
		site2 = bu.Children[1],
		team1 = site1.Children[0],
		team2 = site1.Children[1];

		expect(bu.selected
			&& site1.selected
			&& team1.selected
			&& team2.selected
			&& site2.selected).toEqual(true);
	}));

	var mockSeatPlanService = {
		teams: {
			get: function (param) {
				var queryDeferred = $q.defer();
				var result = [];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	function TeamFactory() {

		function createTeam(id, isSelected) {
			return {
				Children: undefined,
				Id: id,
				Name: 'team ' + id,
				selected: isSelected
			};
		}

		return {
			CreateTeam: createTeam
		};
	};
	
	function setUpController($controller) {

		var scope = $rootScope.$new();

		return $controller('TeamPickerCtrl',
		{
			$scope : scope,
			seatPlanService: mockSeatPlanService
		});
	};

});
