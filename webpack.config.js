// Adapted from: https://github.com/fable-compiler/webpack-config-template

const CONFIG = {
    indexHtmlTemplate: './src/index.html',
    fsharpEntry: './src/App.fsproj',
    cssEntry: './src/scss/main.scss',
    outputDir: './output',
    assetsDir: './public',
    babel: {
        presets: [
            ["@babel/preset-env", {
                "modules": false,
                "useBuiltIns": "usage",
                // This saves around 4KB in minified bundle (not gzipped)
                // "loose": true,
            }]
        ],
    }
}

const path = require("path");
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const WorkboxPlugin = require('workbox-webpack-plugin');

module.exports = function (_,opts) {
  const isProduction = opts.mode === "production";
  return {
    // If you only need to support modern browsers, you can remove @babel/polyfill
    entry: {
        app: ["@babel/polyfill", CONFIG.fsharpEntry, CONFIG.cssEntry]
    },
    // Add a hash to the output file name in production
    // to prevent browser caching if code changes
    output: {
        path: path.join(__dirname, CONFIG.outputDir),
        filename: isProduction ? '[name].[hash].js' : '[name].js'
    },
    mode: isProduction ? "production" : "development",
    devtool: isProduction ? "source-map" : "eval",
    // - HtmlWebpackPlugin: Allows us to use a template for the index.html page
    //   and automatically injects <script> or <link> tags for generated bundles.
    // - MiniCssExtractPlugin: Extracts CSS from bundle to a different file
    // - CopyWebpackPlugin: Copies static assets to output directory
    plugins: [
        new HtmlWebpackPlugin({
            filename: 'index.html',
            template: CONFIG.indexHtmlTemplate
        }),
        new MiniCssExtractPlugin({
            filename: 'style.css'
        }),
        new CopyWebpackPlugin([{
            from: CONFIG.assetsDir
        }]),
        new WorkboxPlugin.GenerateSW({
            // these options encourage the ServiceWorkers to get in there fast
            // and not allow any straggling "old" SWs to hang around
            clientsClaim: true,
            skipWaiting: true
        })
    ],
    // - fable-loader: transforms F# into JS
    // - babel-loader: transforms JS to old syntax (compatible with old browsers)
    // - sass-loaders: transforms SASS/SCSS into JS
    // - file-loader: Moves files referenced in the code (fonts, images) into output folder
    module: {
        rules: [
            {
                test: /\.fs(x|proj)?$/,
                use: "fable-loader"
            },
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: CONFIG.babel
                },
            },
            {
                test: /\.(sass|scss|css)$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    'css-loader',
                    'sass-loader',
                ],
            },
            {
                test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/,
                use: ["file-loader"]
            }
        ]
    }
  };
}