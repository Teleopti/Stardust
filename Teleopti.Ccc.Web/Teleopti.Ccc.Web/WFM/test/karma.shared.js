// http://karma-runner.github.io/1.0/config/configuration-file.html

function applyBaseConfig(config) {
	config.set({
		basePath: '../',
		exclude: ['**/Gruntfile.js'],
		frameworks: ['jasmine', '@angular-devkit/build-angular'],
		plugins: [
			require('@angular-devkit/build-angular/plugins/karma'),
			require('karma-teamcity-reporter'),
			require('karma-mocha-reporter'),
			require('karma-jasmine'),
			require('karma-chrome-launcher'),
			require('karma-ng-html2js-preprocessor')
		],

		proxies: {
			'/dist/ng2/assets/': '/assets/'
		},

		captureTimeout: 210000,
		browserDisconnectTolerance: 3,
		browserDisconnectTimeout: 210000,
		browserNoActivityTimeout: 210000,
		autoWatchBatchDelay: 1000,
		autoWatch: true,
		reporters: ['mocha'],
		mochaReporter: {
			symbols: {
				success: '✔'
			},
			ignoreSkipped: true
		},
		port: 9876,

		// level of logging
		// possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
		logLevel: config.LOG_INFO,
		browsers: ['Chrome'],
		customLaunchers: {
			Chrome_small: {
				base: 'Chrome',
				flags: ['--window-size=400,400', '--window-position=-400,0']
			}
		},
		singleRun: false, // Continuous Integration mode
		// to use this attribute, run "npm run devTest -- keyWordFromSpecFileDescribe". example: npm run devTest -- Rta
		// client: {
		// 	args: parseTestPattern(process.argv)
		// }
		preprocessors: {
			'+(app|html)/**/*.html': ['ng-html2js']
		},
		ngHtml2JsPreprocessor: {
			moduleName: 'wfm.templates'
		}
	});
}

module.exports = { applyBaseConfig };
