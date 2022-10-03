module.exports = function override(config, env) {
  let loaders = config.resolve;
  loaders.fallback = {
    "os": require.resolve("os-browserify/browser"),
  }
  return config;
}
