using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hammock.Silverlight.Compat
{
    public class NameValueCollection : List<KeyValuePair<string, string>>
    {
        public new string this[int index]
        {
            get
            {
                return base[index].Value;
            }
        }

        public string this[string name]
        {
            get
            {
                return this.Single(kv => kv.Key.Equals(name)).Value;
            }
        }

        public NameValueCollection(int capacity) : base(capacity)
        {
            
        }

        public void Add(string name, string value)
        {
            Add(new KeyValuePair<string, string>(name, value));
        }

        public IEnumerable AllKeys
        {
            get
            {
                return this.Select(pair => pair.Key).Cast<object>();
            }
        }
    }
}
