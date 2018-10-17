using System.Collections;
using System.Collections.Generic;

namespace SpriteSleeper
{
    public class GenericPool<T> where T : class
    {
        private System.Func<T> _createFunction = null;
        private System.Action<T> _resetFunction = null;
        private List<T> _itemList;
        private List<T> _usedList;

        public GenericPool(uint initialCapacity, System.Func<T> createFunc) :
            this(initialCapacity, createFunc, null)
        {}

        public GenericPool(uint initialCapacity, System.Func<T> createFunc, System.Action<T> resetFunction)
        {
            _createFunction = createFunc;
            _resetFunction = resetFunction;

            _itemList = new List<T>((int)initialCapacity);
            _usedList = new List<T>((int)initialCapacity);

            T value = null;
            for ( int i = 0; i < initialCapacity; i++ )
            {
                value = _createFunction();
                InternalPut(value);
            }
        }

        public T Get()
        {
            T value = null;
            if (_itemList.Count == 0)
            {
                value = _createFunction();
            } else
            {
                int index = _itemList.Count - 1;
                value = _itemList[index];
                _itemList.RemoveAt(index);
            }

            _usedList.Add(value);

            return value;
        }

        public void Put(T item)
        {
#if UNITY_EDITOR
            if(!_usedList.Contains(item))
            {
                throw new System.Exception("Returning an item to GenericPool that wasn't created by the pool.");
            }
#endif
            _usedList.Remove(item);
            InternalPut(item);
        }

        void InternalPut(T item)
        {
            _itemList.Add(item);

            if (_resetFunction != null)
            {
                _resetFunction(item);
            }
        }

        public void Destroy()
        {
            _usedList.Clear();
            _usedList = null;
            _itemList.Clear();
            _itemList = null;
        }
    }
}