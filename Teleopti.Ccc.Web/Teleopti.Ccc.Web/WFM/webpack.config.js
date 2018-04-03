const webpack = require('webpack'); //to access built-in plugins
const path = require('path');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const ConcatPlugin = require('webpack-concat-plugin');
const ExtraneousFileCleanupPlugin = require('webpack-extraneous-file-cleanup-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');

const htmlPlugin = new HtmlWebpackPlugin({
	message: 'this is a dev template :D',
	template: 'index.webpack.tpl.html',
	// chunks: [] // no chunks
	inject: false
});

const extractSass = new ExtractTextPlugin({
	filename: '[name].css'
	// disable: process.env.NODE_ENV === 'development'
});

const concatJsModules = new ConcatPlugin({
	// examples
	uglify: false,
	sourceMap: false,
	name: 'devJsModules',
	// outputPath: 'dist',
	fileName: 'resources/modules.js',
	filesToConcat: [
		'./node_modules/angular/angular.min.js',
		'./node_modules/angular-ui-router/release/angular-ui-router.min.js',
		'./node_modules/angular-resizable/angular-resizable.min.js',
		'./node_modules/angular-resource/angular-resource.min.js',
		'./node_modules/angular-sanitize/angular-sanitize.min.js',
		'./node_modules/angular-translate/dist/angular-translate.min.js',
		'./node_modules/angular-translate/dist/angular-translate-loader-url/angular-translate-loader-url.min.js',
		'./node_modules/angular-dynamic-locale/tmhDynamicLocale.min.js',
		'./node_modules/moment/min/moment-with-locales.min.js',
		'./node_modules/moment-timezone/builds/moment-timezone-with-data.min.js',
		'./node_modules/angular-moment/angular-moment.min.js',
		'./node_modules/ng-file-upload/dist/ng-file-upload-shim.min.js',
		'./node_modules/ng-file-upload/dist/ng-file-upload.min.js',
		'./node_modules/angular-ui-grid/ui-grid.min.js',
		'./node_modules/angular-ui-indeterminate/dist/indeterminate.min.js',
		'./node_modules/ngstorage/ngStorage.min.js',
		'./node_modules/default-passive-events/dist/index.js',
		'./node_modules/angular-ui-tree/dist/angular-ui-tree.min.js',
		'./node_modules/angular-aria/angular-aria.min.js',
		'./node_modules/angular-animate/angular-animate.min.js',
		'./node_modules/angular-gantt/assets/angular-gantt.js',
		'./node_modules/angular-gantt/assets/angular-gantt-plugins.js',
		'./node_modules/angular-gantt/assets/angular-gantt-table-plugin.js',
		'./node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.js',
		'./node_modules/teleopti-styleguide/styleguide/dist/wfmdirectives.min.js',
		'./node_modules/teleopti-styleguide/styleguide/dist/templates.js',
		'./node_modules/filesaver.js/FileSaver.min.js',
		'./node_modules/jquery/dist/jquery.min.js',
		'./node_modules/hammerjs/hammer.min.js',
		'./node_modules/angular-material/angular-material.min.js',
		'./node_modules/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js',
		'./vendor/fabricjs/fabric.min.js',
		'./vendor/fabricjs/fabricjs_viewport.js',
		'./vendor/ng-image-input-with-preview/ng-image-input-with-preview.js',
		'./vendor/d3/d3.min.js',
		'./vendor/c3/c3.min.js',
		'./vendor/c3/c3-angular.min.js',
		'./vendor/ui-bootstrap-custom-build/datepicker.directive.ext.js',
		'./vendor/ui-bootstrap-custom-build/timepicker.directive.ext.js',
		'./node_modules/angular-dialog-service/dist/dialogs.min.js',
		'./node_modules/angular-dialog-service/dist/dialogs-default-translations.min.js',
		'./vendor/angular-bootstrap-persian-datepicker-master/persiandate.js',
		'./vendor/angular-bootstrap-persian-datepicker-master/persian-datepicker-tpls.js',
		'../Content/signalr/jquery.signalR-2.2.2.js',
		'../Content/signalr/broker-hubs.js',
		'./node_modules/lodash/lodash.min.js'
	],
	attributes: {
		async: false
	}
});
const concatDevJs = new ConcatPlugin({
	uglify: false,
	sourceMap: true,
	name: 'devJS',
	// outputPath: 'dist',
	fileName: 'main.js',
	filesToConcat: [
		[
			'./app/**/*.js',
			'!./app/**/*.spec.js',
			'!./app/**/*.fake.js',
			'!./app/**/*.fortest.js',
			'!./app/app_desktop_client.js'
		]
	],
	attributes: {
		async: false
	}
});
const concatDistCss = new ConcatPlugin({
	uglify: false,
	sourceMap: false,
	name: 'distCss',
	// outputPath: 'dist',
	fileName: 'resources/modules_classic.min.css',
	filesToConcat: [
		'./node_modules/bootstrap/dist/css/bootstrap.min.css',
		'./node_modules/angular-resizable/angular-resizable.min.css',
		'./node_modules/angular-ui-tree/dist/angular-ui-tree.min.css',
		'./node_modules/angular-ui-grid/ui-grid.min.css',
		'./node_modules/angular-material/angular-material.min.css',
		'./node_modules/angular-gantt/assets/angular-gantt.css',
		'./node_modules/angular-gantt/assets/angular-gantt-plugins.css',
		'./node_modules/angular-gantt/assets/angular-gantt-table-plugin.css',
		'./node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.css',
		'./vendor/c3/c3.min.css',
		'./node_modules/teleopti-styleguide/styleguide/dist/main.min.css'
	],
	attributes: {
		async: false
	}
});
const concatDistDarkCss = new ConcatPlugin({
	uglify: false,
	sourceMap: false,
	name: 'distDarkCss',
	// outputPath: 'dist',
	fileName: 'resources/modules_dark.css', // prod env uses a .min.css name instead
	filesToConcat: [
		'./node_modules/bootstrap/dist/css/bootstrap.min.css',
		'./node_modules/angular-resizable/angular-resizable.min.css',
		'./node_modules/angular-ui-tree/dist/angular-ui-tree.min.css',
		'./node_modules/angular-ui-grid/ui-grid.min.css',
		'./node_modules/angular-material/angular-material.min.css',
		'./node_modules/angular-gantt/assets/angular-gantt.css',
		'./node_modules/angular-gantt/assets/angular-gantt-plugins.css',
		'./node_modules/angular-gantt/assets/angular-gantt-table-plugin.css',
		'./node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.css',
		'./vendor/c3/c3.min.css',
		'./node_modules/teleopti-styleguide/styleguide/dist/main_dark.min.css'
	],
	attributes: {
		async: false
	}
});

