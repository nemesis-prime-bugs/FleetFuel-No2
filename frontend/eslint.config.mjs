import { fixupPluginRules } from "@eslint/compat";
import { defineConfig } from "eslint/config";
import tseslint from "typescript-eslint";
import nextPlugin from "@next/eslint-plugin-next";

const eslintConfig = defineConfig([
  {
    files: ["**/*.{ts,tsx}"],
    plugins: {
      "@next/next": fixupPluginRules(nextPlugin),
      "@typescript-eslint": tseslint.plugin,
    },
    languageOptions: {
      parser: tseslint.parser,
    },
    rules: {
      ...nextPlugin.configs.recommended.rules,
      ...nextPlugin.configs["core-web-vitals"].rules,
      "@typescript-eslint/no-explicit-any": "warn",
      "@typescript-eslint/no-unused-vars": ["warn", { argsIgnorePattern: "^_" }],
    },
  },
]);

export default eslintConfig;
