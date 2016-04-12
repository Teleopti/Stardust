(function() {

	'use strict';
	fdescribe('themeController', function() {
		var $rootScope,
			$httpBackend,
			$controller,
			scope;

		beforeEach(module('wfm.themes'));

		beforeEach(inject(function(_$httpBackend_, _$rootScope_, _$interval_, _$controller_) {
			$controller = _$controller_;
			scope = _$rootScope_.$new();
			$rootScope = _$rootScope_;
			$httpBackend = _$httpBackend_;

			$httpBackend.whenGET("../ToggleHandler/AllToggles")
				.respond(200, {
					WfmGlobalLayout_personalOptions_37114: true
				});
		}));

		var createController = function() {
			$controller('themeController', {
				$scope: scope
			});
			scope.$digest();
			$httpBackend.flush();
		};

		it('should save theme', inject(function(ThemeService) {
			createController();
   $httpBackend.expectPOST('../api/Theme', 'dark').respond(200, '');

   ThemeService.saveTheme("dark");

			$httpBackend.flush();
		}));
	});
})();
