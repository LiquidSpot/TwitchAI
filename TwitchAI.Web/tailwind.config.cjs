/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: '#6E59A5',
        accent: '#8EE3F5',
        glass: '#ffffff'
      },
      backdropBlur: {
        xs: '2px'
      }
    },
  },
  plugins: [],
}


