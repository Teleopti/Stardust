module.exports = function (grunt) {

	// Project configuration.
	grunt.initConfig({
		sass: {
			dist: {
				files: {
					'css/main.css': 'css/main.scss'
				}
			},
			styleguide: {
				files: {
					'css/styleguide.css': 'css/_styleguide.scss'
				}
			}
		},
		watch: {
			scripts: {
				files: ['css/*.scss', 'js/*.js'],
				tasks: ['sass', 'concat'], 
				options: {
					spawn: false,
				},
			},
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

		/*uglify: {
			options: {
				sourceMap: true
			},
			dist: {
				files: {
					'dist/main.min.js': ['<%= concat.dist.dest %>']
				}
			}
		}*/

	});

	grunt.loadNpmTasks('grunt-contrib-sass');
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-concat');
	//grunt.loadNpmTasks('grunt-contrib-uglify');


	// Default task(s).
	grunt.registerTask('default', ['sass', 'watch']); // this task watchs
	grunt.registerTask('styleguide', ['sass:styleguide']); // this task watchs
	grunt.registerTask('dist', ['concat']); // this task is kind of package
};