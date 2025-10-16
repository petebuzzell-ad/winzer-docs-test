using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Winzer.Library.Salsify
{
    public class SalsifyProduct : IComparable<SalsifyProduct>, IEquatable<SalsifyProduct>
    {
        public SalsifyProduct() : this(null) { }

        public SalsifyProduct(SalsifyProduct? parent)
        {
            _properties = new Dictionary<string,object?>();
            Parent = parent;
        }

        private readonly IDictionary<string, object?> _properties;

        public IDictionary<string, object?> Properties
        {
            get { return _properties; }
        }

        [JsonIgnore]
        public SalsifyProduct? Parent { get; private set; }

        public SortedSet<SalsifyProduct> Children { get; } = new SortedSet<SalsifyProduct>();

        [JsonIgnore]
        public bool SendToShopify {
            get
            {
                if (GetPropertyValue<bool?>("Send To Shopify").GetValueOrDefault(false))
                {
                    if (Children.Any())
                    {
                        return Children.Any(c => c.SendToShopify);
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public DateTime? CreatedAt { get; set; }

        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }

        public string? SalsifyId { get; set; }

        public string? OracleId { get; set; }

        [JsonIgnore]
        public string? ShopifyId { get; set; }

        [JsonIgnore]
        public string? ShopifyHash { get; set; }

        [JsonIgnore]
        public string? TodayDate { get; set; }

        public string? ParentId { get; set; }

        public string? DataInheritanceHierarchyLevelId { get; set; }

        public T? GetPropertyValue<T>(string propertyName)
        {
            if (_properties.ContainsKey(propertyName))
            {
                return (T?)_properties[propertyName];
            }
            else if (Parent != null)
            {
                return Parent.GetPropertyValue<T>(propertyName);
            }
            else
            {
                return default;
            }
        }

        public string? GetPropertyValueAsString(string propertyName)
        {
            object? value = GetPropertyValue<object>(propertyName);
            if (value is IEnumerable<object>) value = (value as IEnumerable<object>)?.FirstOrDefault();
            return value?.ToString()?.Trim();
        }

        public object? this[string i]
        {
            get { return GetPropertyValue<object>(i); }
            set { _properties[i] = value; }
        }

        public string GetHash()
        {
            using var sha = SHA256.Create();
            string json = JsonConvert.SerializeObject(this);
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
            var sb = new StringBuilder();
            foreach ( var byt in hash )
            {
                sb.Append(byt.ToString("x2"));
            };

            return sb.ToString();
        }

        public int CompareTo(SalsifyProduct? other)
        {
            return SalsifyId?.CompareTo(other?.SalsifyId) ?? -1;
        }

        public bool Equals(SalsifyProduct? other)
        {
            return SalsifyId?.Equals(other?.SalsifyId) ?? false;
        }

        public override int GetHashCode()
        {
            return (SalsifyId ?? String.Empty).GetHashCode();
        }
    }

    public class QuantifiedProductReference
    {
        public string ProductId { get; set; } = string.Empty;

        public int Qty { get; set; }
    }
}
