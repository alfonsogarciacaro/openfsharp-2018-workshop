// Adapted from: https://github.com/fable-compiler/webpack-config-template

const CONFIG = {
    indexHtmlTemplate: './src/index.html',
    fsharpEntry: './src/App/App.fsproj',
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

module.exports = {
    // If you only need to support modern browsers, you can remove @babel/polyfill
    entry: {
        app: ["@babel/polyfill", CONFIG.fsharpEntry, CONFIG.cssEntry]
    },
    output: {
        path: path.join(__dirname, CONFIG.outputDir),
        filename: '[name].js'
    },
    devtool: "source-map",
    externals: {
        "react": "var React",
        "react-dom": "var ReactDOM",
    },
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
            from: CONFIG.assetsDir,
        }], {
            // This is necessary so WorkboxPlugin includes assets in watch compilations
            copyUnmodified: true
        }),
        new WorkboxPlugin.GenerateSW({
            // these options encourage the ServiceWorkers to get in there fast
            // and not allow any straggling "old" SWs to hang around
            clientsClaim: true,
            skipWaiting: true,
        })
    ],
    // - fable-loader: transforms F# into JS
    // - babel-loader: transforms JS to old syntax (compatible with old browsers)
    // - sass-loaders: transforms SASS/SCSS into JS
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
            }
        ]
    }
};
