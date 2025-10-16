const { merge } = require('webpack-merge');
const common = require('./webpack.common.js');
const path = require('path');
const childProcess = require('child_process');
const branchName = childProcess.execSync('git rev-parse --abbrev-ref HEAD').toString().trim();

sitename = process.argv[2].replace('--env=', '');

module.exports = merge(common, {
	mode: 'production',
	entry: './src/index.js',
	output: {
		path: path.resolve(__dirname, `../../sites/${sitename}/assets`),
        filename: 'searchspring.bundle.js',
		chunkFilename: 'searchspring.bundle.chunk.[fullhash:8].[id].js',
		chunkLoadingGlobal: `${branchName}BundleChunks`,
	},
	target: 'browserslist:modern',
	module: {
		rules: [
			{
				test: /\.(js|jsx)$/,
				use: {
					loader: 'babel-loader',
					options: {
						presets: [
							[
								'@babel/preset-env',
								{
									browserslistEnv: 'modern',
								},
							],
						],
					},
				},
			},
		],
	}
});
