using System.Collections;
using System.Collections.Generic;

namespace SpriteSleeper
{
    public class GenericPool<T> where T : class
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

            T value = null;
            for ( int i = 0; i < initialCapacity; i++ )
            {
                value = _createFunction();
                Put(value);
            }
        }

        public T Get()
        {
            if (_itemList.Count == 0)
            {
                return _createFunction();
            }

            int index = _itemList.Count - 1;
            T value = _itemList[index];
            _itemList.RemoveAt(index);

            return value;
        }

        public void Put(T item)
        {
            _itemList.Add(item);

            if ( _resetFunction != null )
            {
                _resetFunction(item);
            }
        }

        public void Destroy()
        {
            _itemList.Clear();
            _itemList = null;
        }
    }
}