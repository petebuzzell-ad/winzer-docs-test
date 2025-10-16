using Cql.Middleware.Library.Shopify.Store;

namespace Cql.Middleware.Library.Shopify.Inventory
{
    public class InventoryLevel
    {
        /// <summary>
        /// A globally-unique identifier.
        /// </summary>
        public String Id { get; set; } = String.Empty;

        /// <summary>
        /// The quantity of inventory items that are available at the inventory level's associated location.
        /// </summary>
        public Int32 Available { get; set; }

        /// <summary>
        /// Whether the inventory items associated with the inventory level can be deactivated.
        /// </summary>
        public Boolean CanDeactivate { get; set; }

        /// <summary>
        /// Describes either the impact of deactivating the inventory level, or why the inventory level can't be deactivated.
        /// </summary>
        public String? DeactivationAlert { get; set; }

        /// <summary>
        /// Describes, in HTML with embedded URLs, either the impact of deactivating the inventory level or why the inventory level can't be deactivated.
        /// </summary>
        public String? DeactivationAlertHtml { get; set; }

        /// <summary>
        /// The quantity of inventory items that are going to the inventory level's associated location.
        /// </summary>
        public Int32 Incoming { get; set; }

        /// <summary>
        /// The date and time when the inventory level was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the inventory level was updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// Inventory item associated with the inventory level.
        /// </summary>
        public InventoryItem? Item { get; set; }

        /// <summary>
        /// The location associated with the inventory level.
        /// </summary>
        public Location? Location { get; set; }
    }
}
