'use strict';
describe('RtaAgentsCtrl for sites', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder;

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

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

		$fakeBackend.clear();

	}));

	it('should get agents for multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			})
			.withAgent({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
			});

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
		expect(scope.agents[1].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
	});

	it('should get agents for single selected site', function() {
		stateParams.siteIds = ["6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		$fakeBackend.withAgent({
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
		});

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
	});

	it('should get agent states for multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withAgent({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564"
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b"
			});

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
		expect(scope.agents[1].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
	});

	it('should update agent states for multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];

		$fakeBackend
			.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withAgent({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "In Call"
			});

		

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply(function() {
				$fakeBackend.clearStates()
					.withState({
						PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
						State: "In Call"
					})
					.withState({
						PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
						State: "Ready"
					});
				
		})
		.wait(5000);
	

		expect(scope.agents[0].State).toEqual("In Call");
		expect(scope.agents[1].State).toEqual("Ready");
	});

	it('should set states to agents for multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withAgent({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready",
				StateStartTime: "\/Date(1429254905000)\/",
				Activity: "Phone",
				NextActivity: "Short break",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Alarm: "In Adherence",
				AlarmStart: "\/Date(1432105910000)\/",
				Color: "#00FF00",
				TimeInState: 15473
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "In Call",
				StateStartTime: "\/Date(1429254905000)\/",
				Activity: "Short break",
				NextActivity: "Phone",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Alarm: "Out of Adherence",
				AlarmStart: "\/Date(1432105910000)\/",
				Color: "#FF0000",
				TimeInState: 15473
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false");

		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].StateStartTime).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[0].Color).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);

		expect(scope.agents[1].State).toEqual("In Call");
		expect(scope.agents[1].StateStartTime).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[1].Activity).toEqual("Short break");
		expect(scope.agents[1].NextActivity).toEqual("Phone");
		expect(scope.agents[1].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[1].Alarm).toEqual("Out of Adherence");
		expect(scope.agents[1].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[1].Color).toEqual("#FF0000");
		expect(scope.agents[1].TimeInState).toEqual(15473);
	});

	it('should filter agent name with agentFilter', function() {
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

	it('should stop polling when page is about to destroy', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
			})
			.withAgent({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
			});

		$controllerBuilder.createController();

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

});
