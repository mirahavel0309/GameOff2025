using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<(int priority, T item)> heap = new();

    public int Count => heap.Count;

    public void Enqueue(T item, int priority)
    {
        heap.Add((priority, item));
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        var (priority, item) = heap[0];
        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);
        HeapifyDown(0);
        return item;
    }

    public T Peek()
    {
        return heap.Count > 0 ? heap[0].item : default;
    }

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[i].priority >= heap[parent].priority)
                break;

            (heap[i], heap[parent]) = (heap[parent], heap[i]);
            i = parent;
        }
    }

    private void HeapifyDown(int i)
    {
        int last = heap.Count - 1;
        while (true)
        {
            int left = i * 2 + 1;
            int right = i * 2 + 2;
            int smallest = i;

            if (left <= last && heap[left].priority < heap[smallest].priority)
                smallest = left;
            if (right <= last && heap[right].priority < heap[smallest].priority)
                smallest = right;

            if (smallest == i) return;

            (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
            i = smallest;
        }
    }
}
