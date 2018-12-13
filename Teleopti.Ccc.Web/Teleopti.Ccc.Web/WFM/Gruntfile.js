const glob = require('glob');
const file = require('fs');
const { join } = require('path');
const { minify } = require('html-minifier');

function templateCache() {
	const readFile = path => file.readFileSync(path, { encoding: 'utf8' });
	const writeFile = (path, data) => file.writeFileSync(path, data, { encoding: 'utf8' });
	const files = glob.sync('@(html|app)/**/*.html');

	const templates = files.map(filename => {
		const html = readFile(join(__dirname, filename));
		const minifiedHtml = minify(html, {
			collapseWhitespace: true,
			removeComments: true,
			caseSensitive: true
		});
		const escapedHtml = minifiedHtml.replace(/(\"|\\.)/gm, '\\$1').replace(/(\r|\n)/gm, '\\n');
		return `$templateCache.put("${filename}", "${escapedHtml}")`;
	});

	let templateCache = `angular.module('wfm.templates',[]).run(['$templateCache', function($templateCache) {${templates}}]);`.trim();
	writeFile(join(__dirname, 'dist', 'templates.js'), templateCache);
}

module.exports = function(grunt) {
	const isDev = grunt.option('development') || false;
	const isProd = !isDev;

	const watch = {
		indexTemplates: {
			files: ['index.tpl.html', 'index_desktop_client.tpl.html', 'src/reset_password.tpl.html'],
			tasks: ['processhtml']
		},
		angularjsTemplates: {
			files: ['src/index.tpl.html', 'app/**/*.html', 'html/**/*.html'],
			tasks: ['templateCache']
		},
		angularjsCode: {
			files: ['app/**/*.js'],
			tasks: ['concat:concatJsWfm']
		},
		antThemes: {
			files: ['src/themes/*.less'],
			tasks: ['less']
		}
	};

	const less = {
		themes: {
			options: {
				compress: isProd,
				javascriptEnabled: true,
				relativeUrls: false
			},
			files: {
				'dist/ant_classic.css': 'src/themes/ant_classic.less',
				'dist/ant_dark.css': 'src/themes/ant_dark.less'
			}
		}
	};

	const processhtml = {
		browser: {
			files: {
				'index.html': ['src/index.tpl.html']
			},
			options: {
				process: true,
				environment: isDev ? 'dev' : 'prod'
			}
		},
		desktop: {
			files: {
				'index_desktop_client.html': ['index_desktop_client.tpl.html']
			},
			options: {
				process: true,
				environment: isDev ? 'dev' : 'prod'
			}
		},
		reset_password: {
			files: {
				'reset_password.html': ['src/reset_password.tpl.html']
			},
			options: {
				process: true,
				environment: isDev ? 'dev' : 'prod'
			}
		}
	};

	const concatJsDependencies = {
		src: [
			'node_modules/interactjs/dist/interact.min.js',
			'node_modules/angular/angular.min.js',
			'node_modules/angular-ui-router/release/angular-ui-router.min.js',
			'node_modules/angular-resizable/angular-resizable.min.js',
			'node_modules/angular-resource/angular-resource.min.js',
			'node_modules/angular-sanitize/angular-sanitize.min.js',
			'node_modules/angular-translate/dist/angular-translate.min.js',
			'node_modules/angular-translate/dist/angular-translate-loader-url/angular-translate-loader-url.min.js',
			'node_modules/angular-dynamic-locale/tmhDynamicLocale.min.js',
			'node_modules/moment/min/moment-with-locales.min.js',
			'node_modules/moment-timezone/builds/moment-timezone-with-data.min.js',
			'node_modules/angular-moment/angular-moment.min.js',
			'node_modules/ng-file-upload/dist/ng-file-upload-shim.min.js',
			'node_modules/ng-file-upload/dist/ng-file-upload.min.js',
			'node_modules/angular-ui-grid/ui-grid.min.js',
			'node_modules/angular-ui-indeterminate/dist/indeterminate.min.js',
			'node_modules/ngstorage/ngStorage.min.js',
			'node_modules/default-passive-events/dist/index.js',
			'node_modules/angular-ui-tree/dist/angular-ui-tree.min.js',
			'node_modules/angular-aria/angular-aria.min.js',
			'node_modules/angular-animate/angular-animate.min.js',
			'node_modules/angular-gantt/assets/angular-gantt.js',
			'node_modules/angular-gantt/assets/angular-gantt-plugins.js',
			'node_modules/angular-gantt/assets/angular-gantt-table-plugin.js',
			'node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.js',
			'node_modules/filesaver.js/FileSaver.min.js',
			'node_modules/jquery/dist/jquery.min.js',
			'node_modules/hammerjs/hammer.min.js',
			'node_modules/angular-material/angular-material.min.js',
			'node_modules/angular-ui-bootstrap/dist/ui-bootstrap-tpls.js',
			'node_modules/ng-image-input-with-preview/dist/ng-image-input-with-preview.js',
			'vendor/fabricjs/fabric.min.js',
			'vendor/fabricjs/fabricjs_viewport.js',

			//To get around the problem of getting a d3 global variable this minified js file was made using
			//rollup and uglify "npm i -g rollup uglify".
			//In case of a new version: Create a file using rollup with the -f iife flag. Edit that one so it uses "window" as an input parameter
			//to the main function, it had "undefined" when we did it, then use uglify to minify the package and place it in the vendor/d3 folder.
			'vendor/d3/d3.min.js',
			'vendor/powerbi/powerbi.bundle.min.js', //same as d3 bundle
			'node_modules/c3/c3.min.js',
			'node_modules/c3-angular/c3-angular.min.js',
			'vendor/ui-bootstrap-custom-build/datepicker.directive.ext.js',
			'vendor/ui-bootstrap-custom-build/timepicker.directive.ext.js',
			'node_modules/angular-dialog-service/dist/dialogs.min.js',
			'node_modules/angular-dialog-service/dist/dialogs-default-translations.min.js',
			'vendor/angular-bootstrap-persian-datepicker-master/persiandate.js',
			'vendor/angular-bootstrap-persian-datepicker-master/persian-datepicker-tpls.js',
			'../Content/signalr/jquery.signalR-2.3.0.js',
			'../Content/signalr/broker-hubs.js',
			'node_modules/lodash/lodash.min.js',
			'node_modules/requirejs/require.js'
		],
		dest: 'dist/resources/modules.js'
	};

	const concatJsWfm = {
		options: {
			separator: ';' + grunt.util.linefeed,
			sourceMap: true
		},
		src: [
			'app/**/*.js',
			'!app/**/*.spec.js',
			'!app/**/*.fake.js',
			'!app/**/*.fortest.js',
			'!app/app_desktop_client.js'
		],
		dest: 'dist/main.js'
	};

	const addD3 = {
		src: ['node_modules/d3/dist/d3.min.js'],
		dest: 'dist/resources/d3.js'
	};

	// TODO: Add desktop concat

	const concatCssDependencies = {
		src: [
			'node_modules/c3/c3.min.css',
			'node_modules/angular-resizable/src/angular-resizable.css',
			'node_modules/angular-ui-tree/source/angular-ui-tree.css',
			'node_modules/angular-ui-grid/ui-grid.css',
			'node_modules/angular-material/angular-material.css',
			'node_modules/angular-gantt/assets/angular-gantt.css',
			'node_modules/angular-gantt/assets/angular-gantt-plugins.css',
			'node_modules/angular-gantt/assets/angular-gantt-table-plugin.css',
			'node_modules/angular-gantt/assets/angular-gantt-tooltips-plugin.css'
		],
		dest: 'dist/dependencies.css'
	};

	const uglifyOptions = {
		sourceMap: false,
		beautify: false,
		mangle: false
	};
	const uglify = {
		browser: {
			files: {
				'dist/main.js': [
					'app/**/*.js',
					'!app/**/*.spec.js',
					'!app/**/*.fake.js',
					'!app/**/*.fortest.js',
					'!app/app_desktop_client.js'
				]
			},
			options: uglifyOptions
		},
		desktop: {
			files: {
				'dist/mainForDesktop.js': [
					'app/**/*.js',
					'!app/**/*.spec.js',
					'!app/**/*.fake.js',
					'!app/**/*.fortest.js',
					'!app/app.js'
				]
			},
			options: uglifyOptions
		}
	};

	grunt.initConfig({
		watch,
		templateCache,
		processhtml,
		less,
		concat: {
			concatJsDependencies,
			concatJsWfm,
			concatCssDependencies
		},
		uglify,
		msbuild: {
			rebuild: {
				src: ['../../../CruiseControl.sln'],
				options: {
					projectConfiguration: 'Debug',
					targets: ['Rebuild'],
					version: 15.0,
					maxCpuCount: null,
					buildParameters: {
						WarningLevel: 2
					},
					verbosity: 'normal'
				}
			},
			build: {
				src: ['../../../CruiseControl.sln'],
				options: {
					projectConfiguration: 'Debug',
					targets: ['build'],
					version: 15.0,
					maxCpuCount: null,
					buildParameters: {
						WarningLevel: 2
					},
					verbosity: 'normal'
				}
			},
			buildWeb: {
				src: ['../Teleopti.Ccc.Web.csproj'],
				options: {
					projectConfiguration: 'Debug',
					targets: ['build'],
					version: 15.0,
					maxCpuCount: null,
					buildParameters: {
						WarningLevel: 2
					},
					verbosity: 'normal'
				}
			}
		},

		copy: {
			locales: {
				files: [
					{
						expand: true,
						cwd: './node_modules/angular-i18n/',
						src: ['angular-locale_*.js'],
						dest: 'dist/angular-i18n/'
					}
				]
			},
			sourceMaps: {
				files: [
					// includes files within path
					{
						expand: true,
						cwd: 'vendor',
						flatten: true,
						src: ['*/*.map'],
						dest: 'dist/resources',
						filter: 'isFile'
					}
				]
			},
			extras: {
				files: [
					{
						expand: true,
						cwd: 'node_modules/angular-ui-grid',
						src: ['*.ttf', '*.woff', '*.eot'],
						dest: 'dist/',
						filter: 'isFile'
					}
				]
			},
			bootstrap: {
				files: [
					{
						expand: true,
						cwd: 'node_modules/bootstrap/fonts',
						src: ['*.ttf', '*.woff', '*.eot'],
						dest: 'dist/fonts',
						filter: 'isFile'
					}
				]
			},
			images: {
				files: [
					{
						expand: true,
						cwd: 'app/seatManagement/images',
						src: ['*.svg', '*.jpg', '*.png'],
						dest: 'dist/images',
						filter: 'isFile'
					}
				]
			}
		}
	});

	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-contrib-copy');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-contrib-less');
	grunt.loadNpmTasks('grunt-msbuild');
	grunt.loadNpmTasks('grunt-angular-templates');
	grunt.loadNpmTasks('grunt-processhtml');

	grunt.registerTask('templateCache', templateCache);
	grunt.registerTask('devBuild', ['concat', 'copy', 'templateCache', 'less', 'processhtml']);
	grunt.registerTask('devWatch', ['devBuild', 'watch']);
	grunt.registerTask('prodBuild', ['concat', 'copy', 'templateCache', 'less', 'processhtml', 'uglify']);

	grunt.registerTask('default', ['devWatch']);

	grunt.registerTask('build', ['msbuild:build']); // build the solution
	grunt.registerTask('buildWeb', ['msbuild:buildWeb']); // build the web project
	grunt.registerTask('rebuild', ['msbuild:rebuild']); // rebuild the solution
};
