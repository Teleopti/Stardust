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

		var setUpTemplate = function(option) {
			if (option === "withHash") {
				var themeModules = document.createElement("link");
				themeModules.setAttribute("id", "themeModules");
				themeModules.setAttribute("href", "orig?123");
				document.head.appendChild(themeModules);
				var themeStyle = document.createElement("link");
				themeStyle.setAttribute("id", "themeStyle");
				themeStyle.setAttribute("href", "orig?123");
				document.head.appendChild(themeStyle);
			}
			else {
				var themeModules = document.createElement("link");
				themeModules.setAttribute("id", "themeModules");
				themeModules.setAttribute("href", "");
				document.head.appendChild(themeModules);
				var themeStyle = document.createElement("link");
				themeStyle.setAttribute("id", "themeStyle");
				themeStyle.setAttribute("href", "");
				document.head.appendChild(themeStyle);
			}

			var checkBox = document.createElement("input");
			checkBox.setAttribute("id","darkTheme");
			document.body.appendChild(checkBox);
		};
		var teardownTemplate = function() {
			document.getElementById('themeModules').remove();
			document.getElementById('themeStyle').remove();
		};

		it('should set theme', inject(function(ThemeService) {
			setUpTemplate();

			ThemeService.setTheme("dark");

			expect(document.getElementById('themeModules').getAttribute('href')).toBe('dist/modules_dark.min.css');
			expect(document.getElementById('themeStyle').getAttribute('href')).toBe('dist/style_dark.min.css');

			teardownTemplate();
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

		it('should init theme', inject(function (ThemeService) {
			$httpBackend.expectGET("../ToggleHandler/AllToggles")
					.respond(200, { WfmGlobalLayout_personalOptions_37114: true });

			$httpBackend.expectGET("../api/Theme")
				.respond(200, {
					Name: "light",
					Overlay: true
				});
		 	setUpTemplate();

			ThemeService.init();

			$httpBackend.flush();
			expect(document.getElementById('themeModules').getAttribute('href')).toBe('dist/modules_light.min.css');
			expect(document.getElementById('themeStyle').getAttribute('href')).toBe('dist/style_light.min.css');
			teardownTemplate();
		}));

		it('should init classic theme if the feature is not available', inject(function (ThemeService) {
			$httpBackend.expectGET("../ToggleHandler/AllToggles")
					.respond(200, { WfmGlobalLayout_personalOptions_37114: false });

			
			setUpTemplate();

			ThemeService.init();

			$httpBackend.flush();
			expect(document.getElementById('themeModules').getAttribute('href')).toBe('dist/modules_classic.min.css');
			expect(document.getElementById('themeStyle').getAttribute('href')).toBe('dist/style_classic.min.css');
			teardownTemplate();
		}));

		it('should persist hash', inject(function (ThemeService) {
			$httpBackend.expectGET("../ToggleHandler/AllToggles")
					.respond(200, { WfmGlobalLayout_personalOptions_37114: true });
			$httpBackend.expectGET("../api/Theme")
				.respond(200, {
					Name: "light",
					Overlay: true
				});
			setUpTemplate("withHash");

			ThemeService.init();

			$httpBackend.flush();
			expect(document.getElementById('themeModules').getAttribute('href')).toBe('dist/modules_light.min.css?123');
			expect(document.getElementById('themeStyle').getAttribute('href')).toBe('dist/style_light.min.css?123');
			teardownTemplate();
		}));

	});
})();
