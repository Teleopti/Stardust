const webpack = require('webpack');
const path = require('path');
const UglifyJSPlugin = require('uglifyjs-webpack-plugin');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

module.exports = {
	entry: {
		app: './app/appwp.js'
	},

	output: {
		filename: '[name].[chunkhash].js',
		chunkFilename: '[name].[chunkhash].js',
		path: path.resolve(__dirname, 'distwp')
	},

	resolve: {
		extensions: ['.ts', '.tsx', '.js', '.jsx']
	},

	module: {
		rules: [
			{
				test: /\.js$/,
				exclude: /node_modules/,
				loader: 'awesome-typescript-loader',

				options: {
					presets: ['env']
				}
			}
		]
	},

	plugins: [new UglifyJSPlugin(), new ExtractTextPlugin('style.css')]
};
