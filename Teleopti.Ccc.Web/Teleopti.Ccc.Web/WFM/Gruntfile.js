module.exports = function (grunt) {

	// Project configuration.
	grunt.initConfig({
		watch: {
			dev: {
				files: ['css/*.scss', 'js/**/*.js'],
				tasks: ['uglify:dev', 'sass', 'cssmin'],
				options: {
					spawn: false,
				}
			},
			test: {
				files: ['tests/**/*.js', 'js/**/*.js'],
				tasks: ['karma']
			}
		},

		karma: {
			unit: {
				configFile: 'karma.conf.js',
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
				options: {
					sourceMap: true,
					beautify: true,
					mangle: false
				},
				files: {
					'dist/main.min.js': ['js/**/*.js']
				}
			}
		}
	});

	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-sass');
	grunt.loadNpmTasks('grunt-karma');
	grunt.loadNpmTasks('grunt-contrib-cssmin');



	// Default task(s).
	grunt.registerTask('default', ['uglify:dev', 'sass', 'cssmin', 'watch:dev']); // this task run the main task and then watch for file changes
	grunt.registerTask('unitTest', ['watch:test']); // this task watchs the js tests files and run the tests if needed

	grunt.registerTask('dist', ['uglify:dist', 'sass', 'cssmin']); // this task should only be used by the build. It's kind of packaging for production.
};