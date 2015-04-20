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
		'node_modules/angular/angular.min.js',
		'node_modules/angular-mocks/angular-mocks.js',
		'node_modules/angular-ui-router/release/angular-ui-router.min.js',
		'node_modules/angular-resource/angular-resource.min.js',
		'node_modules/angular-translate/dist/angular-translate.min.js',
		'node_modules/angular-translate/dist/angular-translate-loader-url/angular-translate-loader-url.min.js',
		'vendor/ui-bootstrap-custom-build/ui-bootstrap-custom-tpls-0.12.1.min.js',
		'node_modules/angular-moment/angular-moment.min.js',
		'vendor/angular-ui-tree/angular-ui-tree.min.js',
		'vendor/angular-aria/angular-aria.min.js',
		'vendor/angular-animate/angular-animate.min.js',
		'vendor/angular-growl.js',
		'vendor/ui-grid/ui-grid-stable.min.js',
		'vendor/hammerjs/hammer.min.js',
		'vendor/angular-material/angular-material.min.js',
		'vendor/fabricjs/fabric.min.js',
		'vendor/fabricjs/fabricjs_viewport.js',
		'vendor/ng-mfb/mfb.directive.js',
		'vendor/ng-image-input-with-preview/ng-image-input-with-preview.js',
		'vendor/d3/d3.min.js',
		'vendor/n3-chart/line-chart/line-chart.min.js',
		'js/ABmetrics.min.js',
		'dist/main.min.js', 
		'tests/**/*.js'
	],


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
	singleRun: false
  });
};
