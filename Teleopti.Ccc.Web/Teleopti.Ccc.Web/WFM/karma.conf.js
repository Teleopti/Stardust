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
		'node_modules/angular/angular.js',
		'node_modules/angular-mocks/angular-mocks.js',
		'node_modules/angular-ui-router/release/angular-ui-router.min.js',
		'node_modules/angular-resource/angular-resource.min.js',
		'node_modules/angular-translate/dist/angular-translate.min.js',
		'node_modules/angular-translate/dist/angular-translate-loader-url/angular-translate-loader-url.min.js',
		'node_modules/angular-ui-bootstrap/ui-bootstrap-tpls.min.js',
		'node_modules/moment/min/moment-with-locales.min.js',
		'node_modules/angular-moment/angular-moment.min.js',
		'node_modules/angular-ui-indeterminate/dist/indeterminate.min.js',
		'node_modules/angular-ui-tree/dist/angular-ui-tree.min.js',
		'node_modules/angular-aria/angular-aria.min.js',
		'node_modules/angular-animate/angular-animate.min.js',
		'node_modules/angular-gantt/assets/angular-gantt.js',
		'node_modules/angular-gantt/assets/angular-gantt-plugins.js',
		'node_modules/angular-gantt/assets/angular-gantt-table-plugin.js',
		'vendor/angular-growl.js',
		'node_modules/ng-file-upload/dist/ng-file-upload-shim.min.js',
		'node_modules/ng-file-upload/dist/ng-file-upload.min.js',
		'node_modules/angular-ui-grid/ui-grid.min.js',
		'node_modules/ngstorage/ngStorage.min.js',
		'vendor/hammerjs/hammer.min.js',
		'node_modules/angular-material/angular-material.min.js',
		'vendor/fabricjs/fabric.js',
		'vendor/fabricjs/fabricjs_viewport.js',
		'vendor/ng-mfb/mfb.directive.js',
		'vendor/csv-js/csv.js',
		'vendor/ng-image-input-with-preview/ng-image-input-with-preview.js',
		'vendor/d3/d3.min.js',
		'vendor/c3/c3.min.js',
		'vendor/c3/c3-angular.min.js',
		'js/**/*.js', 
		'dist/templates.js', 
		'tests/**/*spec.js'
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
	singleRun: true
  });
};
