using Cql.Middleware.Library.Shopify.Inventory;

namespace Cql.Middleware.Library.Shopify.Store
{
    public class Location
    {
        /// <summary>
        /// A globally-unique identifier.
        /// </summary>
        public String Id { get; set; } = String.Empty;

        /// <summary>
        /// The name of the location.
        /// </summary>
        public String Name { get; set; } = String.Empty;

        /// <summary>
        /// Whether this location can be reactivated.
        /// </summary>
        public Boolean Activatable { get; set; }

        /// <summary>
        /// Whether this location can be deactivated.
        /// </summary>
        public Boolean Deactivatable { get; set; }

        /// <summary>
        /// The date and time ([ISO 8601 format](http://en.wikipedia.org/wiki/ISO_8601)) that the location was deactivated at. For example, 3:30 pm on September 7, 2019 in the time zone of UTC (Universal Time Coordinated) is represented as `"2019-09-07T15:50:00Z`".
        /// </summary>
        public String? DeactivatedAt { get; set; }

        /// <summary>
        /// Whether this location can be deleted.
        /// </summary>
        public Boolean Deletable { get; set; }

        /// <summary>
        /// Whether this location can fulfill online orders.
        /// </summary>
        public Boolean FulfillsOnlineOrders { get; set; }

        /// <summary>
        /// Whether this location has active inventory.
        /// </summary>
        public Boolean HasActiveInventory { get; set; }

        /// <summary>
        /// Whether this location has orders that need to be fulfilled.
        /// </summary>
        public Boolean HasUnfulfilledOrders { get; set; }

        /// <summary>
        /// Whether the location is active.
        /// </summary>
        public Boolean IsActive { get; set; }

        /// <summary>
        /// The ID of the corresponding resource in the REST Admin API.
        /// </summary>
        public String LegacyResourceId { get; set; } = String.Empty;

        /// <summary>
        /// Whether this location is used for calculating shipping rates. In multi-origin shipping mode, this flag is ignored.
        /// </summary>
        public Boolean ShipsInventory { get; set; }

        /// <summary>
        /// A list of the quantities of the inventory items that can be stocked at this location.
        /// </summary>
        public IEnumerable<InventoryLevel> InventoryLevels { get; set; } = Enumerable.Empty<InventoryLevel>();
    }
}
