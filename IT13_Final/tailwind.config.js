/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.razor",
    "./**/*.cshtml",
    "./wwwroot/**/*.html",
    "./Components/**/*.razor",
    "./Components/**/*.cshtml",
  ],
  theme: {
    extend: {
      colors: {
        'light-blue-gray': '#A2B9CD',
        'platinum': '#E0E0E0',
        'uranian-blue': '#A7CCED',
        'yinmn-blue': '#304D6D',
        'air-blue': '#82A0BC',
      },
    },
  },
  plugins: [],
}

