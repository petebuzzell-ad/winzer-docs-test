# Winzer AWS Middleware System Documentation
## Table of Contents
- [Overview](#overview)
- [Architecture Components](#architecture-components)
- [Collection Management](#collection-management)
- [Data Flow Architecture](#data-flow-architecture)
- [Critical Dependencies](#critical-dependencies)
- [Middleware Configuration](#middleware-configuration)
- [Monitoring and Alerts](#monitoring-and-alerts)
- [Operational Procedures](#operational-procedures)
- [Emergency Procedures](#emergency-procedures)
- [Access and Permissions](#access-and-permissions)
- [Cost Management](#cost-management)
## AWS Middleware System
### Overview
The Winzer eCommerce platform relies on a critical .NET 6-based middleware system deployed on AWS that handles all data synchronization between external systems and Shopify. This system is essential for the platform's operation and must be understood by the eCommerce team.
### Architecture Components
**AWS Lambda Functions:**
- **Product Feed Handler:** Imports product data from Oracle PIM
- **Pricing Feed Handler:** Synchronizes pricing data from Oracle PIM
- **Inventory Feed Handler:** Updates inventory levels from multiple sources
- **Order Export Handler:** Exports orders to external systems
- **Fulfillment Feed Handler:** Processes fulfillment data
- **Return Export Handler:** Handles return processing
**AWS ECS Fargate Tasks:**
- **Product Feed Console App:** Long-running product import processes
- **Pricing Feed Console App:** Bulk pricing synchronization
### Collection Management
**Automatic Collection Creation:**
- **Smart Collections:** Middleware creates smart collections based on product categories
- **Rule-Based Assignment:** Products are automatically assigned using metafield rules
- **Category Source:** Uses `cql.categories` metafield from Oracle PIM data
- **Collection Rules:** Products must match category metafield exactly
**Collection Service Features:**
- **GetAllCollections():** Retrieves existing Shopify collections
- **CreateCollection():** Creates new smart collections with rules
- **GetCollectionByHandle():** Finds collections by handle
- **GetCategoriesMetafield():** Gets categories metafield definition
**Collection Rule Configuration:**
- **Rule Type:** PRODUCT_METAFIELD_DEFINITION
- **Metafield:** cql.categories
- **Relation:** EQUALS
- **Applied Disjunctively:** false (products must match ALL rules)
### Data Flow Architecture
**Note:** For the complete system data flow diagram showing all integrations, see the [main documentation](winzer-documentation.html#data-flow-and-integration).
#### Middleware-Specific Data Flows
**Product Data Flow:**
- **Oracle PIM** → **SFTP Server** → **AWS Lambda** → **Shopify Products**
- **Shopify Products** → **External Systems** (Google Merchant, TikTok, Meta, Bing)
- **Shopify Products** → **Klaviyo** (Product data for email marketing)
**Collection Management Flow:**
- **Category Extraction:** Extract categories from `cql.categories` metafield
- **Collection Comparison:** Compare against existing Shopify collections
- **Smart Collection Creation:** Create missing collections with automatic rules
- **Product Assignment:** Products automatically assigned based on category metafield
**Pricing Data Flow:**
- **Oracle PIM** → **SFTP Server** → **AWS Lambda** → **Shopify Price Lists**
- **Bulk Pricing** → **Contract Pricing** → **Template Pricing**
- **Customer-specific pricing** via Shopify Plus B2B features
**Inventory Data Flow:**
- **Oracle PIM** → **SFTP Server** → **AWS Lambda** → **Shopify Inventory**
- **Store Locations** → **Inventory Levels** → **Stock Management**
**Customer Data Flow:**
- **Shopify** → **Klaviyo** (Customer data for email marketing)
- **Klaviyo** → **Shopify** (Customer sync and segmentation data)
**Order Data Flow:**
- **Oracle ERP** → **Shopify API** (Pull orders from Shopify)
- **Shopify** → **Oracle ERP** (Fulfillment updates pushed back to Shopify)
- **Fulfillment Updates** → **External Systems**
- **Return Processing** → **Loop Returns API**
### Critical Dependencies
**External Systems:**
- **Oracle PIM:** Primary source of product, pricing, and inventory data
- **SFTP Servers:** Data file exchange (70.164.0.172, secure.kligerweiss.net)
- **AWS S3:** File storage and archival
- **Loop Returns:** Return processing integration
**AWS Services:**
- **Lambda:** Serverless function execution
- **ECS Fargate:** Containerized long-running tasks
- **S3:** File storage and data lake
- **SNS:** Notification and alerting
- **EventBridge:** Scheduled task execution
- **ECR:** Container image registry
### Middleware Configuration
**Environment Variables:**
- `SHOPIFY_ACCESS_TOKEN`: API access for each site
- `ORACLE_SFTP_CREDENTIALS`: ERP data access
- `AWS_REGION`: us-east-1
- `SNS_TOPIC_ARN`: Alert notifications
**Site-Specific Configuration:**
- **OneSource:** DTC-focused product and pricing rules
- **FastServ:** High-volume B2B inventory management
- **Winzer Corp:** Enterprise pricing and contract management
### Monitoring and Alerts
**AWS CloudWatch:**
- Lambda function execution logs
- ECS task status and performance
- SNS notification delivery
- Error rate monitoring
**Alert Conditions:**
- Lambda function failures
- ECS task failures
- Data synchronization errors
- SFTP connection issues
- Shopify API rate limit exceeded
### Operational Procedures
**Daily Monitoring:**
- Check CloudWatch logs for errors
- Verify data synchronization completion
- Monitor inventory update success rates
- Review pricing update accuracy
**Weekly Maintenance:**
- Review SFTP file processing
- Validate Oracle PIM connectivity
- Monitor AWS service health
**Monthly Reviews:**
- Analyze data quality metrics
- Review error patterns and trends
- Update configuration as needed
- Performance optimization review
### Emergency Procedures
**Middleware Failure Response:**
- **Immediate:** Check CloudWatch logs for error details
- **Assessment:** Determine scope of data synchronization impact
- **Escalation:** Contact CQL Corp for technical support
- **Communication:** Notify stakeholders of data sync delays
- **Recovery:** Execute manual data sync if possible
**Data Sync Issues:**
- **Identify:** Which data flow is affected (products, pricing, inventory)
- **Impact:** Assess customer-facing impact
- **Workaround:** Manual data entry if critical
- **Resolution:** Restart failed processes or redeploy functions
### Access and Permissions
**AWS Console Access:**
- **Owner:** Winzer IT
- **Read-Only:** N/A
- **Emergency:** CQL Corp (incident response)
**Required Permissions:**
- CloudWatch logs viewing
- Lambda function monitoring
- ECS task status checking
- SNS notification viewing
### Cost Management
**AWS Costs:**
- **Lambda:** Pay-per-execution (minimal cost)
- **ECS Fargate:** Based on task runtime
- **S3:** Storage and transfer costs
- **SNS:** Notification delivery costs
**Optimization:**
- Monitor Lambda execution duration
- Optimize ECS task resource allocation
- Archive old S3 data regularly
- Review SNS notification frequency
---
**Document Version:** 1.0
**Last Updated:** September 24, 2025
**Next Review:** December 24, 2025
**Document Owner:** Winzer eCommerce Team
**Related Documentation:** [Main Platform Documentation](winzer-documentation.html)