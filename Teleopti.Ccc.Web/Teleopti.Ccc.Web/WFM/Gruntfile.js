module.exports = function (grunt) {

	// Project configuration.
	grunt.initConfig({
		watch: {
			dev: {
				files: ['css/*.css', 'js/**/*.js'],
				tasks: ['uglify'],
				options: {
					spawn: false,
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

	// Default task(s).
	grunt.registerTask('default', ['watch:dev']); // this task watchs
	grunt.registerTask('dist', ['uglify', 'download']); // this task is kind of package
};