using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;
using CQL.Middleware.Impl.Shopify.GraphQL.Models;
using Dasync.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify.Services
{
    public partial class ProductService
    {
        public async Task<ShopifyProduct?> GetProduct(ShopifyProductQuery query, string productId)
        {
            ValidateShopifyProductQuery(query);

            var graphQL = BuildProductGraphQLQuery(query);

            if (!productId.StartsWith("gid"))
            {
                productId = $"gid://shopify/Product/{productId}";
            }

            var queryParameters = new
            {
                metaFieldNamespace = query.MetafieldNamespace,
                id = productId,
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductQueryResultData>(graphQL, queryParameters, true);
            if (result.product != null)
            {
                return await AssembleProductFromGraphQLProduct(result.product, query);
            }

            return null;
        }

        public async Task<ShopifyProductResult> GetProducts(ShopifyProductQuery query, string? cursor = null)
        {
            ValidateShopifyProductQuery(query);

            var graphQL = BuildProductsGraphQLQuery(query);

            var queryParameters = new
            {
                // TODO: Might be a good idea to have these counts be configurable somewhere.
                numProducts = 20,
                metaFieldNamespace = query.MetafieldNamespace,
                after = cursor,
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductsQueryResultData>(graphQL, queryParameters, true);
            var products = new ConcurrentBag<ShopifyProduct>();

            if (result.products != null && result.products.edges != null)
            {
                await result.products.edges.ParallelForEachAsync(async product => {
                    if (product.node != null)
                    {
                        var completeProduct = await AssembleProductFromGraphQLProduct(product.node, query);
                        products.Add(completeProduct);
                    }
                }, maxDegreeOfParallelism: 5);
            }

            var rval = new ShopifyProductResult()
            {
                HasMoreResults = result.products?.pageInfo?.hasNextPage ?? false,
                Cursor = result.products?.pageInfo?.endCursor,
                Products = products.ToList()
            };

            return rval;
        }

        private static void ValidateShopifyProductQuery(ShopifyProductQuery query)
        {
            // If they are requesting variant metafields, then they must request variants as well.
            if (query.IncludeVariantMetafieldsInResults && !query.IncludeVariantsInResults)
            {
                query.IncludeVariantsInResults = true;
            }

            if (query.ProductFields == null || !query.ProductFields.Any())
            {
                query.ProductFields = new string[] { "id " };
            }

            if (query.IncludeVariantsInResults && (query.VariantFields == null || !query.VariantFields.Any()))
            {
                query.VariantFields = new string[] { "id" };
            }

            // If they are requesting any connected objects for the product, then we have to make sure the "id" column is in the product field list.
            if ((query.IncludeVariantsInResults || query.IncludeMetafieldsInResults || query.IncludeImagesInResults)
                &&
                (query.ProductFields == null || !query.ProductFields.Contains("id")))
            {
                var newProductFields = new List<string>
                {
                    "id"
                };
                if (query.ProductFields != null) newProductFields.AddRange(query.ProductFields);
                query.ProductFields = newProductFields.ToArray();
            }

            // If they are requesting any connected objects for the variant, then we have to make sure the "id" column is in the variant field list.
            if (query.IncludeVariantMetafieldsInResults && (query.VariantFields == null || !query.VariantFields.Contains("id")))
            {
                var newVariantFields = new List<string>();
                newVariantFields.Add("id");
                if (query.VariantFields != null) newVariantFields.AddRange(query.VariantFields);
                query.VariantFields = newVariantFields.ToArray();
            }
        }

        private async Task<IList<ShopifyProductVariant>> GetProductVariants(ShopifyProductQuery query, string productId, string? cursor = null)
        {
            var rval = new List<ShopifyProductVariant>();

            var graphQl = BuildVariantsGraphQLQuery(query.IncludeVariantMetafieldsInResults, query.IncludeImagesInResults, query.VariantFields);

            var queryParameters = new
            {
                first = 10,
                query = string.Format("product_id:{0}", RemoveGIDCrapFromShopifyId(productId)),
                metaFieldNamespace = query.MetafieldNamespace,
                after = cursor
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductVariantQueryResultData>(graphQl, queryParameters, true);

            if (result.productVariants != null && result.productVariants.edges != null && result.productVariants.edges.Any())
            {
                rval.AddRange(await AssembleProductVariantsFromVariantEdges(result.productVariants.edges, query));
                if (result.productVariants.pageInfo != null && result.productVariants.pageInfo.hasNextPage)
                {
                    // Recursively call ourself with the cursor for the next page.
                    rval.AddRange(await GetProductVariants(query, productId, result.productVariants.pageInfo.endCursor));
                }
            }
            return rval;
        }

        private static string RemoveGIDCrapFromShopifyId(string idWithGidCrap)
        {
            // Takes a string like "gid://shopify/ProductVariant/1341341343" and returns just "1341341343"
            return idWithGidCrap[(idWithGidCrap.LastIndexOf('/') + 1)..];
        }

        private async Task<IList<ShopifyMetaField>> GetProductMetafields(ShopifyProductQuery query, string productId, string? cursor = null)
        {
            var rval = new List<ShopifyMetaField>();

            var graphQL = BuildProductMetafieldsGraphQLQuery();

            var queryParameters = new
            {
                owner_id = productId,
                first = 20,
                after = cursor,
                mfnamespace = query.MetafieldNamespace
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductMetafieldsQueryResultData>(graphQL, queryParameters, true);

            if (result?.product?.metafields?.edges != null && result.product.metafields.edges.Any())
            {
                rval.AddRange(AssembleMetafieldsFromMetafieldEdges(result.product.metafields.edges));
                if (result.product.metafields.pageInfo != null && result.product.metafields.pageInfo.hasNextPage)
                {
                    // Recurse for next page.
                    rval.AddRange(await GetProductMetafields(query, productId, result.product.metafields.pageInfo.endCursor));
                }
            }

            return rval;
        }

        private async Task<IList<ShopifyMetaField>> GetProductVariantMetafields(ShopifyProductQuery query, string variantId, string? cursor = null)
        {
            var rval = new List<ShopifyMetaField>();

            var graphQL = BuildProductVariantMetafieldsGraphQLQuery();

            var queryParameters = new
            {
                owner_id = variantId,
                first = 20,
                after = cursor,
                mfnamespace = query.MetafieldNamespace
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductVariantMetafieldsQueryResultData>(graphQL, queryParameters, true);

            if (result?.productVariant?.metafields?.edges != null && result.productVariant.metafields.edges.Any())
            {
                rval.AddRange(AssembleMetafieldsFromMetafieldEdges(result.productVariant.metafields.edges));
                if (result.productVariant.metafields.pageInfo != null && result.productVariant.metafields.pageInfo.hasNextPage)
                {
                    // Recurse for next page.
                    rval.AddRange(await GetProductVariantMetafields(query, variantId, result.productVariant.metafields.pageInfo.endCursor));
                }
            }

            return rval;
        }

        private async Task<IList<ShopifyProductImage>> GetProductImages(ShopifyProductQuery query, string productId, string? cursor = null)
        {
            var rval = new List<ShopifyProductImage>();

            var graphQL = BuildProductImagesGraphQLQuery(query.IncludeMetafieldsInResults);

            var queryParameters = new
            {
                owner_id = productId,
                first = 20,
                after = cursor,
                metaFieldNamespace = query.MetafieldNamespace
            };

            var result = await ExecuteGraphQLQuery<ShopifyGraphQLProductImagesQueryResultData>(graphQL, queryParameters, true);

            if (result?.product?.media?.edges != null && result.product.media.edges.Any())
            {
                rval.AddRange(AssembleImagesFromMediaEdges(result.product.media.edges));
                if (result.product.media.pageInfo != null && result.product.media.pageInfo.hasNextPage)
                {
                    // Recurse for next page.
                    rval.AddRange(await GetProductImages(query, productId, result.product.media.pageInfo.endCursor));
                }
            }

            return rval;
        }

        private async Task<ShopifyProduct> AssembleProductFromGraphQLProduct(ShopifyGraphQLProduct product, ShopifyProductQuery query)
        {
            var rval = new ShopifyProduct()
            {
                // Add more fields as needed.
                Id = product.Id,
                CreatedAt = product.createdAt,
                Status = product.status,
                Handle = product.handle,
                ProductType = product.productType,
                Title = product.title,
                Vendor = product.vendor,
                Tags = product.tags != null ? string.Join(",", product.tags) : null,
                DescriptionHtml = product.descriptionHtml,
                Options = product.options?.Select(o => new ShopifyProductOption()
                {
                    Id = o.id,
                    Name = o.name,
                    Values = o.optionValues?.Select(v => new ShopifyProductOptionValue()
                    {
                        Id = v.id,
                        Name = v.name
                    }).ToList() ?? new List<ShopifyProductOptionValue>()
                    
                }).ToList() ?? new List<ShopifyProductOption>()
            };

            // Handle the variants.
            if (product.variants != null && product.variants.edges != null && product.variants.edges.Any())
            {
                if (product.variants.pageInfo == null || !product.variants.pageInfo.hasNextPage)
                {
                    // If we managed to get all the variants in the original query, just construct the variant list
                    // directly from the original query results.
                    rval.Variants = await AssembleProductVariantsFromVariantEdges(product.variants.edges, query);
                }
                else if (rval.Id != null)
                {
                    // If there are multiple pages of variants, then we need to run additional queries to 
                    // get those (and we can't use the cursor from this query to do it...)
                    rval.Variants = await GetProductVariants(query, rval.Id);
                }
            }

            // Handle the metafields.
            if (product.metafields != null && product.metafields.edges != null && product.metafields.edges.Any())
            {
                if (product.metafields.pageInfo == null || !product.metafields.pageInfo.hasNextPage)
                {
                    rval.MetaFields = AssembleMetafieldsFromMetafieldEdges(product.metafields.edges);
                }
                else if (rval.Id != null)
                {
                    rval.MetaFields = await GetProductMetafields(query, rval.Id);
                }
            }

            // Handle the images.
            if (product.media != null && product.media.edges != null && product.media.edges.Any())
            {
                if (product.media.pageInfo == null || !product.media.pageInfo.hasNextPage)
                {
                    rval.Images = AssembleImagesFromMediaEdges(product.media.edges);
                }
                else if (rval.Id != null)
                {
                    rval.Images = await GetProductImages(query, rval.Id);
                }
            }

            return rval;
        }

        private async Task<IList<ShopifyProductVariant>> AssembleProductVariantsFromVariantEdges(ShopifyGraphQLResultEdge<ShopifyGraphQLProductVariant>[] variantEdges, ShopifyProductQuery query)
        {
            var rval = new ConcurrentBag<ShopifyProductVariant>();

            await variantEdges.ParallelForEachAsync(async variantEdge =>
            {
                if (variantEdge.node != null)
                {
                    var variant = new ShopifyProductVariant()
                    {
                        // TODO: Add more fields.
                        Id = variantEdge.node.id,
                        SKU = variantEdge.node.sku,
                        MediaId = variantEdge.node?.media?.edges?.FirstOrDefault()?.node?.id,
                        MediaSrc = variantEdge.node?.media?.edges?.FirstOrDefault()?.node?.preview?.image?.url,
                        MediaAlt = variantEdge.node?.media?.edges?.FirstOrDefault()?.node?.alt,
                        Price = variantEdge.node?.price,
                        Position = variantEdge.node?.position,
                        Option1 = variantEdge.node?.selectedOptions?.Length > 0 ? variantEdge.node.selectedOptions[0].value : null,
                        Option2 = variantEdge.node?.selectedOptions?.Length > 1 ? variantEdge.node.selectedOptions[1].value : null,
                        Option3 = variantEdge.node?.selectedOptions?.Length > 2 ? variantEdge.node.selectedOptions[2].value : null,
                    };

                    // Add the metafields to the variant if there are any to add.
                    if (variantEdge.node?.metafields?.edges?.Any() ?? false)
                    {
                        if (variantEdge.node.metafields.pageInfo == null || !variantEdge.node.metafields.pageInfo.hasNextPage)
                        {
                            // If we captured all the metafields in the parent query, then just use those.
                            variant.Metafields = AssembleMetafieldsFromMetafieldEdges(variantEdge.node.metafields.edges);
                        }
                        else if (variant.Id != null)
                        {
                            // If there are multiple pages, then we need to run another query.
                            variant.Metafields = await GetProductVariantMetafields(query, variant.Id);
                        }
                    }

                    rval.Add(variant);
                }
            }, maxDegreeOfParallelism: 5);

            return rval.ToList();
        }

        private static IList<ShopifyMetaField> AssembleMetafieldsFromMetafieldEdges(ShopifyGraphQLResultEdge<ShopifyGraphQLMetafield>[] edges)
        {
            var rval = new List<ShopifyMetaField>();

            foreach (var edge in edges)
            {
                if (edge.node != null)
                {
                    var metafield = new ShopifyMetaField()
                    {
                        Id = edge.node.id,
                        Key = edge.node.key,
                        Namespace = edge.node.Namespace,
                        Value = edge.node.value,
                    };

                    rval.Add(metafield);
                }
            }

            return rval;
        }

        private static IList<ShopifyProductImage> AssembleImagesFromMediaEdges(ShopifyGraphQLResultEdge<ShopifyGraphQLMedia>[] edges)
        {
            var rval = new List<ShopifyProductImage>();

            foreach (var edge in edges)
            {
                if (edge.node != null && edge.node.preview != null && edge.node.preview.image != null && edge.node.mediaContentType == "IMAGE")
                {
                    var image = new ShopifyProductImage()
                    {
                        MediaId = edge.node.id,
                        ImageId = edge.node.preview.image.id,
                        Alt = edge.node.preview.image.altText,
                        Height = edge.node.preview.image.height,
                        Width = edge.node.preview.image.width,
                        Src = edge.node.preview.image.url,
                    };

                    if (edge.node?.preview?.image?.metafields?.edges?.Any() ?? false)
                    {
                        image.Metafields = AssembleMetafieldsFromMetafieldEdges(edge.node.preview.image.metafields.edges);
                    }

                    rval.Add(image);
                }
            }

            return rval;
        }

        private static string BuildProductGraphQLQuery(ShopifyProductQuery query)
        {
            var graphQL = new StringBuilder(ProductGraphQLQuery);

            // Add all the product fields they requested to the GraphQL query.
            if (query.ProductFields != null && query.ProductFields.Any())
            {
                graphQL.Replace("##FIELDS##", string.Join(Environment.NewLine, query.ProductFields));
            }
            else
            {
                graphQL.Replace("##FIELDS##", string.Empty);
            }

            // Add the Variant sub-template if they requested variant data.
            if (query.IncludeVariantsInResults)
            {
                graphQL.Replace("##VARIANT_PARTIAL##", BuildProductsGraphQLQuery_VariantsPartial(query));
            }
            else
            {
                graphQL.Replace("##VARIANT_PARTIAL##", string.Empty);
            }

            // Add the Metafields sub-template if they requested metafield data.
            if (query.IncludeMetafieldsInResults)
            {
                var metafieldsSubQuery = new StringBuilder(ProductsGraphQLQuery_MetafieldsPartial);
                metafieldsSubQuery.Replace("##FIRST##", "1");
                graphQL.Replace("##METAFIELDS_PARTIAL##", metafieldsSubQuery.ToString());
            }
            else
            {
                graphQL.Replace("##METAFIELDS_PARTIAL##", string.Empty);
            }

            // Add the variable for metafield namespace if any kind of metafield data is requested.
            if (query.IncludeMetafieldsInResults || query.IncludeVariantMetafieldsInResults)
            {
                graphQL.Replace("##METAFIELD_NAMESPACE_VAR##", ", $metaFieldNamespace: String");
            }
            else
            {
                graphQL.Replace("##METAFIELD_NAMESPACE_VAR##", string.Empty);
            }

            // Add the Images sub-template if they requested image data.
            if (query.IncludeImagesInResults)
            {
                var imagesSubQuery = new StringBuilder(ProductsGraphQLQuery_ImagesPartial);
                graphQL.Replace("##IMAGES_PARTIAL##", imagesSubQuery.ToString());
                if (query.IncludeMetafieldsInResults)
                {
                    var metafieldsSubQuery = new StringBuilder(ProductsGraphQLQuery_MetafieldsPartial);
                    metafieldsSubQuery.Replace("##FIRST##", "1");
                    graphQL.Replace("##METAFIELDS_PARTIAL##", metafieldsSubQuery.ToString());
                }
                else
                {
                    graphQL.Replace("##METAFIELDS_PARTIAL##", string.Empty);
                }
            }
            else
            {
                graphQL.Replace("##IMAGES_PARTIAL##", string.Empty);
            }

            return graphQL.ToString();
        }

        private static string BuildProductsGraphQLQuery(ShopifyProductQuery query)
        {
            var graphQL = new StringBuilder(ProductsGraphQLQuery);

            // Add all the product fields they requested to the GraphQL query.
            if (query.ProductFields != null && query.ProductFields.Any())
            {
                graphQL.Replace("##FIELDS##", string.Join(Environment.NewLine, query.ProductFields));
            }
            else
            {
                graphQL.Replace("##FIELDS##", string.Empty);
            }

            // Add the Variant sub-template if they requested variant data.
            if (query.IncludeVariantsInResults)
            {
                graphQL.Replace("##VARIANT_PARTIAL##", BuildProductsGraphQLQuery_VariantsPartial(query));
            }
            else
            {
                graphQL.Replace("##VARIANT_PARTIAL##", string.Empty);
            }

            // Add the Metafields sub-template if they requested metafield data.
            if (query.IncludeMetafieldsInResults)
            {
                var metafieldsSubQuery = new StringBuilder(ProductsGraphQLQuery_MetafieldsPartial);
                metafieldsSubQuery.Replace("##FIRST##", "1");
                graphQL.Replace("##METAFIELDS_PARTIAL##", metafieldsSubQuery.ToString());
            }
            else
            {
                graphQL.Replace("##METAFIELDS_PARTIAL##", string.Empty);
            }

            // Add the variable for metafield namespace if any kind of metafield data is requested.
            if (query.IncludeMetafieldsInResults || query.IncludeVariantMetafieldsInResults)
            {
                graphQL.Replace("##METAFIELD_NAMESPACE_VAR##", ", $metaFieldNamespace: String");
            }
            else
            {
                graphQL.Replace("##METAFIELD_NAMESPACE_VAR##", string.Empty);
            }

            // Add the Images sub-template if they requested image data.
            if (query.IncludeImagesInResults)
            {
                var imagesSubQuery = new StringBuilder(ProductsGraphQLQuery_ImagesPartial);
                graphQL.Replace("##IMAGES_PARTIAL##", imagesSubQuery.ToString());
            }
            else
            {
                graphQL.Replace("##IMAGES_PARTIAL##", string.Empty);
            }

            return graphQL.ToString();
        }

        private static string BuildProductsGraphQLQuery_VariantsPartial(ShopifyProductQuery query)
        {
            var variantsSubQuery = new StringBuilder(ProductsGraphQLQuery_VariantsPartial);
            if (query.VariantFields != null && query.VariantFields.Any())
            {
                variantsSubQuery.Replace("##FIELDS##", string.Join(Environment.NewLine, query.VariantFields));
            }
            else
            {
                variantsSubQuery.Replace("##FIELDS##", string.Empty);
            }

            // Add the metafields sub-template if they requested variant meta-data.
            if (query.IncludeVariantMetafieldsInResults)
            {
                var metafieldsSubQuery = new StringBuilder(ProductsGraphQLQuery_MetafieldsPartial);
                metafieldsSubQuery.Replace("##FIRST##", "1");
                variantsSubQuery.Replace("##METAFIELDS_PARTIAL##", metafieldsSubQuery.ToString());
            }
            else
            {
                variantsSubQuery.Replace("##METAFIELDS_PARTIAL##", string.Empty);
            }

            if (query.IncludeImagesInResults)
            {
                variantsSubQuery.Replace("##MEDIA_PARTIAL##", ProductsGraphQLQuery_FirstMediaPartial);
            }
            else
            {
                variantsSubQuery.Replace("##MEDIA_PARTIAL##", string.Empty);
            }

            return variantsSubQuery.ToString();
        }

        private static string BuildVariantsGraphQLQuery(bool includeMetafields, bool includeImage, string[]? fields)
        {
            var query = new StringBuilder(VariantsGraphQLQuery);
            query.Replace("##FIELDS##", string.Join(Environment.NewLine, fields ?? Array.Empty<string>()));

            if (includeMetafields)
            {
                var metaPartial = new StringBuilder(ProductsGraphQLQuery_MetafieldsPartial);
                metaPartial.Replace("##FIRST##", "1");
                query.Replace("##METAFIELDS_PARTIAL##", metaPartial.ToString());
                query.Replace("##METAFIELD_NAMESPACE_VAR##", ", $metaFieldNamespace: String");
            }
            else
            {
                query.Replace("##METAFIELDS_PARTIAL##", string.Empty);
                query.Replace("##METAFIELD_NAMESPACE_VAR##", string.Empty);
            }

            if (includeImage)
            {
                query.Replace("##MEDIA_PARTIAL##", ProductsGraphQLQuery_FirstMediaPartial);
            }
            else
            {
                query.Replace("##MEDIA_PARTIAL##", string.Empty);
            }

            return query.ToString();
        }

        private static string BuildProductMetafieldsGraphQLQuery()
        {
            // Making this a method for consistency with others even though it really does nothing.
            return ProductMetafieldsGraphQLQuery;
        }

        private static string BuildProductVariantMetafieldsGraphQLQuery()
        {
            // Making this a method for consistency with others even though it really does nothing.
            return ProductVariantMetafieldsGraphQLQuery;
        }

        private static string BuildProductImagesGraphQLQuery(bool includeMetafields)
        {
            // Making this a method for consistency with others even though it really does nothing.
            if (includeMetafields)
            {
                return ProductImagesGraphQLQuery
                    .Replace("##METAFIELD_NAMESPACE_VAR##", ", $metaFieldNamespace: String")
                    .Replace("##METAFIELDS_PARTIAL##", ProductsGraphQLQuery_MetafieldsPartial.Replace("##FIRST##", "5"));
            }
            else
            {
                return ProductImagesGraphQLQuery
                    .Replace("##METAFIELD_NAMESPACE_VAR##", string.Empty)
                    .Replace("##METAFIELDS_PARTIAL##", string.Empty);
            }
        }

        private const string ProductGraphQLQuery =
@"
query ProductLookup($id: ID!, ##METAFIELD_NAMESPACE_VAR##) {
    product(id: $id) {
        ##FIELDS##
        ##VARIANT_PARTIAL##
        ##METAFIELDS_PARTIAL##  
        ##IMAGES_PARTIAL##        
    }
}
";

        private const string ProductsGraphQLQuery =
@"
query ProductLookup($numProducts: Int, $after: String ##METAFIELD_NAMESPACE_VAR##) {
    products(first: $numProducts, after: $after) {
        edges {
            node {
                ##FIELDS##
                ##VARIANT_PARTIAL##
                ##METAFIELDS_PARTIAL##  
                ##IMAGES_PARTIAL##
            }
        }
        pageInfo {
            hasNextPage
            endCursor
        }
    }
}";

        private const string ProductsGraphQLQuery_VariantsPartial =
@"
variants(first:1) {
    edges {
        node {
            selectedOptions {
                name
                value
            }
            ##FIELDS##
            ##MEDIA_PARTIAL##
            ##METAFIELDS_PARTIAL##
        }
    }
    pageInfo {
        hasNextPage
        endCursor
    }
}
";

        private const string ProductsGraphQLQuery_FirstMediaPartial =
@"
media(first: 1) {
    edges{
        node {
            id
            alt
            mediaContentType
            preview {
                image {
                    altText
                    height
                    width
                    url
                    ##METAFIELDS_PARTIAL##
                }
            }
        }
    }
}
";

        private const string ProductsGraphQLQuery_MetafieldsPartial =
@"
    metafields(first:##FIRST##, namespace: $metaFieldNamespace) {
        edges {
            node {
                id
                key
                namespace
                value
            }
        }
        pageInfo {
            hasNextPage
            endCursor
        }
    } 
";
        private const string ProductsGraphQLQuery_ImagesPartial =
@"
    media(first:1) {
        edges{
            node {
                id
                alt
                mediaContentType
                preview {
                    image {
                        altText
                        height
                        width
                        url
                        ##METAFIELDS_PARTIAL##
                    }
                }
            }
        }
        pageInfo{
            hasNextPage
            endCursor
        }
    }   
";

        private const string VariantsGraphQLQuery =
@"
query ProductVariants($first: Int, $after: String, $query: String ##METAFIELD_NAMESPACE_VAR##) {
    productVariants(first: $first, after: $after, query: $query) {
        edges {
            node {
                selectedOptions {
                    name
                    value
                }
                ##FIELDS##
                ##MEDIA_PARTIAL##
                ##METAFIELDS_PARTIAL##
            }
        }
        pageInfo {
            hasNextPage
            endCursor
        }
    }
}
";

        private const string ProductMetafieldsGraphQLQuery =
@"
query ProductMetafields($owner_id: ID!, $mfnamespace: String, $first: Int, $after: String) {
    product(id: $owner_id) {
        metafields(first: $first, after: $after, namespace: $mfnamespace) {
            edges {
                node {
                    id
                    namespace
                    key
                    value
                }
            }
            pageInfo {
                endCursor
                hasNextPage
            }
        }
    }
}
";

        private const string ProductVariantMetafieldsGraphQLQuery =
@"
query VariantMetafields($owner_id: ID!, $mfnamespace: String, $first: Int, $after: String) {
    productVariant(id: $owner_id) {
        metafields(first: $first, after: $after, namespace: $mfnamespace) {
            edges {
                node {
                    id
                    namespace
                    key
                    value
                }
            }
            pageInfo {
                endCursor
                hasNextPage
            }
        }
    }
}
";

        private const string ProductImagesGraphQLQuery =
@"
query ProductImages($owner_id: ID!, $first: Int, $after: String ##METAFIELD_NAMESPACE_VAR##) {
    product(id: $owner_id) {
        media(first: $first, after: $after) {
            edges{
                node {
                    id
                    alt
                    mediaContentType
                    preview {
                        image {
                            id
                            altText
                            height
                            width
                            url
                            ##METAFIELDS_PARTIAL##
                        }
                    }
                }
            }
            pageInfo {
                endCursor
                hasNextPage
            }
        }
    }
}
";

    }
}
