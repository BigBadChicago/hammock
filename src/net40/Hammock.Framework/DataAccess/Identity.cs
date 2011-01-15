using System;

namespace Hammock.Framework.DataAccess
{
    public struct Identity : IComparable<ValueType>
    {
        static Identity()
        {
            None = new Identity();
        }

        private Identity(ValueType key)
            : this()
        {
            Key = key;
        }

        private ValueType Key { get; set; }

        public static Identity None { get; private set; }

        public int CompareTo(ValueType other)
        {
            if(other is long)
            {
                return ((long) this).CompareTo(other);
            }

            if(other is int)
            {
                return ((int) this).CompareTo(other);
            }

            if(other is Guid)
            {
                return ((Guid) this).CompareTo(other);
            }

            return -1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj.GetType() == typeof (Identity) && Equals((Identity) obj);
        }

        public override int GetHashCode()
        {
            return (Key != null ? Key.GetHashCode() : 0);
        }

        public static bool operator ==(Identity left, Identity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Identity left, Identity right)
        {
            return !left.Equals(right);
        }

        public bool Equals(Identity other)
        {
            if (other.Key == null && Key == null)
            {
                return true;
            }

            if (other.Key == null && Key != null)
            {
                return false;
            }

            var internalKey = Key;

            if (internalKey == null)
            {
                if (other.Key != null)
                {
                    var type = other.Key.GetType();
                    if (type.FullName.Equals("System.Guid"))
                    {
                        internalKey = Guid.Empty;
                    }
                    else
                    {
                        // long, int
                        internalKey = -1;
                    }
                }
            }

            return Equals(other.Key, internalKey);
        }

        public static implicit operator Identity(Guid value)
        {
            return new Identity(value);
        }

        public static implicit operator Identity(int value)
        {
            return new Identity(value);
        }

        public static implicit operator Identity(long value)
        {
            return new Identity(value);
        }

        public static implicit operator Guid(Identity value)
        {
            if (value.Key == null)
            {
                return Guid.Empty;
            }

            return (Guid) value.Key;
        }

        public static implicit operator int(Identity value)
        {
            if (value.Key == null)
            {
                return -1;
            }

            return (int) value.Key;
        }

        public static implicit operator long(Identity value)
        {
            if (value.Key == null)
            {
                return -1;
            }

            return (long) value.Key;
        }

        public override string ToString()
        {
            if(this == None)
            {
                return "Identity.None";
            }

            return Key == null ? base.ToString() : Key.ToString();
        }
    }
}