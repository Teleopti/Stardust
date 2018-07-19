const webpack = require('webpack'); //to access built-in plugins
const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const ConcatPlugin = require('webpack-concat-plugin');
const ExtraneousFileCleanupPlugin = require('webpack-extraneous-file-cleanup-plugin');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

module.exports = env => {
	const isProd = typeof env !== 'undefined' ? env.production : false;
	const isDev = !isProd;

	const htmlPlugin = new HtmlWebpackPlugin({
		message: 'this is a dev template :D',
		production: isProd,
		template: './src/index.html',
		filename: '../index.html',
		// chunks: [] // no chunks
		inject: false
	});

	const extractSass = new MiniCssExtractPlugin({
		filename: '[name].css'
	});

	const uglifyDev = {
		mangle: false,
		compress: false,
		keep_fnames: true,
		keep_classnames: true,
		output: {
			ascii_only: false,
			beautify: false,
			bracketize: false,
			comments: true,
			ecma: 5,
			inline_script: false,
			keep_quoted_props: false,
			preamble: null,
			quote_keys: false,
			quote_style: 0,
			wrap_iife: false
		}
	};
	const uglifyProd = {
		mangle: false,
		compress: true,
		keep_fnames: true,
		keep_classnames: true,
		output: {
			ascii_only: false,
			beautify: false,
			bracketize: false,
			comments: false,
			ecma: 5,
			inline_script: false,
			keep_quoted_props: false,
			preamble: null,
			quote_keys: false,
			quote_style: 0,
			wrap_iife: false
		}
	};
	const concatJsModules = new ConcatPlugin({
		// examples
		uglify: false,
		sourceMap: false,
		name: 'jsModules',
		// outputPath: 'dist',
		fileName: 'resources/modules.js',
		filesToConcat: [
			'./node_modules/angular/angular.js',
			'./node_modules/angular-ui-router/release/angular-ui-router.js',
			'./node_modules/angular-resizable/src/angular-resizable.js',
			'./node_modules/angular-resource/angular-resource.js',
			'./node_modules/angular-sanitize/angular-sanitize.js',
			'./node_modules/angular-translate/dist/angular-translate.js',
			'./node_modules/angular-translate/dist/angular-translate-loader-url/angular-translate-loader-url.js',
			'./node_modules/angular-dynamic-locale/src/tmhDynamicLocale.js',
			'./node_modules/moment/min/moment-with-locales.js',
			'./node_modules/moment-timezone/builds/moment-timezone-with-data.js',
			'./node_modules/angular-moment/angular-moment.js',
			'./node_modules/ng-file-upload/dist/ng-file-upload-shim.js',
			'./node_modules/ng-file-upload/dist/ng-file-upload.js',
			'./node_modules/angular-ui-grid/ui-grid.js',
			'./node_modules/angular-ui-indeterminate/dist/indeterminate.js',
			'./node_modules/ngstorage/ngStorage.js',
			'./node_modules/default-passive-events/dist/index.js',
			'./node_modules/angular-ui-tree/dist/angular-ui-tree.js',
			'./node_modules/angular-aria/angular-aria.js',
			'./node_modules/angular-animate/angular-animate.js',
			'./node_modules/angular-gantt/assets/angular-gantt.js',
			'./node_modules/angular-gantt/assets/angular-gantt-plugins.js',
			'./node_modules/angular-gantt/assets/angular-gantt-table-plugin.js',
			'./node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.js',
			'./node_modules/teleopti-styleguide/styleguide/dist/wfmdirectives.min.js',
			'./node_modules/teleopti-styleguide/styleguide/dist/templates.js',
			'./node_modules/filesaver.js/FileSaver.js',
			'./node_modules/jquery/dist/jquery.js',
			'./node_modules/hammerjs/hammer.js',
			'./node_modules/angular-material/angular-material.js',
			'./node_modules/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js',
			'./vendor/fabricjs/fabric.js',
			'./vendor/fabricjs/fabricjs_viewport.js',
			'./vendor/ng-image-input-with-preview/ng-image-input-with-preview.js',
			'./node_modules/d3/d3.js',
			'./node_modules/c3/c3.js',
			'./node_modules/c3-angular/c3-angular.js',
			'./vendor/ui-bootstrap-custom-build/datepicker.directive.ext.js',
			'./vendor/ui-bootstrap-custom-build/timepicker.directive.ext.js',
			'./node_modules/angular-dialog-service/dist/dialogs.js',
			'./node_modules/angular-dialog-service/dist/dialogs-default-translations.js',
			'./vendor/angular-bootstrap-persian-datepicker-master/persiandate.js',
			'./vendor/angular-bootstrap-persian-datepicker-master/persian-datepicker-tpls.js',
			'../Content/signalr/jquery.signalR-2.2.2.js',
			'../Content/signalr/broker-hubs.js',
			'./node_modules/lodash/lodash.js'
		],
		attributes: {
			async: false
		}
	});
	const concatJs = new ConcatPlugin({
		uglify: isProd ? uglifyProd : uglifyDev,
		sourceMap: true,
		name: 'js',
		// outputPath: 'dist',
		fileName: 'main.js',
		filesToConcat: [
			'./app/app.js',
			'./app/**/_modules.js',
			[
				'./app/**/*.js',
				'!./app/**/*.spec.js',
				'!./app/**/*.fake.js',
				'!./app/**/*.fortest.js',
				'!./app/app.js',
				'!./app/app_desktop_client.js',
				'!./app/**/_modules.js'
			]
		],
		attributes: {
			async: false
		}
	});
	const concatCss = new ConcatPlugin({
		uglify: false,
		sourceMap: false,
		name: 'distCss',
		// outputPath: 'dist',
		fileName: 'styleguide_classic.css',
		filesToConcat: ['./node_modules/teleopti-styleguide/styleguide/dist/main.min.css'],
		attributes: {
			async: false
		}
	});
	const concatDarkCss = new ConcatPlugin({
		uglify: false,
		sourceMap: false,
		name: 'darkCss',
		// outputPath: 'dist',
		fileName: 'styleguide_dark.css', // prod env uses a .min.css name instead
		filesToConcat: ['./node_modules/teleopti-styleguide/styleguide/dist/main_dark.min.css'],
		attributes: {
			async: false
		}
	});
	const concatCssDependencies = new ConcatPlugin({
		uglify: false,
		sourceMap: false,
		name: 'dependenciesCss',
		// outputPath: 'dist',
		fileName: 'dependencies.css', // prod env uses a .min.css name instead
		filesToConcat: [
			'./node_modules/c3/c3.min.css',
			'./node_modules/bootstrap/dist/css/bootstrap.css',
			'./node_modules/angular-resizable/src/angular-resizable.css',
			'./node_modules/angular-ui-tree/source/angular-ui-tree.css',
			'./node_modules/angular-ui-grid/ui-grid.css',
			'./node_modules/angular-material/angular-material.css',
			'./node_modules/angular-gantt/assets/angular-gantt.css',
			'./node_modules/angular-gantt/assets/angular-gantt-plugins.css',
			'./node_modules/angular-gantt/assets/angular-gantt-table-plugin.css',
			'./node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.css'
		],
		attributes: {
			async: false
		}
	});

	const copyPlugin = new CopyWebpackPlugin([
		// sourceMaps from old
		// {
		// 	from: { glob: 'vendor/*/*.@(ttf|woff|eot)' },
		// 	to: 'resources',
		// 	flatten: true
		// },
		// extras from old
		{
			from: { glob: 'node_modules/angular-ui-grid/*.@(ttf|woff|eot)' },
			to: '',
			flatten: true
		},
		// bootstrap from old
		{
			from: { glob: 'node_modules/bootstrap/fonts/*.@(ttf|woff|eot)' },
			to: 'fonts',
			flatten: true
		}
	]);

	const preCleanFiles = isProd ? ['dist/*.map'] : [];
	const preCleanUpPlugin = new CleanWebpackPlugin(preCleanFiles, {
		verbose: false,
		dry: false,
		beforeEmit: true
	});
	const postCleanUpPlugin = new ExtraneousFileCleanupPlugin({
		extensions: ['.js'],
		paths: ['style_classic', 'style_dark', 'ant_classic', 'ant_dark'],
		minBytes: 3500
	});

	const config = {
		entry: {
			// ignore_entry: './webpack-entry.js',
			style_classic: './css/style.scss',
			style_dark: './css/darkstyle.scss',
			ant_classic: './src/themes/ant_classic.less',
			ant_dark: './src/themes/ant_dark.less'
		},
		mode: isProd ? 'production' : 'development',
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
					test: /\.s?[ac]ss$/,
					use: [
						MiniCssExtractPlugin.loader,
						'css-loader',
						'postcss-loader',
						{
							loader: 'sass-loader',
							options: {
								outputStyle: isProd ? 'compressed' : 'nested',
								includePaths: ['node_modules/teleopti-styleguide/styleguide/sass']
							}
						}
					]
				},
				{
					test: /\.less$/,
					use: [
						MiniCssExtractPlugin.loader,
						{ loader: 'css-loader', options: { url: false } },
						'postcss-loader',
						{
							loader: 'less-loader',
							options: {
								javascriptEnabled: true,
								relativeUrls: false
							}
						}
					]
				}
			]
		},
		plugins: [
			preCleanUpPlugin,
			postCleanUpPlugin,
			concatJsModules,
			concatJs,
			concatCssDependencies,
			concatCss,
			concatDarkCss,
			extractSass,
			copyPlugin,
			htmlPlugin
			// new UglifyJsPlugin({
			// 	uglifyOptions: {
			// 		mangle: false,
			// 		compress: false,
			// 		keep_fnames: true,
			// 		keep_classnames: true,
			// 		output: {
			// 			ascii_only: false,
			// 			beautify: false,
			// 			bracketize: false,
			// 			comments: false,
			// 			ecma: 5,
			// 			inline_script: false,
			// 			keep_quoted_props: false,
			// 			preamble: null,
			// 			quote_keys: false,
			// 			quote_style: 0,
			// 			wrap_iife: false
			// 		}
			// 	}
			// })
		]
	};

	return config;
};
