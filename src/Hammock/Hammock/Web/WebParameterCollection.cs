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
        public virtual WebParameter this[string name]
        {
            get { return this.SingleOrDefault(p => p.Name.Equals(name)); }
        }

        /// <summary>
        /// Gets the web parameter names.
        /// </summary>
        /// <value>The names.</value>
        public virtual IEnumerable<string> Names
        {
            get { return _parameters.Select(p => p.Name); }
        }

        /// <summary>
        /// Gets the web parameter values.
        /// </summary>
        /// <value>The values.</value>
        public virtual IEnumerable<string> Values
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

        public virtual void AddRange(NameValueCollection collection)
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

        /// <summary>
        /// Initializes a new instance of the <see cref="WebParameterCollection"/> class.
        /// </summary>
        public WebParameterCollection()
        {
            _parameters = new List<WebParameter>(0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebParameterCollection"/> class.
        /// </summary>
        public WebParameterCollection(int capacity)
        {
            _parameters = new List<WebParameter>(capacity);
        }

        private void AddCollection(IEnumerable<WebParameter> collection)
        {
            foreach (var parameter in collection)
            {
                _parameters.Add(new WebParameter(parameter.Name, parameter.Value));
            }
        }

        public virtual void AddRange(WebParameterCollection collection)
        {
            AddCollection(collection);
        }
        
        public virtual void Sort(Comparison<WebParameter> comparison)
        {
            var sorted = new List<WebParameter>(_parameters);
            sorted.Sort(comparison);
            _parameters = sorted;
        }

        public virtual bool RemoveAll(IEnumerable<WebParameter> parameters)
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

        public virtual void Add(string name, string value)
        {
            _parameters.Add(new WebParameter(name, value));
        }

        #region IList<WebParameter> Members

        public virtual IEnumerator<WebParameter> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(WebParameter parameter)
        {
            _parameters.Add(parameter);
        }

        public virtual void Clear()
        {
            _parameters.Clear();
        }

        public virtual bool Contains(WebParameter parameter)
        {
            return _parameters.Contains(parameter);
        }

        public virtual void CopyTo(WebParameter[] parameters, int arrayIndex)
        {
            _parameters.CopyTo(parameters, arrayIndex);
        }

        public virtual bool Remove(WebParameter parameter)
        {
            return _parameters.Remove(parameter);
        }

        public virtual int Count
        {
            get { return _parameters.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return _parameters.IsReadOnly; }
        }

        public virtual int IndexOf(WebParameter parameter)
        {
            return _parameters.IndexOf(parameter);
        }

        public virtual void Insert(int index, WebParameter parameter)
        {
            _parameters.Insert(index, parameter);
        }

        public virtual void RemoveAt(int index)
        {
            _parameters.RemoveAt(index);
        }

        public virtual WebParameter this[int index]
        {
            get { return _parameters[index]; }
            set { _parameters[index] = value; }
        }

        #endregion
    }
}