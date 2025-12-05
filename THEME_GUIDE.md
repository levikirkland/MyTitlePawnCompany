# Title Pawn Company - Global Theme Guide

## Overview
All styling is now consolidated into a single `theme.css` file. No page-specific CSS files are needed.

## File Structure
```
wwwroot/css/
??? site.css          (Bootstrap overrides & body background)
??? theme.css         (All application styling)
```

## Global Settings

### Container Width
- **Max-width**: 1600px
- **Padding**: 40px (20px on mobile)
- **Centered**: All pages

### Color Palette
- **Gray Scale**: #fafafa to #111827
- **Success**: #4b5563 (Dark gray-green)
- **Warning**: #92400e (Dark amber)
- **Danger**: #7f1d1d (Dark red-gray)

### Font Sizes
- **Body text**: 13px (weight: 500)
- **Table text**: 12px (weight: 500)
- **Labels**: 11px (weight: 600)
- **Headers**: 24px (weight: 700)
- **Buttons**: 12px (weight: 600)

### Card Styling
- **Background**: #fafafa (soft gray)
- **Card Header**: #f3f4f6
- **Padding**: 14px 16px (compact)
- **Border**: 1px solid #d1d5db
- **Margin**: 16px bottom

### Buttons
All buttons use the dark monochromatic theme:
- `.btn-primary` - Darkest (#1f2937)
- `.btn-secondary` - Light gray (#e5e7eb)
- `.btn-success` - Dark gray (#4b5563)
- `.btn-info` - Medium gray (#6b7280)
- `.btn-warning` - Dark amber (#92400e)
- `.btn-danger` - Dark red (#7f1d1d)

## Usage

### Page Headers
Use `.page-header` class for consistent dark headers:
```html
<div class="page-header">
    <div class="container-fluid">
        <h1>Page Title</h1>
    </div>
</div>
```

### Content Container
Wrap all page content in `.container-fluid`:
```html
<div class="container-fluid">
    <!-- Your content -->
</div>
```

### Grid Layout
Use `.compact-grid` for responsive multi-column layouts:
```html
<div class="compact-grid">
    <div class="card">...</div>
    <div class="card">...</div>
    <div class="card">...</div>
</div>
```
- **Desktop**: Auto-fit columns (min 280px)
- **1200px**: 2 columns
- **768px**: 1 column

### Forms
All form elements automatically styled:
- Inputs: #fafafa background, turns white on focus
- Labels: 11px uppercase
- Font weight: 500

### Tables
Tables automatically styled with:
- Soft gray background (#fafafa)
- Alternating row colors on hover
- Compact padding (8px 10px)

## Benefits

? **Consistent styling** across all pages
? **No page-specific CSS** needed
? **Soft backgrounds** reduce eye strain
? **Medium font weight** improves readability
? **Dark monochromatic theme** professional appearance
? **Responsive** works on all screen sizes
? **Easy maintenance** single source of truth

## Adding New Pages

Simply use standard HTML elements and Bootstrap classes. The theme automatically applies:
- Cards
- Buttons
- Forms
- Tables
- Alerts
- Badges

No additional CSS required!
