using Cql.Middleware.Library.Shopify.Common;
using Cql.Middleware.Library.Shopify.Products;

namespace Cql.Middleware.Library.Shopify.Inventory
{
    public class InventoryItem
    {
        /// <summary>
        /// A globally-unique identifier.
        /// </summary>
        public String Id { get; set; } = String.Empty;

        /// <summary>
        /// Inventory item SKU.
        /// </summary>
        public String? Sku { get; set; }

        /// <summary>
        /// The ISO 3166-1 alpha-2 country code of where the item originated from.
        /// </summary>
        public String? CountryCodeOfOrigin { get; set; }

        /// <summary>
        /// The number of inventory items that share the same SKU with this item.
        /// </summary>
        public Int32 DuplicateSkuCount { get; set; }

        /// <summary>
        /// The harmonized system code of the item.
        /// </summary>
        public String? HarmonizedSystemCode { get; set; }

        /// <summary>
        /// The URL that points to the inventory history for the item.
        /// </summary>
        public Uri? InventoryHistoryUrl { get; set; }

        /// <summary>
        /// The ID of the corresponding resource in the REST Admin API.
        /// </summary>
        public String LegacyResourceId { get; set; } = String.Empty;

        /// <summary>
        /// The number of locations where this inventory item is stocked.
        /// </summary>
        public Int32 LocationsCount { get; set; }

        /// <summary>
        /// The ISO 3166-2 alpha-2 province code of where the item originated from.
        /// </summary>
        public String? ProvinceCodeOfOrigin { get; set; }

        /// <summary>
        /// Whether the inventory item requires shipping.
        /// </summary>
        public Boolean RequiresShipping { get; set; }

        /// <summary>
        /// Whether inventory levels are tracked for the item.
        /// </summary>
        public Boolean Tracked { get; set; }

        /// <summary>
        /// The date and time when the inventory item was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the inventory item was updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Whether the value of the tracked field for the inventory item can be changed.
        /// </summary>
        public EditableProperty? TrackedEditable { get; set; }

        /// <summary>
        /// Unit cost associated with the inventory item.
        /// </summary>
        public Money? UnitCost { get; set; }

        /// <summary>
        /// The inventory item's quantities at the specified location.
        /// </summary>
        public InventoryLevel? InventoryLevel { get; set; }

        /// <summary>
        /// A list of the inventory item's quantities for each location that the inventory item can be stocked at.
        /// </summary>
        public IEnumerable<InventoryLevel> InventoryLevels { get; set; } = Enumerable.Empty<InventoryLevel>();

        /// <summary>
        /// The variant that owns this inventory item.
        /// Product Variant Placeholder
        /// </summary>
        public ShopifyProductVariant? Variant { get; set; }
    }
}
