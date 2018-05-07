module.exports = function(config) {
	config.set({
		basePath: '../',
		files: [],
		exclude: ['app/**/*'],
		frameworks: ['jasmine', '@angular-devkit/build-angular'],
		plugins: [
			require('@angular-devkit/build-angular/plugins/karma'),
			require('karma-jasmine'),
			require('karma-mocha-reporter'),
			require('karma-chrome-launcher')
		],

		preprocessors: {},
		captureTimeout: 210000,
		browserDisconnectTolerance: 3,
		browserDisconnectTimeout: 210000,
		browserNoActivityTimeout: 210000,
		angularCli: {
			environment: 'dev'
		},
		mochaReporter: {
			symbols: {
				success: '✔'
			}
		},
		reporters: ['mocha'],
		port: 9876,
		colors: true,
		// possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
		logLevel: config.LOG_INFO,
		autoWatch: true,
		browsers: ['Chrome'],
		customLaunchers: {
			Chrome_small: {
				base: 'Chrome',
				flags: ['--window-size=400,400', '--window-position=-400,0']
			}
		}
		// Continuous Integration mode
		// if true, Karma captures browsers, runs the tests and exits
		// singleRun: true
	});
};
