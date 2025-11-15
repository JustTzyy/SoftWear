# Tailwind CSS Setup Instructions

Tailwind CSS has been configured for your Blazor MAUI project! Follow these steps to complete the setup:

## Prerequisites

1. **Install Node.js** (if not already installed):
   - Download from: https://nodejs.org/
   - Install the LTS version
   - This will also install npm (Node Package Manager)

## Setup Steps

1. **Install Dependencies**:
   ```bash
   npm install
   ```

2. **Build Tailwind CSS**:
   ```bash
   npm run build-css
   ```
   This will generate `wwwroot/tailwind.css` from `wwwroot/tailwind-input.css`

3. **For Development (Watch Mode)**:
   ```bash
   npm run watch-css
   ```
   This will automatically rebuild Tailwind CSS whenever you make changes to your `.razor` files.

## Usage

After building, Tailwind CSS classes are available in all your `.razor` files. For example:

```razor
<div class="flex items-center justify-center p-4 bg-blue-500 text-white">
    Hello Tailwind!
</div>
```

## Files Created

- `package.json` - npm configuration with Tailwind dependencies
- `tailwind.config.js` - Tailwind configuration (scans `.razor` files)
- `wwwroot/tailwind-input.css` - Tailwind directives file
- `wwwroot/tailwind.css` - Generated CSS file (created after build)

## Important Notes

- Run `npm run build-css` before building your .NET project to ensure Tailwind CSS is up to date
- The `tailwind.css` file is generated and should be included in your project
- Tailwind scans all `.razor` files in your project for class names
- Your existing `app.css` and Bootstrap styles will still work alongside Tailwind

## Troubleshooting

If you encounter issues:
1. Make sure Node.js and npm are installed: `node --version` and `npm --version`
2. Delete `node_modules` folder and run `npm install` again
3. Check that `tailwind.config.js` includes the correct content paths

