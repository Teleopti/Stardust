// http://karma-runner.github.io/1.0/config/configuration-file.html

const { applyBaseConfig } = require('./karma.shared');

module.exports = function(config) {
	applyBaseConfig(config);
	config.set({
		files: [
			{ pattern: 'dist/resources/modules.js', watched: false },
			{ pattern: '+(app|html)/**/*.html' },
			{ pattern: 'node_modules/angular-material/angular-material-mocks.js', watched: false },
			{ pattern: 'node_modules/angular-mocks/angular-mocks.js', watched: false },
			{ pattern: 'app/**/!(*.spec|app_desktop_client).js' },
			{ pattern: 'app/**/*.spec.js' },

			{
				pattern: 'node_modules/angular-i18n/angular-locale_*.js',
				watched: false,
				included: false
			}
		],
		autoWatch: false,

		reporters: ['teamcity'],

		// Continuous Integration mode
		// if true, Karma captures browsers, runs the tests and exits
		singleRun: true
	});
};
