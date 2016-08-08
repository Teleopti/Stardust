module.exports = function(grunt) {
	// Project configuration.
	grunt.initConfig({
		watch: {
			dev: {
				files: ['css/*.scss', 'index.tpl.html', 'js/**/*.html', 'html/**/*.html', 'js/**/*.js'],
				tasks: ['devDist'],
				options: {
					spawn: false,
				}
			}
		},
		iisexpress: {
			options: {
				config: '../../../.vs/config/applicationhost.config'
			},
			authBridge: {
				options: {
					site: 'teleopti.ccc.web.authenticationBridge'
				}
			},
			web: {
				options: {
					site: 'teleopti.ccc.web',
					openUrl: 'http://localhost:52858',
					open: true,
					verbose: true
				}
			}

		},
		msbuild: {
			rebuild: {
				src: ['../../../CruiseControl.sln'],
				options: {
					projectConfiguration: 'Debug',
					targets: ['Rebuild'],
					version: 14.0,
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
					version: 14.0,
					maxCpuCount: null,
					buildParameters: {
						WarningLevel: 2
					},
					verbosity: 'normal'
				}
			}
		},
		karma: {
			options: {
				configFile: 'karma.conf.js',
			},
			unit: {},
			dev: {
				singleRun: false
			},
			continuous: {
				reporters: 'teamcity'
			}
		},

		sass: {
			options: {
				includePaths: ['node_modules/teleopti-styleguide/css']
			},
			dist: {
				files: {
					'css/style_classic.css': ['css/style.scss'],
					'css/style_dark.css': ['css/darkstyle.scss']
				}
			}
		},

		cssmin: {
			options: {
				shorthandCompacting: false,
				roundingPrecision: -1
			},
			target: {
				files: {
					'dist/style_classic.min.css': ['css/style_classic.css'],
					'dist/style_dark.min.css': ['css/style_dark.css'],
					'dist/modules_classic.min.css': ['dist/modules_classic.css'],
					'dist/modules_dark.min.css': ['dist/modules_dark.css']
				}
			}
		},
		concat: {
			distJs: {
				src: [
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
					'node_modules/angular-ui-tree/dist/angular-ui-tree.min.js',
					'node_modules/angular-aria/angular-aria.min.js',
					'node_modules/angular-animate/angular-animate.min.js',
					'node_modules/angular-gantt/assets/angular-gantt.js',
					'node_modules/angular-gantt/assets/angular-gantt-plugins.js',
					'node_modules/angular-gantt/assets/angular-gantt-table-plugin.js',
					'node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.js',
					'node_modules/teleopti-styleguide/dist/wfmdirectives.min.js',
					'node_modules/teleopti-styleguide/dist/templates.js',
					'node_modules/filesaver.js/FileSaver.min.js',
					'node_modules/jquery/dist/jquery.min.js',
					'vendor/hammerjs/hammer.min.js',
					'node_modules/angular-material/angular-material.min.js',
					'node_modules/angular-ui-bootstrap/ui-bootstrap-tpls.min.js',
					'vendor/fabricjs/fabric.min.js',
					'vendor/fabricjs/fabricjs_viewport.js',
					'vendor/ng-image-input-with-preview/ng-image-input-with-preview.js',
					'vendor/d3/d3.min.js',
					'vendor/c3/c3.min.js',
					'vendor/c3/c3-angular.min.js',
					'vendor/ui-bootstrap-custom-build/datepicker.directive.ext.js',
					'vendor/ui-bootstrap-custom-build/timepicker.directive.ext.js',
					'node_modules/angular-dialog-service/dist/dialogs.min.js',
					'node_modules/angular-dialog-service/dist/dialogs-default-translations.min.js',
					'vendor/uigrid.directive.ext.js',
					'../Content/signalr/jquery.signalR-2.2.0.js',
					'../Content/signalr/broker-hubs.js'
				],
				dest: 'dist/modules.js'
			},
			distJsForDesktop: {
				src: [
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
					'node_modules/angular-ui-tree/dist/angular-ui-tree.min.js',
					'node_modules/angular-aria/angular-aria.min.js',
					'node_modules/angular-animate/angular-animate.min.js',
					'node_modules/angular-gantt/assets/angular-gantt.js',
					'node_modules/angular-gantt/assets/angular-gantt-plugins.js',
					'node_modules/angular-gantt/assets/angular-gantt-table-plugin.js',
					'node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.js',
					'node_modules/teleopti-styleguide/dist/wfmdirectives.min.js',
					'node_modules/teleopti-styleguide/dist/templates.js',
					'node_modules/filesaver.js/FileSaver.min.js',
					'node_modules/jquery/dist/jquery.min.js',
					'vendor/hammerjs/hammer.min.js',
					'node_modules/angular-material/angular-material.min.js',
					'node_modules/angular-ui-bootstrap/ui-bootstrap-tpls.min.js',
					'vendor/fabricjs/fabric.min.js',
					'vendor/fabricjs/fabricjs_viewport.js',
					'vendor/ng-image-input-with-preview/ng-image-input-with-preview.js',
					'vendor/d3/d3.min.js',
					'vendor/c3/c3.min.js',
					'vendor/c3/c3-angular.min.js',
					'vendor/ui-bootstrap-custom-build/datepicker.directive.ext.js',
					'vendor/ui-bootstrap-custom-build/timepicker.directive.ext.js',
					'node_modules/angular-dialog-service/dist/dialogs.min.js',
					'node_modules/angular-dialog-service/dist/dialogs-default-translations.min.js',
					'vendor/uigrid.directive.ext.js',
					'../Content/signalr/jquery.signalR-2.2.0.js',
					'../Content/signalr/broker-hubs.js'
				],
				dest: 'dist/modulesForDesktop.js'
			},
			distCss: {
				src: [
					'node_modules/bootstrap/dist/css/bootstrap.min.css',
					'node_modules/angular-resizable/angular-resizable.min.css',
					'node_modules/angular-ui-tree/dist/angular-ui-tree.min.css',
					'node_modules/angular-ui-grid/ui-grid.min.css',
					'node_modules/angular-material/angular-material.min.css',
					'node_modules/angular-gantt/assets/angular-gantt.css',
					'node_modules/angular-gantt/assets/angular-gantt-plugins.css',
					'node_modules/angular-gantt/assets/angular-gantt-table-plugin.css',
					'node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.css',
					'vendor/c3/c3.min.css',
					'node_modules/teleopti-styleguide/css/main.min.css'
				],
				dest: 'dist/modules_classic.css'
			},
			distDarkCss: {
				src: [
					'node_modules/bootstrap/dist/css/bootstrap.min.css',
					'node_modules/angular-resizable/angular-resizable.min.css',
					'node_modules/angular-ui-tree/dist/angular-ui-tree.min.css',
					'node_modules/angular-ui-grid/ui-grid.min.css',
					'node_modules/angular-material/angular-material.min.css',
					'node_modules/angular-gantt/assets/angular-gantt.css',
					'node_modules/angular-gantt/assets/angular-gantt-plugins.css',
					'node_modules/angular-gantt/assets/angular-gantt-table-plugin.css',
					'node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.css',
					'vendor/c3/c3.min.css',
					'node_modules/teleopti-styleguide/css/main_dark.min.css'
				],
				dest: 'dist/modules_dark.css'
			},
		},

		uglify: {
			dist: {
				files: {
					'dist/main.min.js': ['js/**/*.js', '!js/**/*.spec.js', '!js/**/*.fake.js', '!js/**/*.fortest.js', '!js/app_desktop_client.js'],
					'dist/modules.min.js': ['dist/modules.js'],
					'dist/templates.min.js': ['dist/templates.js']
				}
			},
			distForDesktop: {
				files: {
					'dist/mainForDesktop.min.js': ['js/**/*.js', '!js/**/*.spec.js', '!js/**/*.fake.js', '!js/**/*.fortest.js', '!js/app.js']
				}
			},
			dev: {
				files: {
					'dist/main.min.js': ['js/**/*.js', '!js/**/*.spec.js', '!js/**/*.fake.js', '!js/**/*.fortest.js', '!js/app_desktop_client.js']
				},
				options: {
					sourceMap: true,
					beautify: true,
					mangle: false
				}
			}
		},
		cacheBust: {
			options: {
				encoding: 'utf8',
				algorithm: 'md5',
				length: 16,
				deleteOriginals: false,
				rename: false,
				ignorePatterns: ['MaterialDesignIcons/css/materialdesignicons.min.css']
			},
			dist: {
				assets: {
					files: [{
						src: ['index.html']
					}]
				}
			},
			distForDesktop: {
				assets: {
					files: [{
						src: ['index_desktop_client.html']
					}]
				}
			}
		},
		processhtml: {
			dev: {
				options:{
					process:true,
					enviroment:'dev'
				},
				files: {
					'index.html': ['index.tpl.html']
				}
			},
			dist: {
				options:{
					process:true
				},
				files: {
					'index.html': ['index.tpl.html']
				}
			},
			distForDesktop: {
				files: {
					'index_desktop_client.html': ['index_desktop_client.tpl.html']
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
						dest: 'dist/',
						filter: 'isFile'
					},
				],
			},
			extras: {
				files: [{
					expand: true,
					cwd: 'node_modules/angular-ui-grid',
					src: ['*.ttf', '*.woff', '*.eot'],
					dest: 'dist/',
					filter: 'isFile'
				}]
			}
		},
		clean: {
			dist: {
				src: ['dist/*.js', 'dist/*.css', '!dist/*.min.js', '!dist/*.min.css']
			}
		},

		ngtemplates: {
			'wfm.templates': {
				src: ['html/**/*.html', 'js/**/*.html', 'js/**/html/*.html'],
				dest: 'dist/templates.js',
				options: {
					standalone: true
				}
			}
		}
	});

	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-contrib-copy');
	grunt.loadNpmTasks('grunt-contrib-cssmin');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-contrib-clean');
	grunt.loadNpmTasks('grunt-sass');
	grunt.loadNpmTasks('grunt-karma');
	grunt.loadNpmTasks('grunt-iisexpress');
	grunt.loadNpmTasks('grunt-msbuild');
	grunt.loadNpmTasks('grunt-angular-templates');
	grunt.loadNpmTasks('grunt-cache-Bust');
	grunt.loadNpmTasks('grunt-processhtml');



	// Default task(s).
	grunt.registerTask('default', ['devDist', 'test', 'watch:dev']); // this task run the main task and then watch for file changes
	grunt.registerTask('test', ['ngtemplates', 'karma:unit']);
	grunt.registerTask('devTest', ['ngtemplates', 'karma:dev']);
	grunt.registerTask('devDist', ['ngtemplates', 'sass', 'concat:distJs', 'concat:distCss', 'concat:distDarkCss', 'cssmin', 'copy', 'uglify:dev', 'generateIndexDev']);
	grunt.registerTask('test:continuous', ['ngtemplates', 'karma:continuous']);
	grunt.registerTask('dist', ['ngtemplates', 'sass', 'concat:distJs', 'concat:distCss', 'concat:distDarkCss', 'cssmin', 'uglify:dist', 'copy', 'generateIndex','clean:dist']); // this task should only be used by the build. It's kind of packaging for production.
	grunt.registerTask('nova', ['devDist', 'iisexpress:authBridge', 'iisexpress:web', 'watch:dev']); // this task run the main task and then watch for file changes
	grunt.registerTask('build', ['msbuild:build']); // build the solution
	grunt.registerTask('generateIndex', ['processhtml:dist', 'cacheBust:dist']);
	grunt.registerTask('generateIndexDev', ['processhtml:dev', 'cacheBust:dist']);

	// for desktop client
	grunt.registerTask('buildForDesktop', ['ngtemplates', 'sass', 'concat:distJsForDesktop', 'concat:distCss', 'concat:distDarkCss', 'cssmin', 'uglify:distForDesktop', 'copy:sourceMaps', 'processhtml:distForDesktop', 'cacheBust:distForDesktop']);

};
