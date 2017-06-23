using System;
using System.Collections.Generic;
using System.Linq;

namespace Evade
{
    internal class PriorityQueue<TP, TV>
    {
        private readonly SortedDictionary<TP, Queue<TV>> list = new SortedDictionary<TP, Queue<TV>>();

        public void Enqueue(TP priority, TV value)
        {
            Queue<TV> q;
            if (!list.TryGetValue(priority, out q))
            {
                q = new Queue<TV>();
                list.Add(priority, q);
            }
            q.Enqueue(value);
        }

        public TV Dequeue()
        {
            // will throw if there isn’t any first element!
            var pair = list.First();
            var v = pair.Value.Dequeue();
            if (pair.Value.Count == 0) // nothing left of the top priority.
                list.Remove(pair.Key);
            return v;
        }

        public bool IsEmpty => !list.Any();
    }
}
