'use strict';

var rtaTester = (function () {

	var injectTester = function (sharedTestState, tests, withABagOfCandy) {

		var makeLegacyPolyfill = function () {
			return {
				createController: function () {
					var controller = sharedTestState.createController();
					return {
						controller: controller,
						vm: controller,
						apply: sharedTestState.applyAndWait.apply,
						wait: sharedTestState.applyAndWait.wait
					}
				},

				get current() {
					return sharedTestState.$state.current;
				},
				get go() {
					return sharedTestState.$state.go;
				},

				$emit: function (a) {
					return sharedTestState.scope.$emit(a);
				},
				flush: function () {
					return sharedTestState.$interval.flush();
				},
				verifyNoOutstandingRequest: function () {
					return sharedTestState.$httpBackend.verifyNoOutstandingRequest();
				},

				// fakeBackend stuff
				withTime: function (a) {
					return sharedTestState.$fakeBackend.withTime(a);
				},
				withAgentState: function (a) {
					return sharedTestState.$fakeBackend.withAgentState(a);
				},
				withPhoneState: function (a) {
					return sharedTestState.$fakeBackend.withPhoneState(a);
				},
				withSkillGroup: function (a) {
					return sharedTestState.$fakeBackend.withSkillGroup(a);
				},
				withSkillGroups: function (a) {
					return sharedTestState.$fakeBackend.withSkillGroups(a);
				},
				clearAgentStates: function (a) {
					return sharedTestState.$fakeBackend.clearAgentStates(a);
				},
				get lastAgentStatesRequestParams() {
					return sharedTestState.$fakeBackend.lastAgentStatesRequestParams;
				},

				// stateParams stuff
				set siteIds(value) {
					sharedTestState.stateParams.siteIds = value;
				},
				set teamIds(value) {
					sharedTestState.stateParams.teamIds = value;
				},
				set skillAreaId(value) {
					sharedTestState.stateParams.skillAreaId = value;
				},
				set skillIds(value) {
					sharedTestState.stateParams.skillIds = value;
				},
				set es(value) {
					sharedTestState.stateParams.es = value;
				},
				set showAllAgents(value) {
					sharedTestState.stateParams.showAllAgents = value;
				},

				// session storage stuff
				set buid(value) {
					sharedTestState.$sessionStorage.buid = value;
				},

				// NoticeService stuff
				get info() {
					return sharedTestState.NoticeService.info;
				},
				set info(value) {
					sharedTestState.NoticeService.info = value;
				}
			};
		};

		var makeTester = function () {
			var tester = {
				createController: function (o) {
					return sharedTestState.createController(o);
				},
				destroyController: function () {
					// simulate destroy atleast...
					sharedTestState.scope.$emit('$destroy');
				},
				get stateParams() {
					return sharedTestState.stateParams;
				},
				get lastGoState() {
					return sharedTestState.lastGoState;
				},
				get lastGoParams() {
					return sharedTestState.lastGoParams;
				},
				get backend() {
					return sharedTestState.$fakeBackend;
				},
				get sessionStorage() {
					return sharedTestState.$sessionStorage;
				},
				get lastNotice() {
					return sharedTestState.lastNotice;
				},
				href: sharedTestState.$state.href,
				apply: function (a) {
					sharedTestState.applyAndWait.apply(a);
					return tester;
				},
				wait: function (m) {
					sharedTestState.applyAndWait.wait(m);
					return tester;
				},
				randomId: function () {
					return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
						var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
						return v.toString(16);
					});
				},
				randomString: function (prefix) {
					return (prefix || '') + Math.random().toString(36).substring(8);
				}
			};
			return tester;
		};

		var wit = function (s, fn) {
			it(s, function () {
				fn(makeTester());
			});
		};

		var wfit = function (s, fn) {
			fit(s, function () {
				fn(makeTester());
			});
		};

		var wxit = function (s, fn) {
			xit(s, function () {
				fn(makeTester());
			});
		};

		if (!withABagOfCandy) {
			tests(
				wit,
				wfit,
				wxit)
		} else {
			var legacyPolyfill = makeLegacyPolyfill();
			tests(
				wit,
				wfit,
				wxit,
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
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill,
				legacyPolyfill);
		}

	};

	function safeBackendFlush($httpBackend) {
		try { // the internal mock will throw if no requests were made
			$httpBackend.flush();
		} catch (e) {
			if (e.message && e.message.includes("No pending request to flush !"))
				return;
			throw e;
		}
	}

	var setup = function (tests, stateName, controllerName) {

		var state = {
			stateParams: {}
		};

		beforeEach(module('wfm.rta'));
		beforeEach(module('wfm.rtaTracer'));
		beforeEach(module('wfm.rtaTestShared'));

		beforeEach(function () {
			module(function ($provide) {
				$provide.factory('$stateParams', function () {
					state.stateParams = {};
					return state.stateParams;
				});
			});
		});

		var locale;
		beforeEach(inject(
			['$httpBackend', '$interval', '$state', '$sessionStorage', 'FakeRtaBackend', 'NoticeService', '$translate', '$controller', '$timeout', '$rootScope',
				function ($httpBackend, $interval, $state, $sessionStorage, $fakeBackend, NoticeService, $translate, $controller, $timeout, $rootScope) {
					state.NoticeService = NoticeService;
					state.$interval = $interval;
					state.$state = $state;
					state.$sessionStorage = $sessionStorage;
					state.$httpBackend = $httpBackend;
					state.$fakeBackend = $fakeBackend;
					state.$state.current.name = stateName;
					state.scope = $rootScope.$new();

					state.createController = function (o) {
						o = o || {};
						if (!angular.isDefined(o.flush))
							o.flush = true;
						var controller = $controller(controllerName, {$scope: state.scope});
						state.scope.$digest();
						if (o.flush)
							safeBackendFlush($httpBackend);
						return controller;
					};

					state.applyAndWait = {
						apply: function (apply) {
							if (angular.isFunction(apply)) {
								apply();
								state.scope.$digest();
							} else {
								state.scope.$apply(apply);
							}
							safeBackendFlush(state.$httpBackend);
							return state.applyAndWait;
						},
						wait: function (milliseconds) {
							$interval.flush(milliseconds);
							$timeout.flush(milliseconds);
							safeBackendFlush(state.$httpBackend);
							return state.applyAndWait;
						}
					};

					spyOn($state, 'go').and.callFake(function (stateName, params) {
						state.lastGoState = stateName;
						state.lastGoParams = params;
					});

					spyOn(NoticeService, 'warning').and.callFake(function (message, lifetime, destroyOnStateGo) {
						state.lastNotice = {
							Warning: message,
							Lifetime: lifetime,
							DestroyOnStateGo: destroyOnStateGo
						};
					});

					spyOn($translate, 'instant').and.callFake(function (key) {
						return state.$fakeBackend.data.translation()[key] || key;
					});

					locale = moment.locale();
                    moment.locale('en');
				}]
		));

		afterEach(function () {
			if (state.$fakeBackend)
				state.$fakeBackend.clear.all();
			if (state.$sessionStorage)
				state.$sessionStorage.$reset();
			state.lastGoParams = undefined;
			state.lastNotice = undefined;
            jasmine.clock().uninstall();
            moment.locale(locale);
		});

		var bagOfCandy = controllerName == 'RtaAgentsController78568';
		injectTester(state, tests, bagOfCandy);
	};

	function setupByDescription(description, tests) {
		if (description === 'RtaOverviewController')
			setup(tests, '', 'RtaOverviewController78568');
		if (description === 'RtaAgentsController')
			setup(tests, '', 'RtaAgentsController78568');
		if (description === 'RtaHistoricalController')
			setup(tests, 'rta-historical', 'RtaHistoricalController77045');
		if (description === 'RtaTracerController')
			setup(tests, '', 'RtaTracerController');
		if (description === 'RtaHistoricalOverviewController')
			setup(tests, '', 'RtaHistoricalOverviewController80594');		
		if (description === 'AdjustAdherenceController')
			setup(tests, '', 'AdjustAdherenceController');
		
	}

	return {
		describe: function (description, tests) {
			return describe(description, function () {
				setupByDescription(description, tests);
			});
		},
		fdescribe: function (description, tests) {
			return fdescribe(description, function () {
				setupByDescription(description, tests);
			})
		},
		xdescribe: xdescribe
	}

})();

