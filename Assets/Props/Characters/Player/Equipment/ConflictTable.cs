using System;
using System.Collections;
using System.Collections.Generic;

public class UnsupportedTypeException : Exception {
    public UnsupportedTypeException(Type type)
        : base(string.Format("Error: the type '{0}' is not supported. Type must be an enum, or assignable to 'int'.", type.ToString())){}
}

public class ConflictTable : IEnumerable<KeyValuePair<int, int>>
{
    private bool[,] _conflicts;
    
    public ConflictTable(int size, bool exclusiveDiagonal = false)
    {
        _conflicts = new bool[size, size];
        
        if(exclusiveDiagonal)
            SetExclusiveDiagonal();
    }
    
    public void SetExclusiveDiagonal()
    {
        int size = _conflicts.GetLength(0);
        for(int i = 0; i < size; ++i)
            _conflicts[i, i] = true;
    }
    
    public void Add<T>(params T[] values)
    {
        if(!typeof(int).IsAssignableFrom(typeof(T)) && !typeof(T).IsEnum)
            throw new UnsupportedTypeException(typeof(T));
        
        int len = values.Length;
        
        for(int i = 0; i < len; ++i)
        {
            int a = (int)(object)values[i];
            
            for(int j = 0; j < len; ++j)
            {
                int b = (int)(object)values[j];
                _conflicts[a, b] = true;
            }
        }
    }
    
    public void Clear() {
        Array.Clear(_conflicts, 0, _conflicts.Length);
    }
    
    public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
    {
        int size = _conflicts.GetLength(0);
        
        for(int i = 0; i < size; ++i)
        {
            for(int j = 0; j < size; ++j)
            {
                if(_conflicts[i, j])
                    yield return new KeyValuePair<int, int>(i, j);
            }
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() {
        return this.GetEnumerator();
    }
    
    public bool IsConflict<T>(T a, T b)
    {
        if(!typeof(int).IsAssignableFrom(typeof(T)) && !typeof(T).IsEnum)
            throw new UnsupportedTypeException(typeof(T));
        
        int x = (int)(object)a;
        int y = (int)(object)b;
        return _conflicts[x, y];
    }
    
    public T? GetConflict<T>(T type, Predicate<T> filter = null) where T : struct
    {
        if(!typeof(int).IsAssignableFrom(typeof(T)) && !typeof(T).IsEnum)
            throw new UnsupportedTypeException(typeof(T));
        
        int i = (int)(object)type;
        int size = _conflicts.GetLength(0);
        
        for(int j = 0; j < size; ++j)
        {
            if(_conflicts[i, j])
            {
                T item = (T)(object)j;
                
                if(filter == null || filter(item))
                    return item;
            }
        }
        
        return null;
    }
}
