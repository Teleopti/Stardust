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

	grunt.loadNpmTasks('grunt-sass');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-contrib-uglify');

	grunt.registerTask('dist', ['concat', 'uglify', 'sass']);
};