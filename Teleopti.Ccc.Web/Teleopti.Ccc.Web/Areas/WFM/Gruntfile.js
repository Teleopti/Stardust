module.exports = function (grunt) {

	// Project configuration.
	grunt.initConfig({
		sass: {
			dist: {
				files: {
					'css/main.css': 'css/main.scss'
				}
			}
		},
		watch: {
			scripts: {
				files: ['css/*.scss', 'js/*.js'],
				tasks: ['sass', 'concat', 'uglify'], //
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
				src: ['js/**/*.js', 'node_modules/angular-ui-router/release/angular-ui-router.min.js', 'node_modules/angular-resource/angular-resource.min.js', 'vendor/ui-bootstrap.min.js', 'node_modules/moment/min/moment-with-locales.min.js', 'node_modules/angular-moment/angular-moment.js', 'vendor/angular-ui-tree/angular-ui-tree.min.js', 'vendor/angular-aria/angular-aria.js', 'vendor/angular-animate/angular-animate.js', 'vendor/hammerjs/hammer.js', 'vendor/angular-material/angular-material.js', 'js/ABmetrics.min.js', 'js/app.js', 'js/wfmCtrls.js'],
				dest: 'dist/main.js'
			}
		},

		uglify: {
			options: {
				banner: '/*! <%= grunt.template.today("dd-mm-yyyy") %> */\n'
			},
			dist: {
				files: {
					'dist/main.min.js': ['<%= concat.dist.dest %>']
				}
			}
		}

	});

	grunt.loadNpmTasks('grunt-contrib-sass');
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-contrib-uglify');


	// Default task(s).
	grunt.registerTask('default', ['sass', 'watch']); // this task watchs
	grunt.registerTask('dist', ['concat', 'uglify']); // this task is kind of package
};