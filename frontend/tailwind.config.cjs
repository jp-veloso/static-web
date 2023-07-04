/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{ts,tsx}"
  ],
  theme: {
    extend: {
      backgroundImage: {
        'details-card': "url('./assets/dragon-scale.svg')"

      },
      colors: {
        'granto-gray': '#f0f0f0',
        'granto-darkgray':'#b7b7b7'
      }
    },
  },
  plugins: [/* require("@tailwindcss/forms") */],
}
