using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Hammock.Web
{
    /// <summary>
    /// A collection of parameters for use with web queries.
    /// </summary>
    public class WebParameterCollection : IList<WebParameter>
    {
        private IList<WebParameter> _parameters;

        /// <summary>
        /// Gets the <see cref="WebParameter"/> with the specified name.
        /// If more than one parameter exists with the same name, null is returned.
        /// </summary>
        /// <value></value>
        public WebParameter this[string name]
        {
            get { return this.SingleOrDefault(p => p.Name.Equals(name)); }
        }

        /// <summary>
        /// Gets the web parameter names.
        /// </summary>
        /// <value>The names.</value>
        public IEnumerable<string> Names
        {
            get { return _parameters.Select(p => p.Name); }
        }

        /// <summary>
        /// Gets the web parameter values.
        /// </summary>
        /// <value>The values.</value>
        public IEnumerable<string> Values
        {
            get { return _parameters.Select(p => p.Value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebParameterCollection"/> class.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public WebParameterCollection(IEnumerable<WebParameter> parameters)
        {
            _parameters = new List<WebParameter>(parameters);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="WebParameterCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public WebParameterCollection(NameValueCollection collection) : this()
        {
            AddCollection(collection);
        }

        private void AddCollection(NameValueCollection collection)
        {
            foreach (var parameter in collection.AllKeys.Select(key => new WebParameter(key, collection[key])))
            {
                _parameters.Add(parameter);
            }
        }

        public void AddRange(NameValueCollection collection)
        {
            AddCollection(collection);
        }
#else
        public WebParameterCollection(IDictionary<string, string> collection)
            : this()
        {
            AddCollection(collection);
        }

        private void AddCollection(IDictionary<string, string> collection)
        {
            foreach (var key in collection.Keys)
            {
                var parameter = new WebParameter(key, collection[key]);
                _parameters.Add(parameter);
            }
        }

        public void AddRange(IDictionary<string, string> collection)
        {
            AddCollection(collection);
        }
#endif

        private void AddCollection(IEnumerable<WebParameter> collection)
        {
            foreach (var parameter in collection)
            {
                _parameters.Add(new WebParameter(parameter.Name, parameter.Value));
            }
        }

        public void AddRange(WebParameterCollection collection)
        {
            AddCollection(collection);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebParameterCollection"/> class.
        /// </summary>
        public WebParameterCollection()
        {
            _parameters = new List<WebParameter>(0);
        }

        public void Sort(Comparison<WebParameter> comparison)
        {
            var sorted = new List<WebParameter>(_parameters);
            sorted.Sort(comparison);
            _parameters = sorted;
        }

        public bool RemoveAll(IEnumerable<WebParameter> parameters)
        {
            var success = true;
            var array = parameters.ToArray();
            for (var p = 0; p < array.Length; p++)
            {
                var parameter = array[p];
                success &= _parameters.Remove(parameter);
            }
            return success && array.Length > 0;
        }

        public void Add(string name, string value)
        {
            _parameters.Add(new WebParameter(name, value));
        }

        #region IList<WebParameter> Members

        public IEnumerator<WebParameter> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(WebParameter parameter)
        {
            _parameters.Add(parameter);
        }

        public void Clear()
        {
            _parameters.Clear();
        }

        public bool Contains(WebParameter parameter)
        {
            return _parameters.Contains(parameter);
        }

        public void CopyTo(WebParameter[] parameters, int arrayIndex)
        {
            _parameters.CopyTo(parameters, arrayIndex);
        }

        public bool Remove(WebParameter parameter)
        {
            return _parameters.Remove(parameter);
        }

        public int Count
        {
            get { return _parameters.Count; }
        }

        public bool IsReadOnly
        {
            get { return _parameters.IsReadOnly; }
        }

        public int IndexOf(WebParameter parameter)
        {
            return _parameters.IndexOf(parameter);
        }

        public void Insert(int index, WebParameter parameter)
        {
            _parameters.Insert(index, parameter);
        }

        public void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        public WebParameter this[int index]
        {
            get { return _parameters[index]; }
            set { _parameters[index] = value; }
        }

        #endregion
    }
}