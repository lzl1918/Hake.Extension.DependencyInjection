using Hake.Extension.DependencyInjection.Abstraction.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.DependencyInjection.Utils
{
    internal sealed class TypedCacheItem<T>
    {
        public TypedCacheItem(string typeName, T item)
        {
            TypeName = typeName;
            Item = item;
        }

        internal string TypeName { get; }
        internal T Item { get; }
    }

    public class TypedCache<T>
    {
        private int capacity;
        public int Capacity => capacity;
        private LinkedList<TypedCacheItem<T>> items;

        public TypedCache(int capacity)
        {
            this.capacity = capacity;
            items = new LinkedList<TypedCacheItem<T>>();
        }

        public bool TryGetItem(Type type, out T item)
        {
            string fullName = type.FullName;
            var node = items.First;
            while (node != null)
            {
                if (node.Value.TypeName == fullName)
                {
                    item = node.Value.Item;
                    items.Remove(node);
                    items.AddFirst(node);
                    return true;
                }
                node = node.Next;
            }
            item = default(T);
            return false;
        }

        public bool GetOrInsert(Type type, out T item, Func<Type, T> insertFactory)
        {
            string fullName = type.FullName;
            var node = items.First;
            while (node != null)
            {
                if (node.Value.TypeName == fullName)
                {
                    item = node.Value.Item;
                    items.Remove(node);
                    items.AddFirst(node);
                    return true;
                }
                node = node.Next;
            }

            item = insertFactory(type);
            if (items.Count >= capacity)
                items.RemoveLast();
            items.AddFirst(new TypedCacheItem<T>(fullName, item));
            return false;
        }
    }
}
