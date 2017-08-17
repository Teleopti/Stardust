describe('requests tests', function() {
	'use strict';
	var $rootScope,
		$controller,
		$q,
		toggleObject = {
			Wfm_Requests_Refactoring_45470: false
		},
		fakeState = {
			current: {
				name: ''
			},
			go: function(name) {
				this.current.name = name;
			}
		};

	beforeEach(function() {
		module('wfm.requests');

		module(function($provide) {
			$provide.service('$state', function(){
				return fakeState;
			});
			$provide.service('Toggle', function() {
				toggleObject.togglesLoaded = {
					then: function(cb) {
						cb && cb();
					}
				};
				return toggleObject;
			});
		});
	});

	beforeEach(inject(function(_$rootScope_, _$controller_) {
		$rootScope = _$rootScope_;
		$controller = _$controller_;
	}));

	it('should redirect to requestsRefactor page when toggle Wfm_Requests_Refactoring_45470 is on', function() {
		toggleObject.Wfm_Requests_Refactoring_45470 = true;

		var target = setupTarget();
		var controller = target.controller;

		expect(fakeState.current.name).toEqual('requestsRefactor');
	});

	it('should redirect to requestsOrigin page when toggle Wfm_Requests_Refactoring_45470 is off', function() {
		toggleObject.Wfm_Requests_Refactoring_45470 = false;

		var target = setupTarget();
		var controller = target.controller;

		expect(fakeState.current.name).toEqual('requestsOrigin');
	});

	function setupTarget() {
		var scope = $rootScope.$new();
		var controller = $controller('RequestsController');

		return {
			controller: controller,
			scope: scope
		};
	}
});