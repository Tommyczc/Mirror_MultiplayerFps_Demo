using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackboardModule : GameModule
{
    private Dictionary<string, Blackboard> _blackboards;
    
    public override void OnRegister()
    {
        _blackboards = new Dictionary<string, Blackboard>();
        GetOrCreateBoard("default");
    }

    public Blackboard GetOrCreateBoard(string id) {
        if (_blackboards.ContainsKey(id))
        {
            return _blackboards[id];
        }

        Blackboard newBoard = new Blackboard();
        _blackboards.Add(id, newBoard);
        return newBoard;
    }

    public override void OnUnregister()
    {
        //
    }

    public class Blackboard
    {
        private Dictionary<string, BlackboardItem> _items;

        public Blackboard()
        {
            _items = new Dictionary<string, BlackboardItem>();
        }

        public void Set(string id, object v)
        {
            if (_items.ContainsKey(id))
            {
                _items[id].Set(v);
            }
            else
            {
                BlackboardItem item = new BlackboardItem();
                item.Set(v);
                _items.Add(id, item);
            }
        }

        public T Get<T>(string id, T fallback)
        {
            if (_items.ContainsKey(id))
            {
                return _items[id].Get<T>();
            }
            return fallback;
        }
    }
    
    class BlackboardItem
    {
        private object _value;

        public void Set(object v)
        {
            _value = v;
        }

        public T Get<T>()
        {
            return (T) _value;
        }
    }
}
