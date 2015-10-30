module.exports = function (grunt) {
	// Project configuration.
	grunt.initConfig({
		watch: {
			dev: {
				files: ['css/*.scss', 'js/**/*.js', 'tests/**/*.js'],
				tasks: ['uglify:dev', 'sass', 'cssmin'],
				options: {
					spawn: false,
				}
			}
		},
		iisexpress: {
			 authBridge: {
					 options: {
							 site:'teleopti.ccc.web.authenticationBridge'
					 }
			 },
			 web: {
					options: {
						site:'teleopti.ccc.web',
						openUrl:'http://localhost:52858',
						open:true,
                        verbose:true
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
			unit: {
			},
			dev:{
				singleRun:false
			},
			continuous: {
				reporters: 'teamcity'
			}
		},

		sass: {
			dist: {
				files: {
					'css/style.css': ['css/style.scss']
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
					'dist/style.min.css': ['css/style.css']
				}
			}
		},

		uglify: {
			dist: {
				files: {
					'dist/main.min.js': ['js/**/*.js']
				}
			},
			dev: {
				files: {
					'dist/main.min.js': ['js/**/*.js']
				},
				options: {
					sourceMap: true,
					beautify: true,
					mangle: false
				}
			}
		},

		ngtemplates: {
			'wfm.templates': {
				src: ['html/**/*.html', 'js/**/*.html'],
				dest: 'dist/templates.js',
				options: {
					standalone: true
				}
			}
		}
	});

	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-sass');
	grunt.loadNpmTasks('grunt-karma');
	grunt.loadNpmTasks('grunt-contrib-cssmin');
	grunt.loadNpmTasks('grunt-iisexpress');
	grunt.loadNpmTasks('grunt-msbuild');
	grunt.loadNpmTasks('grunt-angular-templates');

	// Default task(s).
	grunt.registerTask('default', ['uglify:dev', 'sass', 'cssmin', 'test', 'watch:dev']); // this task run the main task and then watch for file changes
	grunt.registerTask('test', ['ngtemplates', 'karma:unit']);
	grunt.registerTask('test:continuous', ['ngtemplates', 'karma:continuous']);
	grunt.registerTask('unitTest', ['test']);

	grunt.registerTask('dist', ['uglify:dist', 'sass', 'cssmin']); // this task should only be used by the build. It's kind of packaging for production.
	grunt.registerTask('nova', ['uglify:dev', 'sass', 'cssmin','iisexpress:authBridge','iisexpress:web', 'watch:dev']); // this task run the main task and then watch for file changes
	grunt.registerTask('rebuild', ['msbuild:rebuild']); // clean and rebuild the solution
	grunt.registerTask('build', ['msbuild:build']); // build the solution

};
