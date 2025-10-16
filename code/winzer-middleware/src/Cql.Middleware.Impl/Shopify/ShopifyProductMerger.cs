using Cql.Middleware.Library.Salsify;
using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;
using Cql.Middleware.Library.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cql.Middleware.Impl.Shopify
{
    public class ShopifyProductMerger : IShopifyProductMerger
    {
        // Metafield Keys that will be checked for deletion.
        private readonly IEnumerable<string> _metafieldKeys;

        public ShopifyProductMerger(IEnumerable<string> metafieldKeys)
        {
            var manualMetafieldKeys = new List<string>{
                "add_on_products",
                "attributes_json",
                "categories"
            };
            _metafieldKeys = metafieldKeys.Concat(manualMetafieldKeys);
        }

        public ShopifyProductWithDeletes MergeProducts(ShopifyProduct original, ShopifyProduct updated)
        {
            // Handle merging basic product info.
            var rval = new ShopifyProductWithDeletes()
            {
                CreatedAt = original.CreatedAt,
                UpdatedAt = original.UpdatedAt,
                DescriptionHtml = updated.DescriptionHtml ?? original.DescriptionHtml,
                Handle = original.Handle,
                Id = original.Id,
                ProductType = original.ProductType,
                PublishedAt = original.PublishedAt,
                PublishedScope = original.PublishedScope,
                Status = updated.Status ?? original.Status,
                Title = updated.Title ?? original.Title,
                Tags = updated.Tags ?? original.Tags,
            };

            bool productHasImageChanges = false;
            foreach (var image in updated.Images)
            {
                var match = original.Images.FirstOrDefault(i => i.IsSameImageAs(image));
                if (match != null)
                {
                    // We already have this image on the product, so keep it as-is.
                    rval.Images.Add(match);
                    //Console.WriteLine($"Keeping original image: {match.Alt}");
                }
                else
                {
                    // New image.
                    rval.Images.Add(image);
                    //Console.WriteLine($"Adding new image: {image.Alt}");
                    productHasImageChanges = true;
                }
            }

            foreach (var image in original.Images)
            {
                var match = updated.Images.FirstOrDefault(i => i.IsSameImageAs(image));
                if (match == null)
                {
                    //Console.WriteLine($"Deleting image: {image.Alt}");
                    // This one isn't in the updated product, so we need to delete it.
                    rval.MediaIdsToDelete.Add(image.MediaId);
                    productHasImageChanges = true;
                }
            }

            // Options
            foreach (var option in updated.Options)
            {
                var match = original.Options.FirstOrDefault(o => o.Name == option.Name);
                if (match != null)
                {
                    rval.Options.Add(match);

                    // Product already has this option, check if there are any new
                    // option values that we need to add to it.
                    var missingOptionValues = new List<ShopifyProductOptionValue>();
                    foreach ( var optionValue in option.Values)
                    {
                        if (!match.Values.Any(v => v.Name == optionValue.Name))
                        {
                            missingOptionValues.Add(optionValue);
                        }
                    }

                    if (missingOptionValues.Any())
                    {
                        var optionUpdate = new ShopifyProductOptionUpdate()
                        {
                            OptionId = match.Id,
                            OptionName = match.Name,
                            OptionValuesToAdd = missingOptionValues.Select(o => o.Name).ToArray()
                        };
                        rval.OptionUpdates.Add(optionUpdate);
                    }
                }
                else
                {
                    // Is this really a new option, or are we just changing the name of an existing option?
                    if (updated.Options.Count() == original.Options.Count())
                    {
                        // If we already have the same number of options that we had before, then we have to assume that this is just a re-naming of an existing option
                        // so we need to figure out which one it is.
                        var optionToReplace = original.Options
                            .Where(o => !updated.Options.Select(u => u.Name).Contains(o.Name))
                            .FirstOrDefault();

                        if (optionToReplace != null)
                        {
                            optionToReplace.Name = option.Name;
                            optionToReplace.Values = option.Values;
                            rval.Options.Add(optionToReplace);
                            rval.OptionUpdates.Add(new ShopifyProductOptionUpdate()
                            {
                                OptionId = optionToReplace.Id,
                                OptionName = option.Name,
                                OptionValuesToAdd = option.Values.Select(o => o.Name).ToArray()
                            });
                        }
                    }
                    else
                    {
                        if (updated.Options.Count() > 3) {
                            throw new Exception($"Error merging product {original.Title}.  Could not add new option `{option.Name}` as the product already has 3 options and none of them appear to be unused.");
                        }

                        rval.Options.Add(option);

                        // Product does not already have this option, so it will need to be created.
                        rval.OptionUpdates.Add(new ShopifyProductOptionUpdate()
                        {
                            OptionId = null,
                            OptionName = option.Name,
                            OptionValuesToAdd = option.Values.Select(o => o.Name).ToArray()
                        });
                    }
                }
            }

            // Metafields.
            foreach (var metafield in updated.MetaFields)
            {
                // Assuming here that shopify doesn't delete metafields if you don't include them in the update.
                // That appears to be the behavior based on testing.
                var match = original.MetaFields.FirstOrDefault(o => o.Key == metafield.Key);
                if (match == null)
                {
                    // New metafield that we need to add.
                    rval.MetaFields.Add(metafield);
                }
                else if (match.Value != metafield.Value)
                {
                    // Need to update the existing one.
                    match.Value = metafield.Value;
                    rval.MetaFields.Add(match);
                }
            }

            foreach (var metafield in original.MetaFields)
            {
                if (String.IsNullOrEmpty(metafield.Id) || !_metafieldKeys.Contains(metafield.Key))
                    continue;

                var match = updated.MetaFields.FirstOrDefault(o => o.Key == metafield.Key);
                if (match == null)
                {
                    // Need to delete the existing one
                    rval.MetafieldIdsToDelete.Add(metafield.Id);
                }
            }

            // Variants.
            foreach (var variant in updated.Variants)
            {
                var originalVariant = original.Variants.FirstOrDefault(v => v.SKU == variant.SKU);
                var mergedVariant = MergeVariant(originalVariant, variant, original.Images, updated, productHasImageChanges);
                rval.Variants.Add(mergedVariant);

                foreach (var metafield in originalVariant?.Metafields ?? new List<ShopifyMetaField>())
                {
                    if (String.IsNullOrEmpty(metafield.Id) || !_metafieldKeys.Contains(metafield.Key))
                        continue;

                    var match = variant.Metafields.FirstOrDefault(o => o.Key == metafield.Key);
                    if (match == null && mergedVariant is ShopifyProductVariantWithDeletes)
                    {
                        ((ShopifyProductVariantWithDeletes)mergedVariant).MetafieldIdsToDelete.Add(metafield.Id);
                    }
                }
            }

            return rval;
        }

        private static ShopifyProductVariant MergeVariant(ShopifyProductVariant? original, ShopifyProductVariant updated, IList<ShopifyProductImage> originalImages, ShopifyProduct updatedProduct, bool productHasImageChanges)
        {
            // Shortcut if this is a totally new variant.
            if (original == null)
            {
                var tmp = new ShopifyProductImage() { Src = updated.MediaSrc, Alt = updated.MediaAlt };

                var match = originalImages.FirstOrDefault(i => i.IsSameImageAs(tmp));
                if (match != null)
                {
                    updated.MediaId = match.MediaId;
                    updated.MediaSrc = null;
                }
                return updated;
            }

            var rval = new ShopifyProductVariantWithDeletes()
            {
                Id = original.Id,
                SKU = updated.SKU ?? original.SKU,
                Position = updated.Position ?? original.Position,
                Option1 = updated.Option1,
                Option2 = updated.Option2,
                Option3 = updated.Option3,
                Title = updated.Title ?? original.Title,
                Taxable = updated.Taxable ?? original.Taxable,
                TaxCode = updated.TaxCode ?? original.TaxCode,
                MediaId = original.MediaId,
                MediaAlt = updated.MediaAlt ?? original.MediaAlt,
                Weight = updated.Weight ?? original.Weight,
                WeightUnit = updated.WeightUnit ?? original.WeightUnit,
            };

            if (productHasImageChanges)
            {
                var updatedImage = new ShopifyProductImage() { Src = updated.MediaSrc, Alt = updated.MediaAlt };
                var originalImage = new ShopifyProductImage() { Src = original.MediaSrc, Alt = original.MediaAlt };
                if (updatedImage.IsSameImageAs(originalImage))
                {
                    // Nothing has changed.
                    if (!string.IsNullOrWhiteSpace(original.MediaId))
                    {
                        rval.MediaId = original.MediaId;
                        rval.MediaSrc = null;
                    }
                    else
                    {
                        rval.MediaId = null;
                        rval.MediaSrc = original.MediaSrc;
                    }
                }
                else
                {
                    // Image changed.  Might be a totally new image, or might be an image that is already
                    // associated to the product.. let's see.
                    var matchingImage = originalImages.FirstOrDefault(i => i.Src == original.MediaSrc);
                    if (matchingImage != null)
                    {
                        rval.MediaId = matchingImage.MediaId; // Matches another existing image.
                        rval.MediaSrc = null;
                    }
                    else
                    {
                        rval.MediaId = null;
                        rval.MediaSrc = updated.MediaSrc; // Doesn't match any existing images, must be a new one.
                    }
                }
            }
            else if (rval.MediaId == null)
            {
                // Noticed that sometimes this happens, but I'm not sure why/how.  Anyways, try to
                // correct it here by seeing if we can find an appropriate image for this variant.
                var matchingImage = originalImages.FirstOrDefault(i => i.Src == original.MediaSrc);
                if (matchingImage != null && !string.IsNullOrWhiteSpace(matchingImage.MediaId))
                {
                    rval.MediaId = matchingImage.MediaId;
                }
            }

            // Handle metafields.
            // Metafields.
            foreach (var metafield in updated.Metafields)
            {
                // Assuming here that shopify doesn't delete metafields if you don't include them in the update.
                // That appears to be the behavior based on testing.
                var match = original.Metafields.FirstOrDefault(o => o.Key == metafield.Key);
                if (match == null)
                {
                    // New metafield that we need to add.
                    rval.Metafields.Add(metafield);
                }
                else if (match.Value != metafield.Value)
                {
                    // Need to update the existing one.
                    match.Value = metafield.Value;
                    rval.Metafields.Add(match);
                }
            }

            return rval;
        }
    }
}
