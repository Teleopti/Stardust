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
				files: ['css/*.scss'],
				tasks: ['sass'],
				options: {
					spawn: false,
				},
			},
		},
	});


	grunt.loadNpmTasks('grunt-contrib-sass');
	grunt.loadNpmTasks('grunt-contrib-watch');

	// Default task(s).
	grunt.registerTask('default', ['sass','watch']);

};