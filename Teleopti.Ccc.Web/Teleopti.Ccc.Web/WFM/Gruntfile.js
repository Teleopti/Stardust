module.exports = function (grunt) {

	// Project configuration.
	grunt.initConfig({
		watch: {
			dev: {
				files: ['css/style.scss', 'js/**/*.js'],
				tasks: ['uglify', 'sass'],
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
					'dist/style.css': ['css/style.scss']
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


	// Default task(s).
	grunt.registerTask('default', ['watch:dev']); // this task watchs
	grunt.registerTask('unitTest', ['watch:test']); // this task watchs

	grunt.registerTask('dist', ['uglify', 'download', 'sass']); // this task is kind of package
};