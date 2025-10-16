# Winzer eCommerce Platform Onboarding Documentation

---
## Table of Contents
- [Executive Summary](#executive-summary)
- [Platform Architecture](#platform-architecture)
- [Content Management](#content-management)
- [Product Management](#product-management)
[Product Grouping & Variants](#product-grouping-and-variants)
- [Data Flow & Integration](#data-flow-and-integration)
- [Site Management](#site-management)
- [Custom Elements](#custom-elements)
[SearchSpring Integration](#searchspring-integration)
- [External Integrations & Feeds](#external-integrations-and-feeds)
- [Operations &amp; Runbooks](#operations--runbooks)
- [KPI &amp; Analytics](#kpi--analytics)
- [Documentation Sources](#documentation-sources)
---
## Executive Summary
### Platform Overview
Winzer operates three distinct Shopify Plus stores under a unified Dawn-based architecture:
- **OneSource** - [https://www.onesourcesupplyco.com](https://www.onesourcesupplyco.com) -  (`winzeronesource.myshopify.com`) - Primary DTC store (695 products, 44 blog posts)
- **FastServ** - [https://store.fastservsupplyco.com](https://store.fastservsupplyco.com) - (`winzerfastserv.myshopify.com`) - Fastserv Supply Co B2B store (42,529 products, 3 blog posts)
- **Winzer Corp** - [https://store.winzer.com](https://store.winzer.com) - (`winzercorp.myshopify.com`) - Winzer B2B store (48,718 products, 3 blog posts)
### Key Technical Components
- **Base Theme:** Dawn with heavy customization
- **SearchSpring Integration:** Advanced search, filtering, and recommendations
- **Multi-site Architecture:** Shared components with site-specific overrides
- **Node.js Build System:** Webpack-based development and deployment
- **Metafield System:** 20+ custom metafields for product data and SearchSpring integration
- **AWS Middleware System:** Critical .NET-based integration layer for data synchronization
---
## Platform Architecture
### Multi-Site Structure
```
winzer-main/
├── sites/
│   ├── _shared/          # Core Dawn overrides (shared across all sites)
│   ├── onesource/        # OneSource-specific customizations
│   ├── fastserv/         # FastServ-specific customizations
│   └── corp/             # Winzer Corp-specific customizations
├── searchspring/         # SearchSpring React components
└── scripts/              # Build and deployment scripts
```
### Site-Specific Configurations
**OneSource (Primary DTC):**
- **Domain:** [https://www.onesourcesupplyco.com](https://www.onesourcesupplyco.com)
- **SearchSpring Site ID:** t047mf
- **Apps:** Klaviyo, Elevar, Matrixify
- **Focus:** Content-rich, educational approach
**FastServ (B2B Supply):**
- **Domain:** [https://store.fastservsupplyco.com](https://store.fastservsupplyco.com)
- **SearchSpring Site ID:** wk4j0d
- **Apps:** Elevar, Matrixify
- **Focus:** High-volume product catalog, minimal content
**Winzer Corp (B2B Enterprise):**
- **Domain:** [https://store.winzer.com](https://store.winzer.com)
- **SearchSpring Site ID:** fsqw40
- **Apps:** Elevar, Matrixify
- **Focus:** Enterprise solutions, corporate messaging
### Technology Stack
- **Frontend:** Dawn theme + custom Liquid templates
- **Search:** SearchSpring with React components
- **Build System:** Node.js + Webpack
- **Deployment:** Shopify Theme Kit + GitHub Actions
- **Middleware:** .NET 6 AWS Lambda functions + ECS Fargate
- **Data Integration:** Oracle ERP, SFTP, AWS, S3
- **Monitoring:** Elevar (conversion tracking), UpTimeRobot (uptime monitoring), Calibre (performance monitoring), Google Lighthouse/PSI (performance, accessibility &amp; SEO monitoring)
---
## Content Management
**Content Inventory:**
- **Pages:** 39 pages across all sites
- **Blog Posts:** 44 posts (OneSource), 0 (FastServ), 0 (Corp)
- **Collections:** 1,200+ collections with custom metafields
- **Products:** 15,000+ products with extensive metafield data
### Page Templates &amp; Content Structure
**Template Architecture:**
The Winzer theme uses a modular section-based approach for content management, allowing flexible page layouts through customizable content blocks.
**Core Page Templates:**
**1. Homepage Template - "Main Landing Page" (`index.json`)**
- **Primary Sections:**
`image-banner`: Hero section with custom messaging
- `icon-list`: Category navigation with 12 product categories
- `featured-links`: Promotional content blocks (3-4 featured items)
- `rich-text`: Brand messaging and value propositions
- `image-with-text`: Brand storytelling sections
- `cards`: Partner/vendor logo displays
- `searchspring-recommendations`: Dynamic product recommendations
**2. Standard Page Template - "General Content Page" (`page.json`)**
- **Default Structure:**
`info-accordion`: FAQ-style content blocks (up to 12 accordion items)
- `main-page`: Standard page content with rich text editor
- **Content Blocks:**
Accordion dropdowns with custom headings and content
- Rich text content with HTML support
- Custom styling and color options
**3. Article Template - "Blog Post Page" (`article.json`)**
- **Structure:**
`main-article`: Blog post content with featured image, title, and content
- `featured_image`: Hero image for blog posts
- `title`: Article headline
- `content`: Rich text article body
- `share`: Social sharing functionality
**4. Blog Template - "Blog Listing Page" (`blog.json`)**
- **Structure:**
`main-blog`: Blog listing page with post grid
- **Features:**
Pagination support
- Category filtering
- Search functionality
- Featured post highlighting
**5. Page Template Variants:**
**A. Brand Pages - "Brand Showcase Page" (`page.brands.json`)**
- **Purpose:** Showcase brand partnerships and product lines
- **Structure:** Hero banner + multiple icon-list sections for brand logos
- **Sections Used:** `image-banner`, `icon-list` (multiple instances)
- **Content Entry:** Brand logos, links to brand collections, hero messaging
- **Example:** OneSource brands page with 3M, Gates, Lenox, Loctite, etc.
- **Wrapper Class:** `div.customer-service-page`
**B. About Pages - "Company Information Page" (`page.about.json`)**
- **Purpose:** Company information and value propositions
- **Structure:** Hero + features list + image-with-text sections + testimonials
- **Sections Used:** `image-banner`, `features-list`, `image-with-text`, `info-carousel`
- **Content Entry:** Company stats, mission content, team photos, testimonials
- **Example:** OneSource about page with company stats (65,000+ products, 46+ years)
- **Wrapper Class:** `div.about-us`
**C. Contact Pages - "Contact Information Page" (`page.contact.json`)**
- **Purpose:** Customer service and contact information
- **Structure:** Hero banner + menu bar + customer service section
- **Sections Used:** `image-banner`, `menu-bar`, `customer-service`
- **Content Entry:** Contact forms, service hours, contact information
- **Features:** Contact form integration, service hours display
- **Wrapper Class:** `div.customer-service-page`
**D. FAQ Pages - "Frequently Asked Questions Page" (`page.faqs.json`)**
- **Purpose:** Customer service and frequently asked questions
- **Structure:** Hero + menu bar + collapsible FAQ sections
- **Sections Used:** `image-banner`, `menu-bar`, `customer-service` (with collapsible tabs)
- **Content Entry:** FAQ questions/answers, quick links, policy information
- **Features:** Collapsible accordion interface, quick navigation links
- **Wrapper Class:** `div.faqs-page`
**E. Customer Service Pages - "Customer Support Page" (`page.customer-service.json`)**
- **Purpose:** General customer service information
- **Structure:** Hero + menu bar + customer service content
- **Sections Used:** `image-banner`, `menu-bar`, `customer-service`
- **Content Entry:** Service information, contact details, forms
- **Wrapper Class:** `div.customer-service-page`
**F. Classic Orders Page - "Legacy Order Management Page" (`page.classic_orders.json`)**
- **Purpose:** Legacy order management interface
- **Structure:** Single section for order management
- **Sections Used:** `page-classic-orders`
- **Content Entry:** Order management interface
**G. Style Guide Page - "Design System Documentation Page" (`page.styleguide.json`)**
- **Purpose:** Design system documentation
- **Structure:** Single section for style guide content
- **Sections Used:** `main-page-styleguide`
- **Content Entry:** Design system documentation
**Winzer Corp Page Templates:**
**H. About Page - "Corporate About Page" (`page.about.json`)**
- **Purpose:** Corporate about page with company statistics and values
- **Structure:** Hero + features list + multiple image-with-text sections + testimonial carousel
- **Sections Used:** `image-banner`, `features-list`, `image-with-text`, `info-carousel`, `rich-text`
- **Content Entry:** Company stats (65,000+ products, 46+ years, 100+ brands), mission content, testimonials
- **Features:** Statistics display, testimonial carousel, company values sections
- **Wrapper Class:** `div.about-us`
**I. Become a Franchise Page - "Franchise Opportunity Page" (`page.become-a-franchise.json`)**
- **Purpose:** Franchise opportunity information and application
- **Structure:** Hero + image-with-text sections + testimonial carousel + CTA
- **Sections Used:** `image-banner`, `image-with-text`, `info-carousel`, `rich-text`
- **Content Entry:** Franchise benefits, application process, contact information
- **Features:** Video integration, online application form, franchise benefits
- **Wrapper Class:** `div.about-us`
**J. Careers Page - "Job Opportunities Page" (`page.careers.json`)**
- **Purpose:** Career opportunities and company culture
- **Structure:** Hero + multiple image-with-text sections + benefits + CTA
- **Sections Used:** `image-banner`, `image-with-text`, `rich-text`
- **Content Entry:** Company values (Service-Oriented, Collaborative, Innovative, Responsible), benefits, job listings
- **Features:** Company culture sections, benefits list, job application links
- **Wrapper Class:** `div.about-us`
**K. Our Customers Page - "Industry Solutions Page" (`page.our-customers.json`)**
- **Purpose:** Industry solutions and customer segments
- **Structure:** Hero + collapsible content sections
- **Sections Used:** `image-banner`, `collapsible-content`
- **Content Entry:** Industry-specific solutions (Automotive, Construction, Fleet, Military, etc.)
- **Features:** Collapsible industry sections, brochure downloads, customer application forms
- **Wrapper Class:** `div.about-us`
**L. Resource Center Page - "Document Library Page" (`page.resource-center.json`)**
- **Purpose:** Resource library with catalogs and brochures
- **Structure:** Hero + multiple featured-links sections
- **Sections Used:** `image-banner`, `featured-links` (multiple instances)
- **Content Entry:** Product catalogs, solution brochures, technical documents
- **Features:** Download links, organized by category (Catalogs, Solution Brochures, Product Brochures)
- **Wrapper Class:** `div.about-us`
**M. Why Winzer Page - "Value Proposition Page" (`page.why-winzer.json`)**
- **Purpose:** Value proposition and competitive advantages
- **Structure:** Hero + multiple image-with-text sections + CTA
- **Sections Used:** `image-banner`, `image-with-text`, `rich-text`
- **Content Entry:** Quality, expertise, distribution, inventory management benefits
- **Features:** Value proposition sections, customer application forms, product links
- **Wrapper Class:** `div.about-us`
### Content Components &amp; Sections
#### Core Content Sections
**1. Rich Text Section (`rich-text.liquid`)**
- **Purpose:** Flexible text content with multiple block types
- **Block Types:**
`eyebrow`: Small text above headlines
- `heading`: Main headlines with custom sizing (h1-h6, custom headline styles)
- `text`: Body text with multiple style options
- `button`: Call-to-action buttons with various styles
- `logo_list`: Partner/vendor logo displays (up to 5 logos)
- `padding`: Spacing control blocks
- **Customization:**
Background colors and images
- Text alignment and sizing
- Animation effects (fadeIn, fadeInUp, etc.)
- Responsive padding controls
- **Example Usage:** Winzer Corp About page (company stats section), OneSource homepage (brand messaging)
**2. Image Banner Section (`image-banner.liquid`)**
- **Purpose:** Hero sections with background images and overlay content
- **Features:**
Desktop and mobile image support
- Parallax scrolling effects
- Content positioning (9 position options)
- Custom background colors and opacity
- Border and styling options
- **Content Blocks:**
`eyebrow`: Small text above main content
- `heading`: Main headline with custom sizing
- `text`: Description text with rich text support
- `button`: Up to 2 call-to-action buttons
- `image`: Additional image elements
- **Example Usage:** All page templates use this for hero sections (About, Contact, FAQ, Careers, etc.)
**3. Image with Text Section (`image-with-text.liquid`)**
- **Purpose:** Side-by-side image and text content blocks
- **Features:**
Image and text positioning (left/right)
- Video support with thumbnail images
- Custom background colors and layouts
- Responsive image handling
- **Content Blocks:**
`eyebrow`: Small text above main content
- `heading`: Main headline with custom sizing
- `text`: Description text with rich text support
- `button`: Up to 2 call-to-action buttons
- **Example Usage:** Winzer Corp About page (company values sections), OneSource homepage (featured content)
**4. Image with Text Alternate Section (`image-with-text-alternate.liquid`)**
- **Purpose:** Alternative layout for image and text content
- **Features:**
Different styling options from standard image-with-text
- Custom background treatments
- Enhanced visual effects
- **Example Usage:** Special content layouts requiring different styling
#### Grid and Layout Sections
**5. Featured Links Section (`featured-links.liquid`)**
- **Purpose:** Grid-based content blocks for promotions and features
- **Layout Options:**
2-5 column desktop layouts
- Image-on-top or image-on-side layouts
- Full-width or card-based displays
- **Content Blocks:**
`link`: Individual featured items with image, heading, description, and button
- Custom background colors per block
- Hover effects and animations
- Accessibility features for image links
- **Example Usage:** Winzer Corp Resource Center page (catalog downloads), OneSource homepage (featured products)
**6. Banner Grid Section (`banner-grid.liquid`)**
- **Purpose:** Grid layout for multiple banner-style content blocks
- **Features:**
Responsive grid system
- Custom column layouts
- Image and text combinations
- Hover effects and animations
- **Example Usage:** Homepage promotional grids, category showcases
**7. Cards Section (`cards.liquid`)**
- **Purpose:** Card-based content layout with carousel support
- **Features:**
Swiper carousel integration
- Custom card designs
- Responsive layouts
- Navigation controls
- **Example Usage:** Product showcases, feature highlights, testimonial cards
**8. Collage Section (`collage.liquid`)**
- **Purpose:** Creative image collage layouts
- **Features:**
Multiple image arrangements
- Overlay text and buttons
- Responsive image handling
- Custom aspect ratios
- **Example Usage:** Visual storytelling, product galleries, brand showcases
#### Interactive Content Sections
**9. Info Accordion Section (`info-accordion.liquid`)**
- **Purpose:** FAQ-style collapsible content
- **Features:**
Up to 12 accordion items
- Custom headings and content per item
- Color customization for headings and content
- Single-open or multi-open behavior
- **Use Cases:**
Product support information
- Rules and gameplay instructions
- Company information and policies
- **Example Usage:** OneSource FAQ page, Winzer Corp Our Customers page (industry solutions)
**10. Info Carousel Section (`info-carousel.liquid`)**
- **Purpose:** Carousel/slider for testimonials and content
- **Features:**
Auto-rotate functionality
- Custom slide content
- Navigation controls (dots, arrows)
- Background images and colors
- **Content Blocks:**
`testimonial_slide`: Customer testimonials
- `content_slide`: General content slides
- **Example Usage:** Winzer Corp About page (testimonial carousel), customer success stories
**11. Slideshow Section (`slideshow.liquid`)**
- **Purpose:** Full-featured image and content slideshow
- **Features:**
Multiple slide types
- Video support
- Custom navigation
- Responsive image handling
- **Example Usage:** Homepage hero slideshows, product showcases
#### Product and Collection Sections
**12. Featured Products Section (`featured-products.liquid`)**
- **Purpose:** Display selected products in various layouts
- **Features:**
Product selection and ordering
- Grid and carousel layouts
- Product metafield integration
- Custom styling options
- **Example Usage:** Homepage product highlights, category featured products
**13. Featured Collections Section (`featured-collections.liquid`)**
- **Purpose:** Display selected collections in grid or carousel format
- **Features:**
Collection selection and ordering
- Custom layouts and styling
- Collection metafield integration
- Responsive design
- **Example Usage:** Homepage category navigation, collection showcases
**14. Collection List Section (`collection-list.liquid`)**
- **Purpose:** Display all or filtered collections
- **Features:**
Automatic collection discovery
- Custom filtering options
- Grid and list layouts
- Collection metafield integration
- **Example Usage:** Collection index pages, category browsing
**15. Product Recommendations Section (`product-recommendations.liquid`)**
- **Purpose:** AI-powered product recommendations
- **Features:**
Shopify's recommendation engine
- Custom styling and layouts
- Performance optimization
- Mobile responsiveness
- **Example Usage:** Product detail pages, cart pages, checkout
**16. Related Products Section (`related-products.liquid`)**
- **Purpose:** Display related products based on tags or collections
- **Features:**
Tag-based product selection
- Collection-based filtering
- Custom layouts
- Product metafield integration
- **Example Usage:** Product detail pages, cross-selling
**17. SearchSpring Recommendations Section (`searchspring-recommendations.liquid`)**
- **Purpose:** SearchSpring-powered product recommendations
- **Features:**
SearchSpring integration
- Custom recommendation algorithms
- Advanced filtering
- Performance optimization
- **Example Usage:** Homepage recommendations, product detail pages
#### Blog and Content Sections
**18. Featured Blog Section (`featured-blog.liquid`)**
- **Purpose:** Display selected blog posts
- **Features:**
Blog post selection
- Custom layouts (grid, carousel)
- Article metafield integration
- Responsive design
- **Example Usage:** Homepage blog highlights, content marketing sections
**19. Related Blog Posts Section (`related-blog-posts.liquid`)**
- **Purpose:** Display related blog posts
- **Features:**
Tag-based post selection
- Custom layouts
- Article metafield integration
- Performance optimization
- **Example Usage:** Blog post pages, article recommendations
#### Navigation and Header Sections
**20. Header Section (`header.liquid`)**
- **Purpose:** Main site navigation and branding
- **Features:**
Logo and branding
- Main navigation menu
- Search functionality
- Cart and account links
- Mobile menu support
- **Example Usage:** All pages (site-wide header)
**21. Horizontal Nav Section (`horizontal-nav.liquid`)**
- **Purpose:** Horizontal navigation bar
- **Features:**
Custom navigation links
- Styling options
- Mobile responsiveness
- Dropdown support
- **Example Usage:** Secondary navigation, category menus
**22. Menu Bar Section (`menu-bar.liquid`)**
- **Purpose:** Additional navigation bar
- **Features:**
Custom menu configuration
- Styling options
- Mobile support
- **Example Usage:** Page-specific navigation, breadcrumb alternatives
**23. Breadcrumb Bar Section (`breadcrumb-bar.liquid`)**
- **Purpose:** Breadcrumb navigation
- **Features:**
Automatic breadcrumb generation
- Custom styling
- SEO benefits
- Mobile optimization
- **Example Usage:** All pages (site-wide breadcrumbs)
#### Footer and Utility Sections
**24. Footer Section (`footer.liquid`)**
- **Purpose:** Site footer with links and information
- **Features:**
Multiple footer blocks
- Social media links
- Newsletter signup
- Legal links
- Contact information
- **Example Usage:** All pages (site-wide footer)
**25. Announcement Bar Section (`announcement-bar.liquid`)**
- **Purpose:** Site-wide announcements and promotions
- **Features:**
Customizable messaging
- Link support
- Dismissible functionality
- Mobile optimization
- **Example Usage:** Promotional announcements, shipping notices
#### Specialized Content Sections
**26. Company Features Section (`company-features.liquid`)**
- **Purpose:** Display company features and benefits
- **Features:**
Custom feature blocks
- Icon and text combinations
- Grid layouts
- Animation support
- **Example Usage:** About pages, company value propositions
**27. Features List Section (`features-list.liquid`)**
- **Purpose:** List-based feature display
- **Features:**
Custom feature items
- Icon support
- Text and image combinations
- Responsive layouts
- **Example Usage:** Winzer Corp About page (company statistics), product features
**28. Icon List Section (`icon-list.liquid`)**
- **Purpose:** Icon-based content lists
- **Features:**
Custom icons
- Text descriptions
- Grid layouts
- Hover effects
- **Example Usage:** Service lists, feature highlights, navigation aids
**29. Customer Service Section (`customer-service.liquid`)**
- **Purpose:** Customer service information and contact
- **Features:**
Contact information
- Service hours
- FAQ integration
- Form support
- **Example Usage:** Customer service pages, contact pages
**30. News Press Section (`news-press.liquid`)**
- **Purpose:** News and press release content
- **Features:**
Article listings
- Custom layouts
- Date sorting
- Content filtering
- **Example Usage:** Press pages, news sections
#### Product Detail Sections
**31. Main Product Section (`main-product.liquid`)**
- **Purpose:** Core product display with metafield integration
- **Features:**
Product images and variants
- Metafield integration
- Add to cart functionality
- Product information display
- **Example Usage:** All product pages
**32. Product Description Section (`product-description.liquid`)**
- **Purpose:** Product description and details
- **Features:**
Rich text support
- Metafield integration
- Custom styling
- Mobile optimization
- **Example Usage:** Product detail pages
**33. Specifications Section (`specifications.liquid`)**
- **Purpose:** Product specifications and technical details
- **Features:**
Metafield integration
- Custom layouts
- Responsive design
- Collapsible content
- **Example Usage:** Technical product pages
#### Collection and Search Sections
**34. Collection Banner Section (`main-collection-banner.liquid`)**
- **Purpose:** Collection page headers with metafield integration
- **Metafield Usage:**
`collection.metafields.cql.featured_image_mobile`: Mobile-specific collection images
- `collection.metafields.custom.collection_parent_category`: Parent category display
- `collection.metafields.cql.seo_collection_tagline`: Collection descriptions
- **Features:**
Responsive image handling
- Content positioning and alignment
- Background color and opacity controls
- Border and styling options
- **Example Usage:** All collection pages (Fasteners, Electrical, Automotive, etc.)
**35. Collection Product Grid Section (`main-collection-product-grid.liquid`)**
- **Purpose:** Product grid display for collections
- **Features:**
Product filtering and sorting
- Pagination support
- Custom layouts
- Performance optimization
- **Example Usage:** Collection pages, search results
**36. Collection Product Grid Native Section (`main-collection-product-grid-native.liquid`)**
- **Purpose:** Native Shopify product grid
- **Features:**
Shopify's native filtering
- Standard product display
- Performance optimization
- Mobile responsiveness
- **Example Usage:** Collection pages with native filtering
**37. SEO Content Section (`main-collection-seo-content.liquid`)**
- **Purpose:** Collection page SEO optimization
- **Metafield Usage:**
`collection.metafields.cql.seo_collection_image`: SEO-optimized images
- `collection.metafields.cql.seo_collection_heading`: SEO headlines
- `collection.metafields.cql.seo_collection_tagline`: SEO descriptions
- **Features:**
Automatic alt text generation
- Responsive image handling
- Content width controls
- **Example Usage:** Collection pages with SEO content (Fasteners, Electrical, Automotive collections)
#### Search and Discovery Sections
**38. Main Search Section (`main-search.liquid`)**
- **Purpose:** Search results page
- **Features:**
Search result display
- Filtering options
- Pagination
- Custom layouts
- **Example Usage:** Search results pages
**39. Main Search Native Section (`main-search-native.liquid`)**
- **Purpose:** Native Shopify search
- **Features:**
Shopify's native search
- Standard result display
- Performance optimization
- **Example Usage:** Search results with native functionality
**40. Predictive Search Section (`predictive-search.liquid`)**
- **Purpose:** Search suggestions and autocomplete
- **Features:**
Real-time search suggestions
- Product and page results
- Custom styling
- Performance optimization
- **Example Usage:** Search bars, autocomplete functionality
**41. Search No Results Boundary Section (`search-no-results-boundary.liquid`)**
- **Purpose:** No search results handling
- **Features:**
Custom no results messaging
- Alternative suggestions
- Search refinement tips
- **Example Usage:** Search pages when no results found
#### Cart and Checkout Sections
**42. Cart Icon Bubble Section (`cart-icon-bubble.liquid`)**
- **Purpose:** Cart icon with item count
- **Features:**
Item count display
- Cart link
- Custom styling
- Mobile optimization
- **Example Usage:** Header cart icon
**43. Main Cart Items Section (`main-cart-items.liquid`)**
- **Purpose:** Cart items display
- **Features:**
Product information
- Quantity controls
- Remove functionality
- Price calculations
- **Example Usage:** Cart page
**44. Main Cart Footer Section (`main-cart-footer.liquid`)**
- **Purpose:** Cart footer with totals and checkout
- **Features:**
Order totals
- Checkout button
- Shipping information
- Discount codes
- **Example Usage:** Cart page
**45. Main Cart Add-ons Section (`main-cart-add-ons.liquid`)**
- **Purpose:** Cart add-ons and upsells
- **Features:**
Product recommendations
- Upsell products
- Custom messaging
- **Example Usage:** Cart page upsells
**46. Cart Recommendation Section (`cart-recommendation.liquid`)**
- **Purpose:** Cart-based product recommendations
- **Features:**
AI-powered recommendations
- Cart context awareness
- Custom layouts
- **Example Usage:** Cart page recommendations
#### Account and Customer Sections
**47. Main Account Section (`main-account.liquid`)**
- **Purpose:** Customer account dashboard
- **Features:**
Account information
- Order history
- Account settings
- Mobile optimization
- **Example Usage:** Customer account pages
**48. Main Addresses Section (`main-addresses.liquid`)**
- **Purpose:** Customer address management
- **Features:**
Address listing
- Add/edit addresses
- Default address setting
- Mobile optimization
- **Example Usage:** Customer address pages
**49. Main Login Section (`main-login.liquid`)**
- **Purpose:** Customer login page
- **Features:**
Login form
- Password reset link
- Registration link
- Mobile optimization
- **Example Usage:** Customer login pages
**50. Main Register Section (`main-register.liquid`)**
- **Purpose:** Customer registration page
- **Features:**
Registration form
- Account creation
- Terms acceptance
- Mobile optimization
- **Example Usage:** Customer registration pages
**51. Main Reset Password Section (`main-reset-password.liquid`)**
- **Purpose:** Password reset page
- **Features:**
Password reset form
- Email verification
- Security features
- **Example Usage:** Password reset pages
**52. Main Order Section (`main-order.liquid`)**
- **Purpose:** Order details page
- **Features:**
Order information
- Order items
- Shipping details
- Order status
- **Example Usage:** Order confirmation and details pages
#### Specialized Display Sections
**53. Video Section (`video.liquid`)**
- **Purpose:** Video content display
- **Features:**
Video embedding
- Custom controls
- Responsive design
- Thumbnail support
- **Example Usage:** Product videos, promotional content
**54. Spotlight Section (`spotlight.liquid`)**
- **Purpose:** Spotlight/highlight content display
- **Features:**
Custom layouts
- Image and text combinations
- Hover effects
- Mobile optimization
- **Example Usage:** Featured content, special promotions
**55. Shop the Look Section (`shop-the-look.liquid`)**
- **Purpose:** Product styling and outfit displays
- **Features:**
Product combinations
- Interactive elements
- Custom layouts
- Mobile optimization
- **Example Usage:** Fashion and lifestyle products
**56. Social Media Feed Section (`social-media-feed.liquid`)**
- **Purpose:** Social media content integration
- **Features:**
Social media API integration
- Custom layouts
- Real-time updates
- Mobile optimization
- **Example Usage:** Social proof, brand content
**57. Promos Banner Section (`promos-banner.liquid`)**
- **Purpose:** Promotional banner display
- **Features:**
Custom messaging
- Link support
- Styling options
- Mobile optimization
- **Example Usage:** Promotional announcements, sales banners
**58. In-Page Promo Banner Section (`in-page-promo-banner.liquid`)**
- **Purpose:** In-page promotional content
- **Features:**
Custom positioning
- Dismissible functionality
- Custom styling
- Mobile optimization
- **Example Usage:** Page-specific promotions, contextual offers
#### Utility and Helper Sections
**59. Filter Helpers Section (`filter-helpers.liquid`)**
- **Purpose:** Collection filtering utilities
- **Features:**
Filter controls
- Sorting options
- Clear filters
- Mobile optimization
- **Example Usage:** Collection pages, search results
**60. Recommendations Section (`recommendations.liquid`)**
- **Purpose:** General product recommendations
- **Features:**
AI-powered suggestions
- Custom layouts
- Performance optimization
- Mobile responsiveness
- **Example Usage:** Homepage, product pages
**61. Page Content Section (`page-content.liquid`)**
- **Purpose:** General page content display
- **Features:**
Rich text support
- Custom layouts
- Metafield integration
- Mobile optimization
- **Example Usage:** General content pages
**62. Page Classic Orders Section (`page-classic-orders.liquid`)**
- **Purpose:** Legacy order management interface
- **Features:**
Order management
- Legacy system integration
- Custom styling
- **Example Usage:** Legacy order pages
**63. Main Page Styleguide Section (`main-page-styleguide.liquid`)**
- **Purpose:** Design system documentation
- **Features:**
Style guide content
- Component examples
- Design documentation
- **Example Usage:** Design system pages
**64. Feature Diagram Section (`feature-diagram.liquid`)**
- **Purpose:** Feature and process diagrams
- **Features:**
Custom diagrams
- Interactive elements
- Responsive design
- Animation support
- **Example Usage:** Process explanations, feature demonstrations
### Metafield Integration
**Content-Level Metafields:**
- **Page Metafields:**
`title_tag [string]`: Custom page titles
- `description_tag [string]`: Meta descriptions
- `page-metadata.creator [single_line_text_field]`: Content creator attribution
**Collection Metafields:**
- **SEO Metafields:**
`cql.seo_collection_image`: Collection hero images
- `cql.seo_collection_heading`: Collection headlines
- `cql.seo_collection_tagline`: Collection descriptions
- **Display Metafields:**
`cql.featured_image_mobile`: Mobile-specific images
- `custom.collection_parent_category`: Category hierarchy
**Blog Post Metafields:**
- **SEO Metafields:**
`title_tag [string]`: Custom post titles
- `description_tag [string]`: Meta descriptions
- **Content Metafields:**
Standard Shopify fields: Author, tags, featured image
- Custom content through rich text editor
### Content Management Workflows
**1. Page Creation Workflow:**
- **Access:** Shopify Admin → Online Store → Pages
- **Template Selection:** Choose appropriate template (page.json, article.json)
- **Content Entry:**
Add page title and handle
- Select template suffix if needed
- Add content using rich text editor
- **Metafield Configuration:**
Set custom title tag
- Add meta description
- Configure any custom metafields
- **Section Configuration:**
Add and configure content sections
- Set section-specific settings (colors, spacing, animations)
- Preview and test responsive behavior
- **Publishing:**
Set visibility and publication date
- Test on staging environment
- Publish to live site
**2. Blog Post Creation Workflow:**
- **Access:** Shopify Admin → Online Store → Blog posts
- **Content Creation:**
Add title and content using rich text editor
- Upload featured image
- Set author and publication date
- Set blog category to Learning Hub
- Add relevant tags
- **SEO Configuration:**
Set custom title tag
- Add meta description
- Configure social sharing settings
- **Publishing:**
Preview content
- Schedule or publish immediately
- Verify on live site
**3. Collection Page Enhancement:**
- **Access:** Shopify Admin → Products → Collections
- **Metafield Configuration:**
Set `cql.seo_collection_heading`
- Add `cql.seo_collection_tagline`
- Upload `cql.seo_collection_image`
- Configure `cql.featured_image_mobile`
- **Template Configuration:**
Select collection template
- Configure banner section settings
- Set SEO content section parameters
### Metafields in Content Management
**Page-Level Metafields:**
- **SEO Metafields:** `page.metafields.custom.seo_title`, `page.metafields.custom.seo_description`
- **Custom Content:** `page.metafields.custom.hero_text`, `page.metafields.custom.cta_text`
- **Styling:** `page.metafields.custom.background_color`, `page.metafields.custom.text_color`
**Blog Post Metafields:**
- **SEO:** `article.metafields.custom.seo_title`, `article.metafields.custom.seo_description`
- **Featured Content:** `article.metafields.custom.featured_quote`, `article.metafields.custom.author_bio`
- **Social Sharing:** `article.metafields.custom.social_image`, `article.metafields.custom.social_title`
**Collection Metafields (Content Integration):**
- **SEO Content:** `collection.metafields.cql.seo_collection_heading`, `collection.metafields.cql.seo_collection_tagline`
- **Visual Content:** `collection.metafields.cql.seo_collection_image`
- **Category Organization:** `collection.metafields.custom.collection_parent_category`
**Content Management Best Practices:**
CAUTION: Changing the default template will change it for all pages/products/collections/blog posts on the site. Be careful with this.
1. **Template Selection:** Choose the most appropriate template variant for content type
2. **Metafield Usage:** Populate relevant metafields for SEO and custom functionality
3. **Section Configuration:** Use section blocks to create engaging, structured content
4. **Responsive Design:** Test content on mobile and desktop views
5. **SEO Optimization:** Ensure all content has proper meta titles and descriptions
6. **Content Consistency:** Follow brand guidelines for tone, imagery, and messaging
4. **Testing:**
- Preview collection page
- Test responsive behavior
- Verify metafield display
### Content Management Best Practices
**1. Content Organization:**
- Use consistent naming conventions for pages and blog posts
- Implement proper URL structure with meaningful handles
- Organize content with appropriate tags and categories
- Maintain content hierarchy through parent-child relationships
**2. SEO Optimization:**
- Always set custom title tags and meta descriptions
- Use relevant keywords in content and metafields
- Optimize images with proper alt text
- Implement structured data where appropriate
**3. Responsive Design:**
- Test all content on mobile and desktop devices
- Use appropriate image sizes for different screen sizes
- Configure mobile-specific settings in sections
- Ensure text readability across all devices
**4. Performance Considerations:**
- Optimize images before uploading
- Use appropriate image formats (WebP when possible)
- Minimize custom CSS and JavaScript
- Test page load speeds
**5. Content Maintenance:**
- Regular content audits and updates
- Monitor broken links and outdated information
- Update metafields when content changes
- Archive or remove outdated content
### Content Management Tools
**Primary Tools:**
- **Shopify Admin:** Main content management interface
- **Matrixify:** Bulk content operations and backups
- **Custom Metafields:** Dynamic content configuration
- **Theme Sections:** Modular content building
**Backup &amp; Recovery:**
- **Daily Matrixify Exports:** Automated content backups
- **Version Control:** Theme code versioning through Git
- **Content Snapshots:** Regular content state captures
**Content Analytics:**
- **Google Analytics:** Content performance tracking
- **Search Console:** SEO performance monitoring
- **Shopify Analytics:** Content engagement metrics
- **Mobile Responsiveness:** All content must be mobile-optimized
- **Brand Consistency:** Follow Winzer brand guidelines
- **Accessibility:** Ensure WCAG 2.1 AA compliance
---
## Product Management
### Current Product Inventory
**OneSource Site:**
- **695 Total Products** with comprehensive metafield data
- **Major Categories:** Fasteners (227), Screws (41), Machine Screws (12), Bolts (38), Sockets (22)
- **Key Vendors:** Lenox, Loctite, Nucor, Permatex, WD-40, Kroil Lubricants
- **Product Types:** Hand tools, power tool accessories, fasteners, lubricants, adhesives
**FastServ Site:**
- **50,716 Total Products** (massive inventory - 73x larger than OneSource)
- **Product Types:** No specific product types assigned (all products use generic structure)
- **Categories:** Fasteners (16,917), Fluid Power (7,000), Bolts (4,680), Automotive (3,579), Cap Screws & Hex Bolts (3,575)
- **Vendors:** FastServ (50,716 - single vendor)
**Winzer Corp Site:**
- **56,953 Total Products** (largest inventory - 82x larger than OneSource)
- **Product Types:** No specific product types assigned (all products use generic structure)
- **Categories:** Fasteners (16,981), Fluid Power (7,034), Bolts (4,705), Assortments (4,612), Automotive (3,670)
- **Vendors:** Winzer (56,953 - single vendor)
### Product Data Standards
**Required Metafields for New Products:**
- `cql.categories` (list.single_line_text_field) - **REQUIRED** for collection assignment
- `cql.attributes_json` (variant-level) - **REQUIRED** for SearchSpring filtering
- `custom.oracle_id` (variant-level) - **REQUIRED** for ERP integration
**Recommended Metafields:**
- `cql.vendor_name` - Brand identification
- `cql.features_text` - Product details
- `cql.minimum_order_quantity` - Order requirements
- `cql.product_lead_time` - Shipping expectations
### Metaobjects
**Product Documents Metaobject:**
- **Purpose:** Manages downloadable product documents (PDFs, manuals, SDS sheets)
- **Fields:**
`title` - Document display name
- `document_url` - Direct link to the document file
- **Usage:** Referenced in product metafields and displayed in product download sections
- **Example:** "Test PDF" with URL to PDF document
**Product Message Metaobject:**
- **Purpose:** Displays important product warnings, notices, and regulatory information
- **Fields:**
`title` - Message title/heading
- `icon` - Icon image for visual identification
- `message` - Rich text message content (supports links and formatting)
- **Usage:** Referenced in variant metafields and displayed as warning badges on product pages
- **Examples:**
**Hazardous Material:** "Hazardous Material - Other Regulated Material"
- **Warning:** "WARNING: Cancer and Reproductive Harm — www.P65Warnings.ca.gov"
- **Shipping Restriction:** "This item is restricted to ship to California"
**Metaobject Integration:**
- **Product Messages:** Displayed in product information sections with icons and styled formatting
- **Product Documents:** Shown in accordion-style download sections with download icons
- **Dynamic Updates:** Messages and documents update automatically when variant changes
- **Responsive Design:** Both metaobjects are mobile-optimized and accessible
### Product Grouping & Variants
**Manual Grouping Process:**
- Product grouping (e.g., batteries vs. shirts) is manually managed by category managers
- Grouping decisions based on product attributes like size, color, and functionality
- Some products like batteries are grouped under a single parent, which can cause filtering and display issues
- Inconsistencies in parent/child relationships between Oracle data and Shopify implementation
**Common Grouping Issues:**
- Missing or incorrect variant descriptions
- Inconsistent product specifications across variants
- Filtering problems when products are improperly grouped
- Display issues on collection and product pages
### Product Creation Workflow
**New Product Process:**
1. Navigate to Shopify Admin → Products
2. Click "Add product"
3. Enter basic product information
4. Add variants with proper SKUs
5. Upload product images
6. Fill required metafields
7. Set collection assignments
8. Test on staging environment
9. Publish to live site
**Bulk Product Updates:**
1. Export current product data via Matrixify
2. Update data in Excel/CSV
3. Import updated data via Matrixify
4. Verify changes on staging
5. Deploy to production
### Data Flow & Integration
#### System Data Flow Architecture
graph TD
A[Oracle PIM
Product Data
Pricing Data
Inventory Data] -->|Data Export| E[SFTP Server]
E -->|File Transfer| B[AWS Middleware
Lambda Functions
ECS Tasks]
B -->|API Sync| C[Shopify
Products & Variants
Smart Collections
Price Lists
Inventory Management]
C -->|Product Data| D[External Systems
Google Merchant Center
Meta
TikTok
Bing]
C -->|Product Data| G[Klaviyo
Email Marketing
Customer Data
Automation]
C -->|Customer Data| G
G -->|Customer Sync| C
F[Oracle ERP
Order Management] -->|Pull Orders| C
C -->|Fulfillment Updates| F
classDef oracle fill:#fff5f5,stroke:#dc3545,stroke-width:2px
classDef middleware fill:#ff9900,stroke:#ff6600,stroke-width:2px,color:#fff
classDef shopify fill:#f8fff5,stroke:#95bf47,stroke-width:2px
classDef external fill:#f8f9fa,stroke:#6c757d,stroke-width:2px
classDef sftp fill:#e3f2fd,stroke:#2196f3,stroke-width:2px
classDef erp fill:#f3e5f5,stroke:#9c27b0,stroke-width:2px
classDef klaviyo fill:#fff3e0,stroke:#ff9800,stroke-width:2px
class A oracle
class B middleware
class C shopify
class D external
class E sftp
class F erp
class G klaviyo
**Product Data Pipeline:**
- **Oracle PIM** → **SFTP Server** → **Middleware** → **Shopify**
- **Automated synchronization** of product updates
- **Real-time inventory** and pricing updates
**Order Processing:**
- **Oracle ERP** ↔ **Shopify** (direct API connection)
- **Fulfillment updates** synchronized back to Shopify
- **Return processing** through Loop Returns integration
**Marketing Integration:**
- **Shopify** → **External Systems** (Google, Meta, TikTok, Bing)
- **Shopify** ↔ **Klaviyo** (product data + customer sync)
**Data Quality Issues:**
- Missing variant descriptions and specifications
- Incorrect product features and attributes
- Inconsistent data formatting between Oracle and Shopify
- Manual data entry required to fill gaps
**AI Enhancement Opportunities:**
- AI tools can extract attributes like battery chemistry and size from product names or descriptions
- Reduces manual work for category managers
- Improves data consistency across product variants
---
## Site Management
### Theme Management
**Current Theme Structure:**
- **Base Theme:** Dawn (Shopify's default theme)
- **Customization Level:** Heavy customization with shared components
- **Multi-site Support:** Shared `_shared` directory with site-specific overrides
**Theme Files Organization:**
```
sites/_shared/
├── assets/          # Shared CSS, JS, and images
├── config/          # Global theme settings
├── layout/          # Shared layout templates
├── sections/        # Reusable section templates
├── snippets/        # Reusable code snippets
└── templates/       # Page templates
sites/{site}/
├── assets/          # Site-specific assets
├── config/          # Site-specific settings
└── templates/       # Site-specific templates
```
### App Management
**Current App Inventory:**
App
Purpose
Sites
Owner
SearchSpring
Search &amp; filtering
All
Winzer
Klaviyo
Email marketing &amp; SMS
OneSource, FastServ, Corp
Winzer
Elevar
Conversion tracking
All
Winzer
Matrixify
Data import/export
All
Winzer
CyberSource
Payment processing
OneSource, FastServ
Winzer
Breadcrumbs
Navigation enhancement
OneSource
Winzer
Address Guard
Address validation
OneSource
Winzer
SEA Post Purchase Survey
Customer feedback
OneSource, FastServ
Winzer
Vertex Tax &amp; Compliance
Tax calculation
OneSource, FastServ, Corp
Winzer
ShipperHQ
Shipping &amp; checkout
OneSource, FastServ, Corp
Winzer
CartBot
Cart abandonment recovery
OneSource
Winzer
Forms
Custom form builder
OneSource, FastServ, Corp
Winzer
Flow
Workflow automation
OneSource, FastServ, Corp
Winzer
Launchpad
Scheduled promotions
OneSource
Winzer
Onboard B2B
B2B customer onboarding
FastServ, Corp
Winzer
Checkout Blocks
Checkout customization
FastServ
Winzer
Helium Customer Fields
Customer data collection
FastServ
Winzer
Shopify GraphiQL App
API development tool
OneSource, FastServ, Corp
Winzer
Winzer Pricing API
Custom pricing integration
OneSource, FastServ, Corp
CQL Corp
Theme Access
Theme development access
OneSource, FastServ, Corp
Winzer
**App Configuration:**
**Core eCommerce Apps:**
- **SearchSpring:** Site-specific configurations with custom React components
- **Klaviyo:** All sites, integrated with product data and customer segmentation
- **Elevar:** All sites, tracks conversions and customer behavior
- **Matrixify:** Daily product backups, bulk data operations
**Payment &amp; Checkout:**
- **CyberSource:** Payment processing with fraud protection (All sites)
- **Vertex Tax &amp; Compliance:** Automated tax calculation and compliance (All sites)
- **ShipperHQ:** Advanced shipping rates and checkout optimization (All sites)
- **Address Guard:** Address validation and standardization (OneSource only). Installed to prevent carrier re-routing fees due to inaccurate address entry at checkout.
- **Checkout Blocks:** Checkout customization and optimization (All sites)
**B2B &amp; Customer Management:**
- **Onboard B2B:** Self-Service B2B customer onboarding and management (FastServ, Corp)
- **Helium Customer Fields:** Customer data collection and management (FastServ only). Installed for evaluation. Not active.
- **Search &amp; Discovery:** Enhanced search features and filtering (Corp only). Installed as a test. Not active.
**Customer Experience:**
- **Breadcrumbs:** Enhanced navigation and breadcrumb trails (OneSource only). Shopify doesn't include breadcrumbs in Dawn.
- **CartBot:** Cart abandonment recovery and email campaigns (OneSource only). Installed for evaluation. Not active.
- **SEA Post Purchase Survey:** Customer feedback collection (OneSource, FastServ). Installed for evaluation. Not active.
- **Forms:** Custom form builder for contact and lead generation (All sites). Installed for evaluation. Not active.
**Marketing &amp; Promotions:**
- **Flow:** Workflow automation for customer journeys (All sites)
- **Launchpad:** Scheduled promotions and flash sales (OneSource only). Recommend adding to all sites to schedule promotions and content changes.
**Development &amp; Integration:**
- **Shopify GraphiQL App:** API development and testing tool (All sites)
- **Winzer Pricing API:** Custom pricing integration (CQL Corp managed, All sites)
- **Theme Access:** Development team access to theme files (All sites)
#### Winzer Pricing API App
**Purpose:** Provides exact 4-decimal pricing for B2B customers based on their company-specific pricing agreements.
**How It Works:**
1. **Customer Authentication:** Uses `customer.current_company.id` to identify the logged-in B2B customer's company
2. **API Endpoints:**
- `/apps/pricing-api/customer-product-pricing` - Get exact pricing for specific products
- `/apps/pricing-api/bulk-pricing` - Get pricing for multiple variants at once
- `/apps/pricing-api/erp-orders` - ERP order integration (Classic Orders page)
**Implementation Details:**
**Product Detail Pages (PDP):**
- **Trigger:** "See .0000 Pricing" link appears for logged-in B2B customers
- **Location:** Below product price, only for non-OneSource stores
- **Condition:** Only shows for variants with price between $0.01 and $999,999.99
- **Process:**
Customer clicks "See .0000 Pricing" link
- Loading animation displays for 1 second
- API call fetches exact pricing: `GET /apps/pricing-api/customer-product-pricing?current_company_id={companyId}&amp;product_ids={productId}`
- Price updates to show 4 decimal places (e.g., $1.2345)
- Link disappears after successful pricing update
**SearchSpring Integration (PLP):**
- **Trigger:** "See .0000 Pricing" button in search results
- **Process:**
Bulk API call: `GET /apps/pricing-api/bulk-pricing?variant_ids={variantIds}`
- Updates all variant prices in search results
- Maintains pricing data for variant switching
**Technical Implementation:**
- **JavaScript Files:**
`global.js` - Main pricing fetch function (`fetchExactPrice`)
- `four-decimal-pricing.js` - Add to cart pricing integration
- SearchSpring React components handle PLP pricing
- **Liquid Snippets:**
`exact-pricing.liquid` - PDP pricing link and tooltip
- `price.liquid` - Standard price display (no API integration)
- **CSS:** `component-exact-pricing.css` - Styling for pricing elements
**User Experience:**
- **Tooltip Message:** "Prices shown are rounded up to the next cent, click here to see your exact 4 decimal pricing"
- **Loading State:** Animated loading icon during API call
- **Error Handling:** Link remains visible if API call fails
- **Variant Switching:** Exact pricing persists when switching product variants
**Business Logic:**
- Only available to logged-in B2B customers
- Company-specific pricing based on ERP agreements
- Rounds displayed prices up to next cent, shows exact pricing on demand
- Integrates with Oracle ERP for real-time pricing data
#### B2B Catalog Pricing Solution
**Purpose:** Creative solution to handle products not in a B2B customer's company catalog while still allowing them to see product information and request pricing.
**How It Works:**
1. **$1 Million Price Logic:** Products not in a company's catalog are set to $1,000,000 in Shopify
2. **Price Display Logic:** When a product price is ≥ $1,000,000 or ≤ $0, the system displays "Not available to purchase" instead of the price
3. **Customer Experience:** B2B customers can browse all products but only see pricing for items in their catalog
**Implementation Details:**
**Product Detail Pages (PDP):**
- **Condition:** `sofa_variant.price &gt;= 1000000 or sofa_variant.price &lt;= 0`
- **Display:** Shows "Not available to purchase" message instead of price
- **Location:** Replaces the standard price display in the product price block
- **Exact Pricing:** The "See .0000 Pricing" link is hidden for these products
**SearchSpring Integration (PLP):**
- **Condition:** Same $1 million logic applied in search results
- **Display:** Shows "Not available to purchase" in search result pricing
- **Function:** `notInYourCatalog(price)` checks if price ≥ $1,000,000 or ≤ $0
- **Add to Cart:** Disabled for products not in catalog
**Cart and Checkout:**
- **Add to Cart Button:** Disabled for products with $1 million pricing
- **Button Text:** Shows "Unavailable" instead of "Add to cart"
- **Validation:** Prevents adding non-catalog items to cart
**Technical Implementation:**
- **Price Validation:** `parseInt(corePrice) &gt;= 1000000 || parseInt(corePrice) &lt;= 0`
- **Display Logic:** Conditional rendering based on price value
- **Localization:** Uses `products.product.not_in_catalog` translation key
- **CSS Class:** `.not-in-catalog` for styling
**Template Pricing Fallback System:**
**Purpose:** Provides fallback pricing for B2B customers whose companies have template pricing enabled, allowing them to see "See pricing" instead of "Not available to purchase" for non-catalog items.
**How It Works:**
1. **Company Metafield:** `cql.template_pricing` metafield assigned to companies (e.g., "FRANCHISE")
2. **Fallback Logic:** When a product is not in the company catalog (price = $1,000,000) AND the company has template pricing enabled, show "See pricing" instead of "Not available to purchase"
3. **Customer Experience:** Logged-in B2B customers with template pricing can see pricing options for all products
**Implementation Details:**
**Company Metafield:**
- **Field:** `cql.template_pricing` (company-level metafield)
- **Values:**
`FRANCHISE` (3,466 companies) - Most common template pricing type
- `LOW_MRO` (1 company) - Low MRO template pricing
- `INN1` (1 company) - Internal template pricing
- `GS01` (1 company) - Government/contractor template pricing
- `FASTSERV` (1 company) - FastServ-specific template pricing
- **Purpose:** Determines if company has access to template pricing fallback
- **Coverage:** 3,470 companies have template pricing enabled out of 20,936 total companies
**SearchSpring Integration (PLP):**
- **Condition:** `customer.logged_in &amp;&amp; customer.current_company.metafields.cql.template_pricing != blank &amp;&amp; product.price &gt;= 1000000`
- **Display:** Shows "See pricing" instead of "Not available to purchase"
- **API Call:** Uses `pricing_json` template to fetch fallback pricing data
- **Template:** `product.pricing_json.liquid` returns JSON with variant pricing
- **Template Types:** Works for all template pricing types (FRANCHISE, LOW_MRO, INN1, GS01, FASTSERV)
**Technical Implementation:**
- **API Endpoint:** `/apps/pricing-api/customer-product-pricing?current_company_id={companyId}&amp;product_ids={productId}`
- **Response Format:** JSON array with product_id and variants array containing id and price
- **Template Pricing Logic:** API checks company's `cql.template_pricing` metafield and returns appropriate pricing
- **SearchSpring Integration:** "See pricing" button triggers API call and updates display with template pricing
- **Pricing JSON Template:** `/sites/_shared/templates/product.pricing_json.liquid` (alternative endpoint)
**Working Implementation:**
- **API Endpoint:** `/apps/pricing-api/customer-product-pricing?current_company_id={companyId}&amp;product_ids={productId}`
- **Response Format:** JSON array with product_id and variants array containing id and price
- **Template Pricing Logic:** When company has `cql.template_pricing` metafield set, API returns template pricing instead of $1,000,000
- **User Experience:** "See pricing" button triggers API call and displays actual template pricing
- **Example Response:** Returns 4-decimal pricing (e.g., "61.6440", "2.0064") for all variants
**Business Value:**
- Allows B2B customers to browse full product catalog
- Prevents confusion by clearly indicating catalog vs. non-catalog items
- Maintains professional appearance while encouraging inquiry
- Enables sales team to follow up on pricing requests
---
## Custom Elements
### SearchSpring Integration
**Configuration:**
- **Site IDs:** OneSource (t047mf), FastServ (wk4j0d), Winzer Corp (fsqw40)
- **Custom Components:** React-based result display and filtering
- **Data Sync:** Real-time product data synchronization
- **Customization Points:** Result display, filtering logic, autocomplete
**Key Features:**
- Advanced product filtering
- Autocomplete search
- Product recommendations
- Dynamic variant switching
- Custom result display
**Filtering Behavior:**
- **Automated Filter Generation:** Filters are automatically generated via SearchSpring, pulling from Shopify's category hierarchy fields (e.g., CQL.category_hierarchy)
- **Filter Visibility Rules:** Filters won't appear if all values are the same or if there's insufficient variation
- **Minimum Product Threshold:** Categories with fewer than three products may not display filters
- **Manual Filter Management:** Category managers can manually manage and remove unnecessary filters
- **Product Grouping Impact:** Filter behavior varies depending on how products are grouped and categorized
**Analytics & Reporting:**
- **Revenue Tracking:** On-site recommendations only reflect revenue from products purchased via recommender clicks
- **Data Discrepancies:** Search and product performance data may show discrepancies due to internal testing and Google Analytics integration
- **Testing Considerations:** Test orders may be mistakenly interpreted as real orders in reporting
### External Integrations & Feeds
**Google Shopping Feed:**
- **Management:** Google Shopping feed is managed via Matrixify
- **Process:** Exports a static text file that's linked to Google Merchant Center
- **Update Frequency:** Manual updates as needed
**Social Media Integration Recommendations:**
- **TikTok Shop:** Recommended to use dedicated sales channel apps instead of Matrixify
- **Meta Shop:** Recommended to use dedicated sales channel apps instead of Matrixify
- **Rationale:** Dedicated apps provide better integration, real-time sync, and platform-specific features
**Feed Management Best Practices:**
- Use Matrixify for Google Shopping and other static feed requirements
- Use dedicated platform apps for dynamic, real-time integrations
- Regular monitoring of feed accuracy and completeness
- Test feed updates in staging environment before production deployment
### Custom JavaScript Functions
**Pricing API Integration:**
function fetchExactPrice(companyId, productId) {
// Fetches exact pricing from custom API
// Updates displayed price dynamically
// Hides &quot;exact pricing&quot; link after success
}
**Variant Metafield Display:**
function switchVariantMetafieldDisplay(variant) {
// Updates metafield display when variant changes
// Handles minimum order quantity, lead time, promo messaging
// Manages low inventory warnings
}
### Custom CSS Classes
**Site-Specific Styling:**
- `.site-onesource` - OneSource-specific styles
- `.site-fastserv` - FastServ-specific styles
- `.site-corp` - Winzer Corp-specific styles
- `.pdp__` - Product detail page components
- `.ss__` - SearchSpring components
---
## Operations &amp; Runbooks
### User &amp; Access Provisioning
**Shopify Admin Access:**
1. **Request Access:** Contact Shopify admin for access.
2. **Role Assignment:** Assign appropriate role (Staff, Manager, Owner)
3. **Permissions:** Configure specific permissions based on role
4. **Training:** Complete Shopify admin training
5. **Testing:** Verify access and functionality
**App Access Management:**
- **SearchSpring:** Contact Searchspring Admin for access changes
- **Klaviyo:** Contact Klaviyo Admin for access changes
- **Elevar:** Manage through Elevar dashboard
- **Matrixify:** Manage through Matrixify dashboard
### Theme Build &amp; Deploy Workflow
**Environment Setup:**
1. **Prerequisites:** Node.js 16+, npm, Shopify CLI
2. **Repository Clone:** `git clone [repository-url]`
3. **Dependencies:** `npm install`
4. **Environment Variables:** Configure `.env` file
5. **Shopify CLI:** `shopify theme dev` for local development
**Development Process:**
1. **Create Feature Branch:** `git checkout -b feature/description`
2. **Local Development:** Make changes and test locally
3. **Code Review:** Submit pull request for review
4. **Testing:** Deploy to staging environment
5. **Approval:** Get approval from technical lead
**Deployment Process:**
1. **Staging Deploy:** `npm run deploy:staging`
2. **Testing:** Comprehensive testing on staging
3. **Production Deploy:** `npm run deploy:production`
4. **Validation:** Verify all functionality works
5. **Monitoring:** Watch for errors and issues
**Rollback Procedure:**
1. **Identify Issue:** Document the problem
2. **Access Previous Version:** Use Shopify admin or Git
3. **Deploy Previous Version:** `npm run deploy:production --version=previous`
4. **Verify Fix:** Confirm issue is resolved
5. **Post-Mortem:** Document lessons learned
**Escalation Path:**
1. **Level 1:** Winzer eCommerce Team
2. **Level 2:** CQL Corp Technical Support
3. **Level 3:** Shopify Plus Support
4. **Emergency:** Direct contact with CQL Corp leadership
### Matrixify Backup &amp; Restore
**Daily Backup Process:**
1. **Schedule:** Automated daily at 2 AM EST
2. **Scope:** All products, collections, pages, blog posts
3. **Storage:** Cloud storage + local backup
4. **Verification:** Automated validation of backup integrity
5. **Notification:** Email confirmation of successful backup
**Manual Backup Process:**
1. **Access Matrixify:** Login to Matrixify dashboard
2. **Select Data:** Choose products, collections, pages, etc.
3. **Configure Export:** Set export parameters
4. **Execute Export:** Run export process
5. **Download:** Save backup file securely
6. **Verify:** Check backup completeness
**Restore Process:**
1. **Identify Issue:** Determine what needs to be restored
2. **Select Backup:** Choose appropriate backup file
3. **Test Restore:** First restore to staging environment
4. **Verify Data:** Confirm data integrity
5. **Production Restore:** Execute restore to production
6. **Monitor:** Watch for any issues
### SearchSpring Incident Handling
**Common Issues:**
- **Products Not Appearing:** Check `cql.attributes_json` metafield
- **Filtering Not Working:** Verify metafield data format
- **Search Results Empty:** Check SearchSpring configuration
- **Performance Issues:** Monitor SearchSpring dashboard
**Incident Response:**
1. **Identify Issue:** Document symptoms and impact
2. **Check Status:** Verify SearchSpring service status
3. **Review Logs:** Check SearchSpring dashboard for errors
4. **Test Fix:** Implement solution on staging
5. **Deploy Fix:** Apply fix to production
6. **Monitor:** Watch for resolution
**Escalation:**
1. **Level 1:** Winzer eCommerce Team (30 minutes)
2. **Level 2:** CQL Corp Technical Support (1 hour)
3. **Level 3:** SearchSpring Support (2 hours)
4. **Emergency:** Direct contact with CQL Corp leadership
### AWS Middleware Incident Handling
**Common Issues:**
- **Data Sync Failures:** Lambda function errors or timeouts
- **SFTP Connection Issues:** Oracle ERP connectivity problems
- **Pricing Updates Missing:** Pricing feed handler failures
- **Inventory Discrepancies:** Inventory sync process errors
- **Product Import Delays:** Salsify integration issues
**Incident Response:**
1. **Check CloudWatch Logs:** Review Lambda function execution logs
2. **Verify AWS Service Status:** Check AWS service health dashboard
3. **Test External Connections:** Validate SFTP and API connectivity
4. **Review Data Quality:** Check for data format issues
5. **Restart Failed Processes:** Redeploy Lambda functions if needed
6. **Monitor Recovery:** Watch for successful data synchronization
**Escalation:**
1. **Level 1:** Winzer eCommerce Team 
2. **Level 2:** CQL Corp Technical Support 
3. **Level 3:** AWS Support 
4. **Emergency:** Direct contact with CQL Corp leadership
**Critical Data Flows to Monitor:**
- **Product Import:** Oracle PIM -> Middleware → Shopify
- **Pricing Updates:** Oracle PIM → Shopify
- **Inventory Sync:** Oracle PIM → Shopify
- **Order Export:** Shopify → Oracle ERP (real-time)
### Monitoring Tasks
**Daily Tasks:**
- **Site Health Check:** Verify all sites are accessible
- **SearchSpring Status:** Check search functionality
- **Order Processing:** Monitor order flow
- **Error Logs:** Review Shopify and app error logs
- **Performance:** Check site speed and responsiveness
- **AWS Middleware:** Check CloudWatch logs for Lambda function errors
- **Data Sync Status:** Verify product, pricing, and inventory synchronization
- **SFTP Connectivity:** Test Oracle ERP and KWI SFTP connections
**Weekly Tasks:**
- **Backup Verification:** Confirm backups are complete
- **App Updates:** Check for app updates and security patches
- **Content Review:** Review new content and products
- **Analytics Review:** Check key performance indicators
- **Security Scan:** Run security vulnerability scans
- **AWS Service Health:** Review AWS service status and performance
- **Data Quality Review:** Check for data synchronization issues
- **Middleware Performance:** Analyze Lambda execution metrics
**Monthly Tasks:**
- **Full Site Audit:** Comprehensive site functionality check
- **Performance Optimization:** Review and optimize site speed
- **Content Audit:** Review and update content quality
- **App Review:** Evaluate app performance and costs
- **Security Review:** Comprehensive security assessment
- **AWS Cost Review:** Analyze and optimize AWS service costs
- **Data Integration Audit:** Review all data flows and dependencies
- **Middleware Configuration Review:** Update and optimize settings
---
## KPI &amp; Analytics
### Key Performance Indicators
**Traffic Metrics:**
- **Sessions:** Daily, weekly, monthly session counts
- **Page Views:** Total and unique page views
- **Bounce Rate:** Site engagement quality
- **Average Session Duration:** User engagement time
**Conversion Metrics:**
- **Conversion Rate:** Percentage of visitors who make a purchase
- **Revenue:** Total and average order value
- **Cart Abandonment Rate:** Percentage of abandoned carts
- **Checkout Completion Rate:** Percentage of completed checkouts
**Search Performance:**
- **Search Queries:** Most common search terms
- **Search Conversion Rate:** Percentage of searches leading to purchases
- **Filter Usage:** Most used product filters
- **No Results Rate:** Percentage of searches with no results
### Analytics Dashboards
**Primary Dashboard:** Google Analytics 4
- **Owner:** Winzer
- **Access:** Admin access for team members
- **Update Frequency:** Real-time
- **Key Metrics:** Traffic, conversions, user behavior
**SearchSpring Dashboard:**
- **Owner:** Winzer
- **Access:** Shared access for monitoring
- **Update Frequency:** Real-time
- **Key Metrics:** Search performance, filter usage, recommendations
**Elevar Dashboard:**
- **Owner:** Winzer
- **Access:** Admin access for team members
- **Update Frequency:** Real-time
- **Key Metrics:** Conversion tracking, customer behavior
### Site OperationsReporting Cadence
**Daily Reports:**
- **Traffic Summary:** Sessions, page views, top pages
- **Conversion Summary:** Orders, revenue, conversion rate
- **Search Summary:** Top queries, no results rate
- **Owner:** Winzer eCommerce Team
**Weekly Reports:**
- **Performance Analysis:** Week-over-week comparisons
- **Content Performance:** Top performing pages and products
- **Search Analysis:** Search trends and optimization opportunities
- **Owner:** Winzer eCommerce Team
**Monthly Reports:**
- **Comprehensive Analysis:** Full month performance review
- **Trend Analysis:** Long-term performance trends
- **Accessibility Review:** Routine accessibility review & remediation
- **Page Performance Review:** Review core web vitals and optimize site speed
- **Recommendations:** Actionable insights and improvements
- **Owner:** Winzer eCommerce Team
---
## AWS Middleware System
**Middleware Documentation:** Detailed AWS middleware system documentation has been moved to a separate file for better organization and maintenance.
**Reference:** [Winzer AWS Middleware System Documentation](winzer-middleware-documentation.html)
**Quick Reference:**
- **Architecture:** .NET 6 AWS Lambda functions + ECS Fargate tasks
- **Data Integration:** Oracle ERP, SFTP, AWS S3
- **Critical Flows:** Product sync, pricing updates, inventory management, order processing
- **Monitoring:** AWS CloudWatch logs and alerts
- **Access:** Winzer IT (owner), CQL Corp (emergency support)
---
## Documentation Sources
### Data Sources
- **OneSource Export:** `EVERYTHING-Export_2025-09-24_115603.zip` (September 24, 2025)
- **FastServ Export:** `fastserv-EVERYTHING-Export_2025-09-24_121740.zip` (September 24, 2025)
- **Winzer Corp Export:** `winzer-EVERYTHING-Export_2025-09-24_121626.zip` (September 24, 2025)
- **AWS Middleware:** `cqlcorp-winzer-middleware-cc9fb6a56078.zip` (June 10, 2025)
### Code Analysis
- **Theme Files:** Dawn-based multi-site architecture
- **SearchSpring Integration:** Custom React components
- **Metafield Usage:** 20+ custom metafields across products and variants
- **App Configurations:** Site-specific app settings and integrations
- **AWS Middleware:** .NET 6 Lambda functions and ECS Fargate tasks
- **Data Integration:** Oracle ERP, Salsify PIM, SFTP, and AWS services
### Export Data Summary
- **OneSource:** 6,327 product rows, 552 collection rows, 3,138 page rows, 4,438 blog post rows
- **FastServ:** 50,811 product rows, 777 collection rows, 2,994 page rows, 441 blog post rows
- **Winzer Corp:** 56,954 product rows, 538 collection rows, 195 page rows, 441 blog post rows
### Data Freshness
- **Last Updated:** September 24, 2025
- **Export Frequency:** Daily automated backups via Matrixify
- **Manual Exports:** Available on-demand through Matrixify dashboard
- **Data Owner:** Winzer eCommerce Team
### Maintenance Schedule
- **Daily:** Automated backups and monitoring
- **Weekly:** Content and product reviews
- **Monthly:** Full site audits and performance reviews
- **Quarterly:** Comprehensive documentation updates
---
**Document Version:** 2.0
**Last Updated:** September 25, 2025