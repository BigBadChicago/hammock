using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hammock.Caching
{
    public class CacheOptions
    {
        public CacheMode Mode { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
