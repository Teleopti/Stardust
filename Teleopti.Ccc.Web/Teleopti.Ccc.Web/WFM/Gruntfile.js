module.exports = function (grunt) {

	// Project configuration.
	grunt.initConfig({
		watch: {
			dev: {
				files: ['css/*.css', 'js/**/*.js'],
				tasks: ['concat'],
				options: {
					spawn: false,
				}
			}
		},

		concat: {
			options: {
				separator: '\r\n'
			},
			dist: {
				src: ['js/**/*.js'],
				dest: 'dist/main.js'
			}
		},

		uglify: {
			options: {
				sourceMap: true
			},
			dist: {
				files: {
					'dist/main.min.js': ['<%= concat.dist.dest %>']
				}
			}
		}
	});

	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-contrib-uglify');

	// Default task(s).
	grunt.registerTask('default', ['watch:dev']); // this task watchs
	grunt.registerTask('dist', ['concat', 'uglify']); // this task is kind of package
};