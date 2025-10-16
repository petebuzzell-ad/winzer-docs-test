module.exports = {
  env: {
    browser: true,
    node: true
  },
  extends: ["eslint:recommended"],
  rules: {
    "no-console": ["error", { allow: ["warn", "error"] }],
    "no-unused-vars": "off", // needed since preact causes lots of issues with this. Also cannot extend preact due to use of legacy decorators in snap
  },
  parser: "@babel/eslint-parser",
};
