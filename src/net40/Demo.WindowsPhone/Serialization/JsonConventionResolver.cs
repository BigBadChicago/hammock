using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Demo.WindowsPhone.Serialization
{
    public class JsonConventionResolver : DefaultContractResolver
    {
        public class ToStringComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return x.ToString().CompareTo(y.ToString());
            }
        }
        
        protected override IList<JsonProperty> CreateProperties(JsonObjectContract contract)
        {
            var properties = base.CreateProperties(contract);

            foreach (var property in properties)
            {
                property.PropertyName = PascalCaseToElement(property.PropertyName);
            }

            var result = properties;
            return result;
        }

        private static string PascalCaseToElement(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var result = new StringBuilder();
            result.Append(char.ToLowerInvariant(input[0]));
            
            for (var i = 1; i < input.Length; i++)
            {
                if (char.IsLower(input[i]))
                {
                    result.Append(input[i]);
                }
                else
                {
                    result.Append("_");
                    result.Append(char.ToLowerInvariant(input[i]));
                }
            }

            return result.ToString();
        }
    }
}