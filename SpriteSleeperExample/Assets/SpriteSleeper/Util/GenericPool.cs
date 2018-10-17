using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpriteSleeper
{
    public class GenericPool<T>
    {
        private System.Func<T> _createFunction = null;
        private System.Action<T> _resetFunction = null;
        private List<T> _itemList;

        public GenericPool(uint initialCapacity, System.Func<T> createFunc) :
            this(initialCapacity, createFunc, null)
        {}

        public GenericPool(uint initialCapacity, System.Func<T> createFunc, System.Action<T> resetFunction)
        {
            _createFunction = createFunc;
            _resetFunction = resetFunction;

            _itemList = new List<T>();

            for( int i = 0; i < initialCapacity; i++ )
            {
                _itemList.Add(_createFunction());
            }
        }

        public T Get()
        {
            if( _itemList.Count == 0)
            {
                return _createFunction();
            }

            T value = _itemList[0];
            _itemList.RemoveAt(0);

            return value;
        }

        public void Put(T item)
        {
            if( _resetFunction != null )
            {
                _resetFunction(item);
            }
            _itemList.Add(item);
        }

        public void Destroy()
        {
            _itemList.Clear();
            _itemList = null;
        }
    }
}