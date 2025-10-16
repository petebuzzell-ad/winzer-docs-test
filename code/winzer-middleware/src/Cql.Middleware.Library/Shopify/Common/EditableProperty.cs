namespace Cql.Middleware.Library.Shopify.Common
{
    public class EditableProperty
    {
        /// <summary>
        /// Whether the attribute is locked for editing.
        /// </summary>
        public Boolean Locked { get; set; }

        /// <summary>
        /// The reason the attribute is locked for editing.
        /// FormattedString can contain HTML https://shopify.dev/api/admin-graphql/2022-04/scalars/FormattedString
        /// </summary>
        public String Reason { get; set; } = String.Empty;
    }
}
