(function() {
	'use strict';

	describe('ThemeService', function() {
		var $httpBackend,
			$rootScope;

		beforeEach(function() {
			module('wfm.themes');
		});

		beforeEach(inject(function(_$httpBackend_, _$rootScope_) {
			$httpBackend = _$httpBackend_;
			$rootScope = _$rootScope_.$new();
	
		}));

		it('should get theme', function(done) {
			inject(function (ThemeService) {
				$httpBackend.expectGET("../ToggleHandler/AllToggles")
					.respond(200, { WfmGlobalLayout_personalOptions_37114: true });

				$httpBackend.expectGET("../api/Theme")
					.respond(200, {
						Name: "light",
						Overlay: true
					});

				ThemeService.getTheme().success(function(result) {
					expect(result.Name).toBe("light");
					expect(result.Overlay).toBe(true);
					done();
				});

				$httpBackend.flush();
			});
		});


	});
})();
