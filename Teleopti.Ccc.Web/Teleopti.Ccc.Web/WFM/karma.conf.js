// Karma configuration
// Generated on Wed Feb 04 2015 10:25:13 GMT+0100 (W. Europe Standard Time)

module.exports = function(config) {
  config.set({

	// base path that will be used to resolve all patterns (eg. files, exclude)
	basePath: '',


	// frameworks to use
	// available frameworks: https://npmjs.org/browse/keyword/karma-adapter
	frameworks: ['jasmine'],


	// list of files / patterns to load in the browser
	files: [
        'dist/assets/js/modules.js',
		'node_modules/angular-mocks/angular-mocks.js',
		'dist/assets/js/templates.js',
		'js/**/*.js',
		'tests/**/*spec.js',

		//served seat image file at browser because addSeat function need to create seat object from image in seatManagement test.
		{ pattern: 'js/seatManagement/images/*.svg', watched: false, included: false, served: true },
		'js/**/*.js',
		'dist/assets/js/templates.js',
		'tests/**/*spec.js'
	],

	proxies: {
		'/js/seatManagement/images/': '/base/js/seatManagement/images/'
	},

	// list of files to exclude
	exclude: [
		'**/Gruntfile.js'
	],


	// preprocess matching files before serving them to the browser
	// available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
	preprocessors: {
	},


	// test results reporter to use
	// possible values: 'dots', 'progress'
	// available reporters: https://npmjs.org/browse/keyword/karma-reporter
	reporters: ['progress'],


	// web server port
	port: 9876,


	// enable / disable colors in the output (reporters and logs)
	colors: true,


	// level of logging
	// possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
	logLevel: config.LOG_INFO,


	// enable / disable watching file and executing tests whenever any file changes
	autoWatch: true,


	// start these browsers
	// available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
	browsers: ['Chrome'],


	// Continuous Integration mode
	// if true, Karma captures browsers, runs the tests and exits
	singleRun: true
  });
};
