'use strict';

var rtaTester = (function () {

	var describer = function (tests) {

		var $interval,
			$httpBackend,
			$state,
			$sessionStorage,
			$fakeBackend,
			$controllerBuilder,
			NoticeService,
			vm,
			scope,
			controller;

		var stateParams = {};

		beforeEach(module('wfm.rta'));
		beforeEach(module('wfm.rtaTestShared'));

		beforeEach(function () {
			module(function ($provide) {
				$provide.factory('$stateParams', function () {
					stateParams = {};
					return stateParams;
				});
			});
		});

		beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_, _NoticeService_) {
			$interval = _$interval_;
			$state = _$state_;
			$sessionStorage = _$sessionStorage_;
			$httpBackend = _$httpBackend_;
			$fakeBackend = _FakeRtaBackend_;
			$controllerBuilder = _ControllerBuilder_;
			NoticeService = _NoticeService_;
			scope = $controllerBuilder.setup('RtaAgentsController46758');
			spyOn($state, 'go');
			// spyOn(NoticeService, 'info');
		}));

		afterEach(function () {
			$fakeBackend.clear();
			$sessionStorage.$reset();
		});

		var legacyPolyfill = {
			createController: function () {
				return $controllerBuilder.createController();
			},

			get current() {
				return $state.current;
			},
			get go() {
				return $state.go;
			},

			$emit: function (a) {
				return scope.$emit(a);
			},
			flush: function () {
				return $interval.flush();
			},
			verifyNoOutstandingRequest: function () {
				return $httpBackend.verifyNoOutstandingRequest();
			},

			// fakeBackend stuff
			withTime: function (a) {
				return $fakeBackend.withTime(a);
			},
			withAgentState: function (a) {
				return $fakeBackend.withAgentState(a);
			},
			withPhoneState: function (a) {
				return $fakeBackend.withPhoneState(a);
			},
			withSkillAreas: function (a) {
				return $fakeBackend.withSkillAreas(a);
			},
			clearAgentStates: function (a) {
				return $fakeBackend.clearAgentStates(a);
			},
			get lastAgentStatesRequestParams() {
				return $fakeBackend.lastAgentStatesRequestParams;
			},

			// stateParams stuff
			set siteIds(value) {
				stateParams.siteIds = value;
			},
			set teamIds(value) {
				stateParams.teamIds = value;
			},
			set skillAreaId(value) {
				stateParams.skillAreaId = value;
			},
			set skillIds(value) {
				stateParams.skillIds = value;
			},
			set es(value) {
				stateParams.es = value;
			},
			set showAllAgents(value) {
				stateParams.showAllAgents = value;
			},

			// session storage stuff
			set buid(value) {
				$sessionStorage.buid = value;
			},

			// NoticeService stuff
			get info() {
				return NoticeService.info;
			},
			set info(value) {
				NoticeService.info = value;
			},
		};

		var tester = {
			createController: function () {
				return controller = $controllerBuilder.createController();
			},
			backend: function () {
				return $fakeBackend;
			},
			get vm() {
				return controller.vm;
			},
			apply: function (a) {
				return controller.apply(a)
			},
			wait: function (a) {
				return controller.wait(a)
			}
		};

		var wit = function (s, fn) {
			it(s, function () {
				fn(tester);
			});
		};

		var wfit = function (s, fn) {
			fit(s, function () {
				fn(tester);
			});
		};

		var wxit = function (s, fn) {
			xit(s, function () {
				fn(tester);
			});
		};

		tests(
			wit,
			wfit,
			wxit,
			tester,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill,
			legacyPolyfill);

	};

	return {
		describe: function (description, tests) {
			describe(description, function () {
				describer(tests);
			})
		},
		fdescribe: function (description, tests) {
			fdescribe(description, function () {
				describer(tests);
			})
		}
	}

})();

