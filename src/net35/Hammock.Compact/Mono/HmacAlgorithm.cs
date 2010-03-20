#region License

// TweetSharp
// Copyright (c) 2010 Daniel Crenna and Jason Diller
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography
{
    // References:
    // a.	FIPS PUB 198: The Keyed-Hash Message Authentication Code (HMAC), 2002 March.
    //	http://csrc.nist.gov/publications/fips/fips198/fips-198a.pdf
    // b.	Internet RFC 2104, HMAC, Keyed-Hashing for Message Authentication
    //	(include C source for HMAC-MD5)
    //	http://www.ietf.org/rfc/rfc2104.txt
    // c.	IETF RFC2202: Test Cases for HMAC-MD5 and HMAC-SHA-1
    //	(include C source for HMAC-MD5 and HAMAC-SHA1)
    //	http://www.ietf.org/rfc/rfc2202.txt
    // d.	ANSI X9.71, Keyed Hash Message Authentication Code.
    //	not free :-(
    //	http://webstore.ansi.org/ansidocstore/product.asp?sku=ANSI+X9%2E71%2D2000

    // Generic HMAC mechanisms - most of HMAC work is done in here.
    // It should work with any hash function e.g. MD5 for HMACMD5 (RFC2104)
    internal class HMACAlgorithm
    {
        private HashAlgorithm algo;
        private BlockProcessor block;
        private byte[] hash;
        private string hashName;
        private byte[] key;

        public HMACAlgorithm(string algoName)
        {
            CreateHash(algoName);
        }

        public HashAlgorithm Algo
        {
            get { return algo; }
        }

        public string HashName
        {
            get { return hashName; }
            set { CreateHash(value); }
        }

        public byte[] Key
        {
            get { return key; }
            set
            {
                if ((value != null) && (value.Length > 64))
                    key = algo.ComputeHash(value);
                else if (value != null)
                {
                    key = (byte[]) value.Clone();
                }
            }
        }

        ~HMACAlgorithm()
        {
            Dispose();
        }

        private void CreateHash(string algoName)
        {
            algo = HashAlgorithm.Create(algoName);
            hashName = algoName;
            block = new BlockProcessor(algo, 8);
        }

        public void Dispose()
        {
            if (key != null)
                Array.Clear(key, 0, key.Length);
        }

        public void Initialize()
        {
            hash = null;
            block.Initialize();
            var buf = KeySetup(key, 0x36);
            algo.Initialize();
            block.Core(buf);
            // zeroize key
            Array.Clear(buf, 0, buf.Length);
        }

        private static byte[] KeySetup(byte[] key, byte padding)
        {
            var buf = new byte[64];

            for (var i = 0; i < key.Length; ++i)
                buf[i] = (byte) (key[i] ^ padding);

            for (var i = key.Length; i < 64; ++i)
                buf[i] = padding;

            return buf;
        }

        public void Core(byte[] rgb, int ib, int cb)
        {
            block.Core(rgb, ib, cb);
        }

        public byte[] Final()
        {
            block.Final();
            var intermediate = algo.Hash;

            var buf = KeySetup(key, 0x5C);
            algo.Initialize();
            algo.TransformBlock(buf, 0, buf.Length, buf, 0);
            algo.TransformFinalBlock(intermediate, 0, intermediate.Length);
            hash = algo.Hash;
            //algo.Clear();
            // zeroize sensitive data
            Array.Clear(buf, 0, buf.Length);
            Array.Clear(intermediate, 0, intermediate.Length);
            return hash;
        }
    }
}