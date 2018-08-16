module.exports = function(grunt) {
	grunt.option.init({ development: true });

	const isDev = grunt.option('development');
	const isProd = grunt.option('no-development');

	const watch = {
		indexTemplates: {
			files: ['index.tpl.html', 'index_desktop_client.tpl.html'],
			tasks: ['processhtml']
		},
		angularjsTemplates: {
			files: ['index.tpl.html', 'app/**/*.html', 'html/**/*.html'],
			tasks: ['ngtemplates']
		},
		angularjsCode: {
			files: ['app/**/*.js'],
			tasks: ['concat:concatJsWfm']
		},
		antThemes: {
			files: ['src/themes/*.less'],
			tasks: ['less']
		}
	};

	const less = {
		options: {
			compress: isProd,
			javascriptEnabled: true,
			relativeUrls: false
		},
		files: {
			'dist/ant_classic.css': 'src/themes/ant_classic.less',
			'dist/ant_dark.css': 'src/themes/ant_dark.less'
		}
	};

	const ngtemplates = {
		'wfm.templates': {
			src: ['html/**/*.html', 'app/**/*.html', 'app/**/html/*.html'],
			dest: 'dist/templates.js',
			options: {
				standalone: true
			}
		}
	};

	const processhtml = {
		browser: {
			files: {
				'index.html': ['index.tpl.html']
			},
			options: {
				process: true,
				environment: isDev ? 'dev' : 'prod'
			}
		},
		desktop: {
			files: {
				'index_desktop_client.html': ['index_desktop_client.tpl.html']
			},
			options: {
				process: true,
				environment: isDev ? 'dev' : 'prod'
			}
		}
	};

	const concatJsDependencies = {
		src: [
			'node_modules/interactjs/dist/interact.min.js',
			'node_modules/angular/angular.min.js',
			'node_modules/angular-ui-router/release/angular-ui-router.min.js',
			'node_modules/angular-resizable/angular-resizable.min.js',
			'node_modules/angular-resource/angular-resource.min.js',
			'node_modules/angular-sanitize/angular-sanitize.min.js',
			'node_modules/angular-translate/dist/angular-translate.min.js',
			'node_modules/angular-translate/dist/angular-translate-loader-url/angular-translate-loader-url.min.js',
			'node_modules/angular-dynamic-locale/tmhDynamicLocale.min.js',
			'node_modules/moment/min/moment-with-locales.min.js',
			'node_modules/moment-timezone/builds/moment-timezone-with-data.min.js',
			'node_modules/angular-moment/angular-moment.min.js',
			'node_modules/ng-file-upload/dist/ng-file-upload-shim.min.js',
			'node_modules/ng-file-upload/dist/ng-file-upload.min.js',
			'node_modules/angular-ui-grid/ui-grid.min.js',
			'node_modules/angular-ui-indeterminate/dist/indeterminate.min.js',
			'node_modules/ngstorage/ngStorage.min.js',
			'node_modules/default-passive-events/dist/index.js',
			'node_modules/angular-ui-tree/dist/angular-ui-tree.min.js',
			'node_modules/angular-aria/angular-aria.min.js',
			'node_modules/angular-animate/angular-animate.min.js',
			'node_modules/angular-gantt/assets/angular-gantt.js',
			'node_modules/angular-gantt/assets/angular-gantt-plugins.js',
			'node_modules/angular-gantt/assets/angular-gantt-table-plugin.js',
			'node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.js',
			'node_modules/teleopti-styleguide/styleguide/dist/wfmdirectives.min.js',
			'node_modules/teleopti-styleguide/styleguide/dist/templates.js',
			'node_modules/filesaver.js/FileSaver.min.js',
			'node_modules/jquery/dist/jquery.min.js',
			'node_modules/hammerjs/hammer.min.js',
			'node_modules/angular-material/angular-material.min.js',
			'node_modules/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js',
			'vendor/fabricjs/fabric.min.js',
			'vendor/fabricjs/fabricjs_viewport.js',
			'vendor/ng-image-input-with-preview/ng-image-input-with-preview.js',
			'node_modules/d3/d3.min.js',
			'node_modules/c3/c3.min.js',
			'node_modules/c3-angular/c3-angular.min.js',
			'vendor/ui-bootstrap-custom-build/datepicker.directive.ext.js',
			'vendor/ui-bootstrap-custom-build/timepicker.directive.ext.js',
			'node_modules/angular-dialog-service/dist/dialogs.min.js',
			'node_modules/angular-dialog-service/dist/dialogs-default-translations.min.js',
			'vendor/angular-bootstrap-persian-datepicker-master/persiandate.js',
			'vendor/angular-bootstrap-persian-datepicker-master/persian-datepicker-tpls.js',
			'../Content/signalr/jquery.signalR-2.2.2.js',
			'../Content/signalr/broker-hubs.js',
			'node_modules/lodash/lodash.min.js'
		],
		dest: 'dist/resources/modules.js'
	};

	const concatJsWfm = {
		options: {
			separator: ';' + grunt.util.linefeed,
			sourceMap: true
		},
		src: [
			'app/**/*.js',
			'!app/**/*.spec.js',
			'!app/**/*.fake.js',
			'!app/**/*.fortest.js',
			'!app/app_desktop_client.js'
		],
		dest: 'dist/main.js'
	};
	// TODO: Add desktop concat

	const concatStyleguideClassic = {
		src: ['node_modules/teleopti-styleguide/styleguide/dist/main.min.css'],
		dest: 'dist/styleguide_classic.css'
	};

	const concatStyleguideDark = {
		src: ['node_modules/teleopti-styleguide/styleguide/dist/main_dark.min.css'],
		dest: 'dist/styleguide_dark.css'
	};

	const concatCssDependencies = {
		src: [
			'node_modules/c3/c3.min.css',
			'node_modules/bootstrap/dist/css/bootstrap.css',
			'node_modules/angular-resizable/src/angular-resizable.css',
			'node_modules/angular-ui-tree/source/angular-ui-tree.css',
			'node_modules/angular-ui-grid/ui-grid.css',
			'node_modules/angular-material/angular-material.css',
			'node_modules/angular-gantt/assets/angular-gantt.css',
			'node_modules/angular-gantt/assets/angular-gantt-plugins.css',
			'node_modules/angular-gantt/assets/angular-gantt-table-plugin.css',
			'node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.css'
		],
		dest: 'dist/dependencies.css'
	};

	const uglifyOptions = {
		sourceMap: false,
		beautify: false,
		mangle: false
	};
	const uglify = {
		templates: {
			files: {
				'dist/templates.js': ['dist/templates.js']
			},
			options: uglifyOptions
		},
		browser: {
			files: {
				'dist/main.js': [
					'app/**/*.js',
					'!app/**/*.spec.js',
					'!app/**/*.fake.js',
					'!app/**/*.fortest.js',
					'!app/app_desktop_client.js'
				]
			},
			options: uglifyOptions
		},
		desktop: {
			files: {
				'dist/mainForDesktop.js': [
					'app/**/*.js',
					'!app/**/*.spec.js',
					'!app/**/*.fake.js',
					'!app/**/*.fortest.js',
					'!app/app.js'
				]
			},
			options: uglifyOptions
		}
	};

	grunt.initConfig(
		{
			watch,
			ngtemplates,
			processhtml,
			less,
			concat: {
				concatJsDependencies,
				concatJsWfm,
				concatStyleguideClassic,
				concatStyleguideDark,
				concatCssDependencies
			},
			uglify,
			msbuild: {
				rebuild: {
					src: ['../../../CruiseControl.sln'],
					options: {
						projectConfiguration: 'Debug',
						targets: ['Rebuild'],
						version: 15.0,
						maxCpuCount: null,
						buildParameters: {
							WarningLevel: 2
						},
						verbosity: 'normal'
					}
				},
				build: {
					src: ['../../../CruiseControl.sln'],
					options: {
						projectConfiguration: 'Debug',
						targets: ['build'],
						version: 15.0,
						maxCpuCount: null,
						buildParameters: {
							WarningLevel: 2
						},
						verbosity: 'normal'
					}
				},
				buildWeb: {
					src: ['../Teleopti.Ccc.Web.csproj'],
					options: {
						projectConfiguration: 'Debug',
						targets: ['build'],
						version: 15.0,
						maxCpuCount: null,
						buildParameters: {
							WarningLevel: 2
						},
						verbosity: 'normal'
					}
				}
			},

			copy: {
				sourceMaps: {
					files: [
						// includes files within path
						{
							expand: true,
							cwd: 'vendor',
							flatten: true,
							src: ['*/*.map'],
							dest: 'dist/resources',
							filter: 'isFile'
						}
					]
				},
				extras: {
					files: [
						{
							expand: true,
							cwd: 'node_modules/angular-ui-grid',
							src: ['*.ttf', '*.woff', '*.eot'],
							dest: 'dist/',
							filter: 'isFile'
						}
					]
				},
				bootstrap: {
					files: [
						{
							expand: true,
							cwd: 'node_modules/bootstrap/fonts',
							src: ['*.ttf', '*.woff', '*.eot'],
							dest: 'dist/fonts',
							filter: 'isFile'
						}
					]
				}
			}
		}
		// eslint: {
		// 	global: {
		// 		src: ['app/global/**/*.js', '!app/**/*.spec.js', '!app/**/*.fake.js', '!app/global/i18n/*.js']
		// 	},
		// 	rta: {
		// 		src: ['app/rta/**/*.js', '!app/rta/rta/rta.faketime.service.js']
		// 	},
		// 	schedule: {
		// 		src: [
		// 			'app/resourceplanner/resource_planner_agent_group/**/*.js',
		// 			'app/resourceplanner/resource_planner_day_off_rule/**/*.js',
		// 			'app/resourceplanner/resource_planner_planning_period/**/*.js',
		// 			'app/resourceplanner/resource_planner_v2/**/*.js'
		// 		]
		// 	},
		// 	dev: {
		// 		src: [
		// 			//add your path to module here
		// 			'app/permissions/refact/**/*js',
		// 			'app/staffing/**/*.js',
		// 			'app/skillPrio/**/*.js',
		// 			'app/requests/**/*.js',
		// 			'app/teamSchedule/**/*.js',
		// 			'app/rtaTool/**/*.js',
		// 			'!app/**/*.spec.js',
		// 			'!app/**/*.fake.js'
		// 		]
		// 	}
		// },
	);

	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-contrib-copy');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-contrib-less');
	grunt.loadNpmTasks('grunt-msbuild');
	grunt.loadNpmTasks('grunt-angular-templates');
	grunt.loadNpmTasks('grunt-processhtml');
	grunt.loadNpmTasks('grunt-eslint');

	grunt.registerTask('devBuild', ['concat', 'copy', 'ngtemplates', 'less', 'processhtml']);
	grunt.registerTask('devWatch', ['devBuild', 'watch']);
	grunt.registerTask('prodBuild', ['concat', 'copy', 'ngtemplates', 'less', 'processhtml', 'uglify']);

	grunt.registerTask('default', ['devWatch']);

	grunt.registerTask('build', ['msbuild:build']); // build the solution
	grunt.registerTask('buildWeb', ['msbuild:buildWeb']); // build the web project
	grunt.registerTask('rebuild', ['msbuild:rebuild']); // rebuild the solution
};
