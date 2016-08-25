'use strict';
describe('RtaTeamsCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaTeamsCtrl');

		$fakeBackend.clear();
	}));

	it('should display team for site', function () {
		stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
		$fakeBackend.withTeam({
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			Name: "Green",
			NumberOfAgents: 1
		});

		$controllerBuilder.createController();

		expect(scope.teams[0].Name).toEqual("Green");
		expect(scope.teams[0].NumberOfAgents).toEqual(1);
	});

	it('should display agents out of adherence in the team', function () {
		stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
		$fakeBackend.withTeam({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withTeam({
				Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withTeamAdherence({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565",
				OutOfAdherence: 1
			})
			.withTeamAdherence({
				Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495",
				OutOfAdherence: 5,
			});

		$controllerBuilder.createController();

		expect(scope.teams[0].OutOfAdherence).toEqual(1);
		expect(scope.teams[1].OutOfAdherence).toEqual(5);
	});

	it('should display site name London', function() {
		stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
		$fakeBackend.withSite({
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			Name: "London"
		});

		$controllerBuilder.createController();

		expect(scope.siteName).toEqual("London");
	});

	it('should display site name Paris', function() {
		stateParams.siteId = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		$fakeBackend.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London"
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				Name: "Paris"
			});

		$controllerBuilder.createController();

		expect(scope.siteName).toEqual("Paris");
	});

	it('should update adherence', function () {
		$fakeBackend.withTeam({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565"
			})
			.withTeamAdherence({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565",
				OutOfAdherence: 1
			});

		$controllerBuilder.createController()
			.apply(function() {
				$fakeBackend.clearTeamAdherences()
					.withTeamAdherence({
						Id: "2d45a50e-db48-41db-b771-a53000ef6565",
						OutOfAdherence: 3
					});
			})
			.wait(5000);

		expect(scope.teams[0].OutOfAdherence).toEqual(3);
	});

	it('should stop polling when page is about to destroy', function() {
		$controllerBuilder.createController()
			.wait(5000);

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	fit('should go to agents for multiple teams', function() {
		$fakeBackend.withTeam({
				Id: "2d45a50e-db48-41db-b771-a53000ef6565"
			})
			.withTeam({
				Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
			});
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				scope.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
				scope.toggleSelection("0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495");
				scope.openSelectedTeams();
			});

		expect($state.go).toHaveBeenCalledWith('rta.agents-teams', {
			teamIds: ['2d45a50e-db48-41db-b771-a53000ef6565',
				"0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
			]
		});
	});

	fit('should go to agents after deselecting team', function() {
		$fakeBackend.withTeam({
			Id: "2d45a50e-db48-41db-b771-a53000ef6565"
		})
		.withTeam({
			Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
		});
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				scope.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
				scope.toggleSelection("0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495");
				scope.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
				scope.openSelectedTeams();
			});

		expect($state.go).toHaveBeenCalledWith('rta.agents-teams', {
			teamIds: ["0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"]
		});
	});

	it('should display site name Stores', function() {
		stateParams.siteId = "413157c4-74a9-482c-9760-a0a200d9f90f";
		$fakeBackend.withSite({
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name:
					"London"
			})
			.withSite({
				Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
				Name: "Paris"
			})
			.withSite({
				Id: "413157c4-74a9-482c-9760-a0a200d9f90f",
				Name: "Stores"
			});

		$controllerBuilder.createController();

		expect(scope.siteName).toEqual("Stores");
	});

	it('should go back to sites when business unit is changed', function() {
		$sessionStorage.buid = "928dd0bc-bf40-412e-b970-9b5e015aadea";
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should not go back to sites overview when business unit is not initialized yet', function() {
		$sessionStorage.buid = undefined;
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).not.toHaveBeenCalledWith('rta');
	});

	it('should convert teams out of adherence and number of agents to percent', function() {
		//$fakeBackend.withTeamAdherence({
		//		OutOfAdherence: 5
		//	})
		//	.withTeam({
		//		NumberOfAgents: 10
		//	});

		$controllerBuilder.createController();

		var result = scope.getAdherencePercent(5, 10);

		expect(result).toEqual(50);
	});
});
