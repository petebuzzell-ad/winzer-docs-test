# Winzer eCommerce Platform - Developer Handoff Guide

## Overview

This repository contains the complete Winzer eCommerce platform including documentation, source code, and deployment configurations. The platform serves three brands: Winzer, OneSource Supply Co, and FastServ Supply.

## Repository Structure

```
winzer-docs-test/
├── Documentation (HTML + Markdown)
│   ├── winzer-documentation.html/md          # Main platform documentation
│   ├── winzer-middleware-documentation.html/md # AWS middleware system
│   ├── winzer-product-data-map.html/md       # Oracle PIM to Shopify mapping
│   ├── winzer-searchspring-documentation.html/md # SearchSpring configuration
│   └── winzer-shipperhq-documentation.html/md # ShipperHQ shipping rules
├── Source Code
│   ├── code/winzer-main/                     # Main codebase
│   │   ├── dawn/                            # Shopify Dawn theme customizations
│   │   ├── searchspring/winzer/             # SearchSpring React app
│   │   └── sites/                           # Multi-brand configurations
│   └── code/winzer-middleware/              # .NET 6 AWS middleware
└── Assets
    ├── arcadia-style.css                    # Arcadia Digital branding
    └── index.html                           # Documentation hub
```

## Quick Start

### 1. Access Documentation
- **Live Site**: https://petebuzzell-ad.github.io/winzer-documentation/
- **Local**: Open `index.html` in any browser

### 2. Source Code Access
- **Main Codebase**: `code/winzer-main/` - Complete Shopify theme and SearchSpring app
- **Middleware**: `code/winzer-middleware/` - .NET 6 AWS services

## Platform Architecture

### Frontend (Shopify)
- **Theme**: Customized Dawn theme with multi-brand support
- **SearchSpring**: React app that generates `bundle.js` for Shopify templates
- **Brands**: Winzer, OneSource Supply Co, FastServ Supply

### Backend (AWS)
- **Middleware**: .NET 6 Lambda functions and ECS Fargate tasks
- **Data Source**: Oracle PIM system
- **Integration**: Shopify Admin API for product/order management

### Data Flow
1. Oracle PIM → AWS Middleware → Shopify Admin API
2. SearchSpring React app → Bundle generation → Shopify theme
3. Multi-brand configurations → Site-specific customizations

## Development Setup

### Main Codebase (`code/winzer-main/`)

```bash
cd code/winzer-main
npm install
# or
yarn install

# SearchSpring React app
cd searchspring/winzer
npm install
npm run build  # Generates bundle.js for Shopify
```

**Key Files:**
- `package.json` - Main dependencies and scripts
- `searchspring/winzer/` - React app for SearchSpring integration
- `sites/` - Brand-specific configurations
- `scripts/` - Deployment and build utilities

### Middleware (`code/winzer-middleware/`)

```bash
cd code/winzer-middleware
# Requires .NET 6 SDK
dotnet restore
dotnet build
```

**Key Projects:**
- `Winzer.ShopifyMiddleware.AWS/` - Lambda functions
- `Winzer.Core/` - Core business logic
- `Winzer.Repo/` - Data access layer
- Console apps for product/price feeds

## Deployment

### Shopify Theme
- Use `scripts/deploy.js` for theme deployment
- SearchSpring bundle must be built and deployed to theme assets

### AWS Middleware
- Lambda functions deploy via AWS CLI or Bitbucket Pipelines
- ECS Fargate tasks for long-running processes
- See `build/` directory for deployment scripts

## Key Integrations

### SearchSpring
- React app generates search/filter functionality
- Bundle.js gets injected into Shopify theme
- Configuration in `winzer-searchspring-documentation.html`

### ShipperHQ
- Product-based shipping rules
- State-specific restrictions
- Hashbang-delimited metafields

### Oracle PIM
- Product data source
- Transformed through middleware
- Mapped to Shopify metafields

## Maintenance Notes

### No Active Developer
- This is a handoff repository
- All source code is included for continuity
- Documentation is comprehensive and up-to-date

### Critical Files
- `DatabaseCreationScript.sql` - Database schema
- `Resources/` - Sample CSV files for data mapping
- `BuildAndDeployToProduction.txt` - Production deployment notes

### Brand Management
- Each brand has separate configurations in `sites/`
- Shared assets in `sites/_shared/`
- Brand-specific customizations in respective directories

## Support & Contact

**Primary Contact**: pete.buzzell@arcadiadigital.com

**Documentation**: All technical details are in the HTML documentation files
**Source Code**: Complete codebase is included in `/code` directory
**Deployment**: See individual README files in each code directory

## Next Steps for New Developer

1. **Review Documentation**: Start with `winzer-documentation.html`
2. **Understand Architecture**: Read `winzer-middleware-documentation.html`
3. **Set Up Development**: Follow setup instructions in code READMEs
4. **Test Deployments**: Use staging environments first
5. **Familiarize with Data Flow**: Study product data mapping documentation

---

*This handoff package provides everything needed to maintain and extend the Winzer eCommerce platform.*
