'use strict';
describe('RtaAgentsCtrl', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
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

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

		$fakeBackend.clear();

		spyOn($state, 'go');
	}));

	it('should filter agent name', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		})
		.withAgent({
			Name: "Charley Caper",
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "Charley"');

		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
	});

	it('should filter agent on state', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "In Call"
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "In Call"');

		expect(scope.filteredData[0].State).toEqual("In Call");
		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
	});

	it('should filter agent on site name', function () {
		stateParams.siteIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "84590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TeamId: "45612a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "London",
				TeamName: "Green"
			})

			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "84590a63-6331-4921-bc9f-9b5e015ab495",
				TeamId: "85321a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "Paris",
				TeamName: "Red"
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "Paris"');
		expect(scope.filteredData[0].SiteAndTeamName).toEqual("Paris/Red");
		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
	});


	it('should filter agent on team name', function () {
		stateParams.teamIds = ["45612a63-6331-4921-bc9f-9b5e015ab495", "85321a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TeamId: "45612a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "London",
				TeamName: "Green"
			})

			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "84590a63-6331-4921-bc9f-9b5e015ab495",
				TeamId: "85321a63-6331-4921-bc9f-9b5e015ab495",
				SiteName: "Paris",
				TeamName: "Red"
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "Red"');
		expect(scope.filteredData[0].SiteAndTeamName).toEqual("Paris/Red");
		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
	});

	it('should filter agent on activity', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
				
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Activity: "Phone"
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				Activity: "Lunch"
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "Lunch"');

		expect(scope.filteredData[0].Activity).toEqual("Lunch");
		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
	});

	it('should filter agent on alarm', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend
			.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				Alarm: "Positive"
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				Alarm: "Adhering"
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "Adhering"');

		expect(scope.filteredData[0].Alarm).toEqual("Adhering");
		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
	});

	it('should filter agent state updates', function () {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});
		$fakeBackend.withState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "In Call"
		});

		var c = $controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "Ashley"');
		$fakeBackend
			.clearStates()
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			});
		c.wait(5000);

		expect(scope.filteredData[0].State).toEqual("Ready");
		expect(scope.filteredData[0].Name).toEqual("Ashley Andeen");
		
	});

	it('should filter agent name', function () {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			Name: "Ashley Andeen",
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
		})
			.withAgent({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				Name: "Charlie Caper",
				SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply("filterText = 'Ashley'");

		expect(scope.filteredData[0].Name).toEqual("Ashley Andeen");
	});

	it('should filter agent state updates', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		$fakeBackend.withAgent({
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "In Call",
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply("filterText = 'Caper'")
			.apply(function () {
				$fakeBackend.clearStates()
					.withState({
						PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
						State: "Ready",
					});
			})
			.wait(5000);

		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
		expect(scope.filteredData[0].State).toEqual("Ready");
	});
});
