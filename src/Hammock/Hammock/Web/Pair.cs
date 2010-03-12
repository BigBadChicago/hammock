namespace Hammock.Web
{
    /// <summary>
    /// A pair of class instances.
    /// </summary>
    /// <typeparam name="T">The first type.</typeparam>
    /// <typeparam name="K">The second type.</typeparam>
    public class Pair<T, K>
    {
        /// <summary>
        /// Gets or sets the first instance.
        /// </summary>
        /// <value>The first instance.</value>
        public T First { get; set; }

        /// <summary>
        /// Gets or sets the second instance.
        /// </summary>
        /// <value>The second instance.</value>
        public K Second { get; set; }
    }
}