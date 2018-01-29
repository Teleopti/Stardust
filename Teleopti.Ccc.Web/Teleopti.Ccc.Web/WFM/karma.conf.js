// Karma configuration
// Generated on Wed Feb 04 2015 10:25:13 GMT+0100 (W. Europe Standard Time)

module.exports = function (config) {
	config.set({

		// base path that will be used to resolve all patterns (eg. files, exclude)
		basePath: '',

		// to use this attribute, run "npm run devTest -- keyWordFromSpecFileDescribe". example: npm run devTest -- rta
		client: {
			args: parseTestPattern(process.argv)
		},

		// frameworks to use
		// available frameworks: https://npmjs.org/browse/keyword/karma-adapter
		frameworks: ['jasmine'],


		// list of files / patterns to load in the browser
		files: [
			// 'dist/resources/modules.js',
			// 'dist/templates.js',
			// 'app/**/*.js',
			// 'node_modules/angular-material/angular-material-mocks.js',
			// 'node_modules/angular-mocks/angular-mocks.js',


			{ pattern: 'dist/resources/modules.js', watched: false },
			{ pattern: 'dist/templates.js', watched: false, nocache: true },
			{ pattern: 'node_modules/angular-material/angular-material-mocks.js', watched: false },
			{ pattern: 'node_modules/angular-mocks/angular-mocks.js', watched: false },
			{ pattern: 'app/**/!(*.spec|app_desktop_client).js', watched: false, nocache: true },
			'app/**/*.spec.js',

			//served seat image file at browser because addSeat function need to create seat object from image in seatManagement test.
			{ pattern: 'app/seatManagement/images/*.svg', watched: false, included: false, served: true }
		],

		proxies: {
			'/app/seatManagement/images/': '/base/app/seatManagement/images/'
		},

		// list of files to exclude
		exclude: [
			'**/Gruntfile.js'
		],
		captureTimeout: 210000,
		browserDisconnectTolerance: 3, 
		browserDisconnectTimeout : 210000,
		browserNoActivityTimeout : 210000,

		// preprocess matching files before serving them to the browser
		// available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
		preprocessors: {
		},


		// test results reporter to use
		// possible values: 'dots', 'progress'
		// available reporters: https://npmjs.org/browse/keyword/karma-reporter
		reporters: ['teamcity'],



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

		customLaunchers: {
			Chrome_small: {
				base: 'Chrome',
				flags: [
					'--window-size=400,400',
					'--window-position=-400,0'
				]
			}
		},

		// Continuous Integration mode
		// if true, Karma captures browsers, runs the tests and exits
		singleRun: true
	});
};

function parseTestPattern(argv) {
	var found = false;
	var pattern = argv.map(function (v) {
		if (found) {
			return v;
		}
		if (v === '--') {
			found = true;
		}
	}).filter(function (a) { return a }).join(' ');
	return pattern ? ['--grep', pattern] : [];
};
