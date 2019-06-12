using System;
using System.Collections.Generic;
using System.Text;

namespace CompareNetObjects.Delta
{
  public class Delta<T>
  {
    private readonly Action<T> _action;

    public Delta(Action<T> action)
    {
      _action = action;
    }

    public void Apply(T obj) => _action(obj);

    public Delta<T> Merge(params Delta<T>[] deltas)
    {
      Action<T> newAction = _action;

      foreach(Delta<T> delta in deltas )
      {
        newAction += delta._action;
      }

      return new Delta<T>(newAction);
    }
  }
}
