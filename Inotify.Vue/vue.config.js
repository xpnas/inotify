'use strict'
const path = require('path')
const defaultSettings = require('./src/settings.js')

function resolve(dir) {
  return path.join(__dirname, dir)
}

const name = defaultSettings.title || 'Inotify'
const port = process.env.port || process.env.npm_config_port || 9000

const axiosV = require('axios/package.json').version
const echartsV = require('echarts/package.json').version
const elementV = require('element-ui/package.json').version
const jscookieV = require('js-cookie/package.json').version
const normalizeV = require('normalize.css/package.json').version
const vueV = require('vue/package.json').version
const routerV = require('vue-router/package.json').version
const vuexV = require('vuex/package.json').version
const cookieV = require('js-cookie/package.json').version
const nprogressV = require('nprogress/package.json').version
const momentV = require('moment/package.json').version
const cdn = {
  externals: {
    axios: 'axios',
    echarts: 'echarts',
    'element-ui': 'ELEMENT',
    moment: 'moment',
    locale: 'locale',
    vue: 'Vue',
    vuex: 'Vuex',
    'vue-router': 'VueRouter'
  },
  css: [`https://cdnjs.cloudflare.com/ajax/libs/element-ui/${elementV}/theme-chalk/index.css`, `https://cdnjs.cloudflare.com/ajax/libs/nprogress/${nprogressV}/nprogress.min.css`, `https://lib.baomitu.com/normalize/${normalizeV}/normalize.css`],
  js: [
    `https://cdnjs.cloudflare.com/ajax/libs/vue/${vueV}/vue.min.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/vuex/${vuexV}/vuex.min.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/js-cookie/${jscookieV}/js.cookie.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/element-ui/${elementV}/index.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/axios/${axiosV}/axios.min.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.29.1/moment.min.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/echarts/4.9.0-rc.1/echarts.min.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/vue-router/${routerV}/vue-router.min.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/element-ui/${elementV}/locale/zh-CN.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/js-cookie/${cookieV}/js.cookie.min.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/nprogress/${nprogressV}/nprogress.min.js`,
    `https://cdnjs.cloudflare.com/ajax/libs/qrcodejs/1.0.0/qrcode.js`
  ]
}

module.exports = {
  publicPath: '/',
  outputDir: '../Inotify/wwwroot',
  assetsDir: 'static',
  lintOnSave: process.env.NODE_ENV === 'development',
  productionSourceMap: false,
  devServer: {
    port: port,
    open: true,
    overlay: {
      warnings: false,
      errors: true
    },
    before: require('./mock/mock-server.js')
  },
  configureWebpack: {
    resolve: {
      alias: {
        '@': resolve('src')
      }
    },
    devtool: 'source-map',
    performance: {
      hints: 'warning',
      hints: 'error',
      hints: false,
      maxAssetSize: 200000,
      maxEntrypointSize: 400000,
      assetFilter: function(assetFilename) {
        return assetFilename.endsWith('.css') || assetFilename.endsWith('.js')
      }
    },
    externals: cdn.externals
  },
  chainWebpack(config) {
    config.plugin('html').tap(args => {
      args[0].cdn = cdn
      return args
    })

    config.plugin('preload').tap(() => [
      {
        rel: 'preload',
        // to ignore runtime.js
        // https://github.com/vuejs/vue-cli/blob/dev/packages/@vue/cli-service/lib/config/app.js#L171
        fileBlacklist: [/\.map$/, /hot-update\.js$/, /runtime\..*\.js$/],
        include: 'initial'
      }
    ])

    config.plugins.delete('prefetch')
    config.module
      .rule('svg')
      .exclude.add(resolve('src/icons'))
      .end()
    config.module
      .rule('icons')
      .test(/\.svg$/)
      .include.add(resolve('src/icons'))
      .end()
      .use('svg-sprite-loader')
      .loader('svg-sprite-loader')
      .options({
        symbolId: 'icon-[name]'
      })
      .end()

    config.when(process.env.NODE_ENV !== 'development', config => {
      config
        .plugin('ScriptExtHtmlWebpackPlugin')
        .after('html')
        .use('script-ext-html-webpack-plugin', [
          {
            // `runtime` must same as runtimeChunk name. default is `runtime`
            inline: /runtime\..*\.js$/
          }
        ])
        .end()
      config.optimization.splitChunks({
        chunks: 'all',
        cacheGroups: {
          libs: {
            name: 'chunk-libs',
            test: /[\\/]node_modules[\\/]/,
            priority: 10,
            chunks: 'initial' // only package third parties that are initially dependent
          },
          elementUI: {
            name: 'chunk-elementUI', // split elementUI into a single package
            priority: 20, // the weight needs to be larger than libs and app or it will be packaged into libs or app
            test: /[\\/]node_modules[\\/]_?element-ui(.*)/ // in order to adapt to cnpm
          },
          commons: {
            name: 'chunk-commons',
            test: resolve('src/components'), // can customize your rules
            minChunks: 3, //  minimum common number
            priority: 5,
            reuseExistingChunk: true
          }
        }
      })
      // https:// webpack.js.org/configuration/optimization/#optimizationruntimechunk
      config.optimization.runtimeChunk('single')
    })
  }
}
