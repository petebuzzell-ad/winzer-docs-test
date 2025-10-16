# Deployment Guide

## Test Deployment Steps

### 1. Create GitHub Repository
```bash
# Create new repo: winzer-documentation
# Make sure it's public for GitHub Pages
```

### 2. Push Code
```bash
cd /Users/pete/dev/shopify/onesource/winzer-docs-test
git init
git add .
git commit -m "Initial Jekyll documentation setup"
git branch -M main
git remote add origin https://github.com/petebuzzell-ad/winzer-documentation.git
git push -u origin main
```

### 3. Enable GitHub Pages
1. Go to repository Settings
2. Scroll to "Pages" section
3. Source: "Deploy from a branch"
4. Branch: "main" / "/ (root)"
5. Click "Save"

### 4. Test URLs
- **Current HTML docs:** `https://petebuzzell-ad.github.io/winzer/`
- **New documentation:** `https://petebuzzell-ad.github.io/winzer-documentation/`

## Comparison Testing

### What to Test
1. **Navigation** - All internal links work
2. **Styling** - Arcadia branding preserved
3. **Content** - All documentation present
4. **Mobile** - Responsive design
5. **Performance** - Load times

### Key Differences
- **Jekyll** - Better navigation, search, maintainability
- **HTML** - Simpler, no build process needed

## Next Steps

If Jekyll version works well:
1. Update `_config.yml` with final URL
2. Replace current HTML docs
3. Update Winzer with new URL
4. Archive old HTML files

If issues found:
1. Fix in test repo
2. Re-test before migration
3. Keep HTML as fallback
