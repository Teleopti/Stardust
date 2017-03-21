'use strict';
describe('RtaAgentsController', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder,
		vm;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
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

		scope = $controllerBuilder.setup('RtaAgentsController');

		$fakeBackend.clear();
		$fakeBackend.withToggle('RTA_FasterAgentsView_42039');
		spyOn($state, 'go');
	}));
	[{
		name: "multiple sites",
		type: 'siteIds',
		ids: ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"],
		createAgent: function (personId, siteId, state, timeInAlarm) {
			return {
				PersonId: personId,
				SiteId: siteId,
				State: state,
				TimeInAlarm: timeInAlarm
			}
		},
		createAgent2: function (obj,siteId) {
			obj.SiteId = siteId;
			return obj;
		}
	}, {
		name: "multiple teams",
		type: 'teamIds',
		ids: ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"],
		createAgent: function (personId, teamId, state, timeInAlarm) {
			return {
				PersonId: personId,
				TeamId: teamId,
				State: state,
				TimeInAlarm: timeInAlarm
			}
		},
		createAgent2: function (obj,teamId) {
			obj.TeamId = teamId;
			return obj;
		}
	}].forEach(function (selection) {


		it('should get agent states for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgentState(
				selection.createAgent("AshleyGuid", selection.ids[0], "Break", 30)
				)
				.withAgentState(
				selection.createAgent("JohnGuid", selection.ids[1], "Ready", 30)
				);

			vm = $controllerBuilder.createController().vm;

			expect(vm.agents[0].PersonId).toEqual("AshleyGuid");
			expect(vm.agents[1].PersonId).toEqual("JohnGuid");
		});

		it('should update agent states for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgentState(
				selection.createAgent("AshleyGuid", selection.ids[0], "Ready", 30)
				)
				.withAgentState(
				selection.createAgent("JohnGuid", selection.ids[1], "In Call", 30)
				);

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.agentsInAlarm = false)
				.apply(function () {
					$fakeBackend.clearAgentStates()
						.withAgentState(
						selection.createAgent("AshleyGuid", selection.ids[0], "In Call", 30)
						)
						.withAgentState(
						selection.createAgent("JohnGuid", selection.ids[1], "Ready", 30)
						);

				})
				.wait(5000);

			expect(vm.agents[0].State).toEqual("In Call");
			expect(vm.agents[1].State).toEqual("Ready");
		});

		it('should set states to agents for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgentState(selection.createAgent2({
					PersonId: "AshleyGuid",
					State: "Ready",
					Activity: "Phone",
					NextActivity: "Short break",
					NextActivityStartTime: "\/Date(1432109700000)\/",
					Rule: "In Adherence",
					Color: "#00FF00",
					TimeInState: 15473
				},selection.ids[0]))
				.withAgentState(selection.createAgent2({
					PersonId: "JohnGuid",
					State: "In Call",
					Activity: "Short break",
					NextActivity: "Phone",
					NextActivityStartTime: "\/Date(1432109700000)\/",
					Rule: "Out of Adherence",
					Color: "#FF0000",
					TimeInState: 15473
				},selection.ids[1]));

			var c = $controllerBuilder.createController();
			vm = c.vm;
			c.apply(vm.agentsInAlarm = false);

			expect(vm.agents[0].State).toEqual("Ready");
			expect(vm.agents[0].Activity).toEqual("Phone");
			expect(vm.agents[0].NextActivity).toEqual("Short break");
			expect(vm.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
			expect(vm.agents[0].Rule).toEqual("In Adherence");
			expect(vm.agents[0].Color).toEqual("#00FF00");
			expect(vm.agents[0].TimeInState).toEqual(15473);

			expect(vm.agents[1].State).toEqual("In Call");
			expect(vm.agents[1].Activity).toEqual("Short break");
			expect(vm.agents[1].NextActivity).toEqual("Phone");
			expect(vm.agents[1].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
			expect(vm.agents[1].Rule).toEqual("Out of Adherence");
			expect(vm.agents[1].Color).toEqual("#FF0000");
			expect(vm.agents[1].TimeInState).toEqual(15473);
		});

		it('should stop polling when page is about to destroy for ' + selection.name, function () {
			stateParams[selection.type] = selection.ids
			$fakeBackend
				.withAgentState(
				selection.createAgent("11610fe4-0130-4568-97de-9b5e015b2564", selection.ids[0],"Ready", 30)
				)
				.withAgentState(
				selection.createAgent("6b693b41-e2ca-4ef0-af0b-9e06008d969b", selection.ids[1],"Ready", 30)
				);

			vm = $controllerBuilder.createController().vm;

			scope.$emit('$destroy');
			$interval.flush(5000);
			$httpBackend.verifyNoOutstandingRequest();
		});

		it('should get agents for single' + selection.type, function () {
			stateParams[selection.type] = selection.ids[0];
			$fakeBackend
				.withAgentState(
				selection.createAgent("6b693b41-e2ca-4ef0-af0b-9e06008d969b", selection.ids[0],"Ready", 30)
				);

			vm = $controllerBuilder.createController().vm;

			expect(vm.agents[0].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
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