const copyPlugin = new CopyWebpackPlugin([
	// sourceMaps from old
	{
		from: { glob: 'vendor/*/*.@(ttf|woff|eot)' },
		to: 'resources',
		flatten: true
	},
	// extras from old
	{
		from: { glob: 'node_modules/angular-ui-grid/*.@(ttf|woff|eot)' },
		to: 'resources',
		flatten: true
	},
	// bootstrap from old
	{
		from: { glob: 'node_modules/bootstrap/fonts/*.@(ttf|woff|eot)' },
		to: 'fonts',
		flatten: true
	}
]);

const cleanUpPlugin = new ExtraneousFileCleanupPlugin({
	extensions: ['.js'],
	paths: ['style_classic', 'style_dark'],
	minBytes: 3500
});

const config = {
	entry: {
		// ignore_entry: './webpack-entry.js',
		'style_classic.min': './css/style.scss',
		'style_dark.min': './css/darkstyle.scss'
	},
	output: {
		path: __dirname + '/dist',
		filename: '[name].js'
	},
	module: {
		rules: [
			{
				test: /\.(svg)$/,
				use: [
					{
						loader: 'url-loader',
						options: {
							limit: 8192
						}
					}
				]
			},
			{
				test: /\.scss$/,
				use: extractSass.extract({
					use: [
						{
							loader: 'css-loader'
						},
						{
							loader: 'sass-loader',
							options: {
								includePaths: ['node_modules/teleopti-styleguide/styleguide/sass']
							}
						}
					]
					// // use style-loader in development
					// fallback: 'style-loader'
				})
			}
		]
	},
	plugins: [
		cleanUpPlugin,
		concatJsModules,
		concatDevJs,
		concatDistCss,
		concatDistDarkCss,
		extractSass,
		copyPlugin,
		htmlPlugin
	]
};

module.exports = config;
