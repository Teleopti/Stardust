'use strict';

describe('RtaTracerController', function () {
	var
		$controllerBuilder,
		$fakeBackend,
		$httpBackend;

	var
		stateParams,
		scope;
	
	beforeEach(module('wfm.rtaTracer'));
	beforeEach(module('wfm.rtaTestShared'));
	
	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_RtaTracerBackendFake_, _ControllerBuilder_) {
		$controllerBuilder = _ControllerBuilder_;
		$fakeBackend = _RtaTracerBackendFake_;

		scope = $controllerBuilder.setup('RtaTracerController');

		$fakeBackend.clear();
	}));

	it('should be able to input user code', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		
		expect(vm.userCode).toBe('');
	});

	it('should start tracing user code', function () {
		var c = $controllerBuilder.createController();
		var vm = c.vm;
		
		c.apply(function (){
			vm.userCode = 'userCode2';
		});
		c.apply(function (){
			vm.trace();
		});

		expect($fakeBackend.traceCalledForUserCode).toBe('userCode2');
	});

});