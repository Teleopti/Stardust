'use strict';
describe('RtaAgentsCtrl for sites', function () {
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

		spyOn($state, 'go');
	}));
	[{
		name: "multiple sites",
		type: 'siteIds',
		ids: ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"],
		createAgent: function (personId, siteId) {
			return {
				PersonId: personId,
				SiteId: siteId
			}
		}
	}, {
			name: "multiple teams",
			type: 'teamIds',
			ids: ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"],
			createAgent: function (personId, teamId) {
				return {
					PersonId: personId,
					TeamId: teamId
				}
			}
		}
	].forEach(function (selection) {

		
		it('should get agents for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgent(
				selection.createAgent("11610fe4-0130-4568-97de-9b5e015b2564", selection.ids[0])
				)
				.withAgent(
				selection.createAgent("6b693b41-e2ca-4ef0-af0b-9e06008d969b", selection.ids[1])
				);

			$controllerBuilder.createController();

			expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
			expect(scope.agents[1].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
		});

		it('should get agent states for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgent(
				selection.createAgent("11610fe4-0130-4568-97de-9b5e015b2564", selection.ids[0])
				)
				.withAgent(
				selection.createAgent("6b693b41-e2ca-4ef0-af0b-9e06008d969b", selection.ids[1])
				)
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

		it('should update agent states for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgent(
				selection.createAgent("11610fe4-0130-4568-97de-9b5e015b2564", selection.ids[0])
				)
				.withAgent(
				selection.createAgent("6b693b41-e2ca-4ef0-af0b-9e06008d969b", selection.ids[1])
				)
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
				.apply(function () {
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

		it('should set states to agents for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgent(
				selection.createAgent("11610fe4-0130-4568-97de-9b5e015b2564", selection.ids[0])
				)
				.withAgent(
				selection.createAgent("6b693b41-e2ca-4ef0-af0b-9e06008d969b", selection.ids[1])
				)
				.withState({
					PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
					State: "Ready",
					Activity: "Phone",
					NextActivity: "Short break",
					NextActivityStartTime: "\/Date(1432109700000)\/",
					Alarm: "In Adherence",
					Color: "#00FF00",
					TimeInState: 15473
				})
				.withState({
					PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
					State: "In Call",
					Activity: "Short break",
					NextActivity: "Phone",
					NextActivityStartTime: "\/Date(1432109700000)\/",
					Alarm: "Out of Adherence",
					Color: "#FF0000",
					TimeInState: 15473
				});

			$controllerBuilder.createController()
				.apply("agentsInAlarm = false");

			expect(scope.agents[0].State).toEqual("Ready");
			expect(scope.agents[0].Activity).toEqual("Phone");
			expect(scope.agents[0].NextActivity).toEqual("Short break");
			expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
			expect(scope.agents[0].Alarm).toEqual("In Adherence");
			expect(scope.agents[0].Color).toEqual("#00FF00");
			expect(scope.agents[0].TimeInState).toEqual(15473);

			expect(scope.agents[1].State).toEqual("In Call");
			expect(scope.agents[1].Activity).toEqual("Short break");
			expect(scope.agents[1].NextActivity).toEqual("Phone");
			expect(scope.agents[1].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
			expect(scope.agents[1].Alarm).toEqual("Out of Adherence");
			expect(scope.agents[1].Color).toEqual("#FF0000");
			expect(scope.agents[1].TimeInState).toEqual(15473);
		});

		it('should stop polling when page is about to destroy for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgent(
				selection.createAgent("11610fe4-0130-4568-97de-9b5e015b2564", selection.ids[0])
				)
				.withAgent(
				selection.createAgent("6b693b41-e2ca-4ef0-af0b-9e06008d969b", selection.ids[1])
				);

			$controllerBuilder.createController();

			scope.$emit('$destroy');
			$interval.flush(5000);
			$httpBackend.verifyNoOutstandingRequest();
		});

		it('should get agents for single' + selection.type, function () {
			stateParams[selection.type] = selection.ids[0];
			$fakeBackend
				.withAgent(
				selection.createAgent("6b693b41-e2ca-4ef0-af0b-9e06008d969b", selection.ids[0])
				);

			$controllerBuilder.createController();

			expect(scope.agents[0].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
		});

		it('should go back to sites when business unit is changed', function () {
			stateParams[selection.type] = selection.ids
			$sessionStorage.buid = "928dd0bc-bf40-412e-b970-9b5e015aadea";

			$controllerBuilder.createController()
				.apply(function () {
					$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
				});

			expect($state.go).toHaveBeenCalledWith('rta');
		});

		it('should not go back to sites overview when business unit is not initialized yet', function () {
			stateParams[selection.type] = selection.ids
			$sessionStorage.buid = undefined;

			$controllerBuilder.createController()
				.apply(function () {
					$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
				});

			expect($state.go).not.toHaveBeenCalledWith('rta');
		});

	});

});
