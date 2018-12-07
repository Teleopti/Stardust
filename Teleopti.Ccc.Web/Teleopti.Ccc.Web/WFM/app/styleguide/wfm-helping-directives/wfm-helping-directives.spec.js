describe('wfm helping directives', function() {
	var scope, $rootScope, $compile;

	beforeEach(function() {
		module('wfm.helpingDirectives');
	});

	beforeEach(inject(function(_$rootScope_, _$compile_) {
		$rootScope = _$rootScope_;
		$compile = _$compile_;
	}));

	it('should execute binding outside-click function when clicking outside', function() {
		var outsideClickFnExecuted = false;
		scope = $rootScope.$new();
		scope.outsideClickFn = function() {
			outsideClickFnExecuted = true;
		};

		$compile('<div outside-click="outsideClickFn()">This is a customized element</div>')(scope);

		var backdropElement = document.getElementsByClassName('outside-click-backdrop');
		expect(backdropElement.length).toEqual(1);

		backdropElement[0].dispatchEvent(
			new MouseEvent('click', {
				bubbles: true,
				cancelable: true,
				view: window
			})
		);
		scope.$apply();

		expect(outsideClickFnExecuted).toEqual(true);
	});
});
