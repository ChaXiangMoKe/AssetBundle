using UnityEngine;
using System.Collections;

// ???
public class RGRes  {
   
    public string ResName { get; private set; }

    public int RefCount { get; private set; }

    private Object _resObj;

    public RGRes(string resname,Object obj)
    {
        ResName = resname;
        _resObj = obj;
    }

    public int IncRef()
    {
        RefCount++;
        return RefCount;
    }

    public int DecRef()
    {
        RefCount--;
        return RefCount;
    }
    
    public Object GetRes()
    {
        return _resObj;
    }

    public void UnLoad()
    {
        _resObj = null;
        RefCount = 0;
    }
}
