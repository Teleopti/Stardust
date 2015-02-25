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
			dev: {
				scripts: {
					files: ['css/*.scss', 'js/*.js'],
					tasks: ['sass', 'concat'],
					options: {
						spawn: false,
					},
				}
			}, //'sass:styleguide', 'kss:dist', 
			styleguide: {
				files: ['css/*.scss'],
				tasks: ['sass:styleguide', 'shell']
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
		},

		kss: {
			options: {
				template: 'wfm-template'
			},
			dist: {
				files: {
					'styleguide': ['css']
				}
			}
		},
		shell: {
			options: {
				stderr: false
			},
			target: {
				command: 'kss-node css styleguide css\styleguide.md --template wfm-template'
			}
		}

	});

	grunt.loadNpmTasks('grunt-sass');
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-kss');
	grunt.loadNpmTasks('grunt-shell');


	// Default task(s).
	grunt.registerTask('default', ['sass', 'watch']); // this task watchs
	grunt.registerTask('styleguide', ['watch:styleguide']); // this task watchs
	grunt.registerTask('dist', ['concat', 'uglify', 'sass']); // this task is kind of package
};