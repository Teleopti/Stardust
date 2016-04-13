(function() {
	'use strict';

	describe('ThemeService', function() {
		var $httpBackend,
			deferred, scope;

		beforeEach(function() {
			module('wfm.themes');
		});

		beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
			$httpBackend = _$httpBackend_;
			deferred = _$q_.defer();
			scope = _$rootScope_.$new();
		}));

		var setUpTemplate = function() {
			var themeModules = document.createElement("link");
			themeModules.setAttribute("id", "themeModules");
			themeModules.setAttribute("href", "");
			document.head.appendChild(themeModules);
			var themeStylesheet = document.createElement("link");
			themeStylesheet.setAttribute("id", "themeStylesheet");
			themeStylesheet.setAttribute("href", "");
			document.head.appendChild(themeStylesheet);
		};
		var teardownTemplate = function() {
			document.getElementById('themeModules').remove();
			document.getElementById('themeStylesheet').remove();
		};

		it('should set theme', inject(function(ThemeService) {
			setUpTemplate();

			ThemeService.setTheme("dark");

			expect(document.getElementById('themeModules').getAttribute('href')).toBe('dist/modules_dark.min.css');
			expect(document.getElementById('themeStylesheet').getAttribute('href')).toBe('dist/style_dark.min.css');

			teardownTemplate();
		}));

		it('should get theme', function(done) {
			inject(function(ThemeService) {
				$httpBackend.expectGET("../api/Theme")
					.respond(200, {
						Name: "light"
					});

				ThemeService.getTheme().success(function(result) {
					expect(result.Name).toBe("light");
					done();
				});

				$httpBackend.flush();
			})
		});

		it('should init theme', inject(function(ThemeService) {
			$httpBackend.expectGET("../api/Theme")
				.respond(200, {
					Name: "light"
				});
			setUpTemplate();

			ThemeService.init()

			$httpBackend.flush();
			expect(document.getElementById('themeModules').getAttribute('href')).toBe('dist/modules_light.min.css');
			expect(document.getElementById('themeStylesheet').getAttribute('href')).toBe('dist/style_light.min.css');
			teardownTemplate();
		}));
	});
})();
