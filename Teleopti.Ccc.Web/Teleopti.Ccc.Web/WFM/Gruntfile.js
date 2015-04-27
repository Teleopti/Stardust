module.exports = function (grunt) {

	// Project configuration.
	grunt.initConfig({
		watch: {
			dev: {
				files: ['css/*.scss', 'js/**/*.js'],
				tasks: ['uglify', 'sass', 'cssmin'],
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
					'dist/style.min.css': ['css/style.css', 'css/main.css']
				}
			}
		},

		uglify: {
			options: {
				sourceMap: true
			},
			dist: {
				files: {
					'dist/main.min.js': ['js/**/*.js']
				}
			}
		},

		download: {
			dist: {
				src: ['http://teleopti.github.io/styleguide/css/main.css'],
				dest: 'css/'
			},
		}
	});

	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-http-download');
	grunt.loadNpmTasks('grunt-sass');
	grunt.loadNpmTasks('grunt-karma');
	grunt.loadNpmTasks('grunt-contrib-cssmin');


	// Default task(s).
	grunt.registerTask('default', ['watch:dev']); // this task watchs
	grunt.registerTask('unitTest', ['watch:test']); // this task watchs

	grunt.registerTask('dist', ['uglify', 'download', 'sass', 'cssmin']); // this task is kind of package
};