using System.Collections.Generic;

// https://android.googlesource.com/platform/frameworks/support.git/+/795b97d901e1793dac5c3e67d43c96a758fec388/v4/java/android/support/v4/util/LruCache.java
// https://developer.android.com/reference/android/util/LruCache.html
public class LruCache<TKey, TValue> {
	private int size;
	private int maxSize;
	private int putCount;
	private int createCount;
	private int evictionCount;
	private int hitCount;
	private int missCount;

	private OrderedDictionary<TKey, TValue> map;

	public int Size { get { return size; } }
	public int MaxSize { get { return maxSize; } }
	public int PutCount { get { return putCount; } }
	public int CreateCount { get { return createCount; } }
	public int EvictionCount { get { return evictionCount; } }
	public int HitCount { get { return hitCount; } }
	public int MissCount { get { return missCount; } }
	public IDictionary<TKey, TValue> Snapshot { get { return new Dictionary<TKey, TValue> (this.map); } }

	public LruCache(int maxSize) {
		this.maxSize = maxSize;
		this.map = new OrderedDictionary<TKey, TValue>(true);
	}

	public TValue Get(TKey key) {
		TValue value;

		if (this.map.TryGetValue (key, out value)) {
			hitCount++;
			return value;
		}
			
		missCount++;
		value = Create (key);

		if (value != null) {
			createCount++;
			this.size += SafeSizeOf (key, value);
			this.map [key] = value;
			TrimToSize (this.maxSize);
		}

		return value;
	}

	public TValue Put(TKey key, TValue value)
	{
		putCount++;
		size += SafeSizeOf (key, value);

		TValue previous;
		if (this.map.TryGetValue (key, out previous)) {
			size -= SafeSizeOf (key, previous);
		}
		this.map [key] = value;
		TrimToSize (maxSize);

		return previous;
	}

	public TValue Remove(TKey key)
	{
		var previous = this.map [key];

		if (previous != null) {
			this.map.Remove (key);
			size -= SafeSizeOf (key, previous);
		}

		return previous;	
	}

	public void EvictAll()
	{
		TrimToSize (-1);
	}

	public override string ToString ()
	{
		var accesses = hitCount + missCount;
		var hitPercent = accesses != 0 ? (100 * hitCount / accesses) : 0;

		return string.Format ("LruCache[maxSize={0},hits={1},misses={2},hitRate={3}]", maxSize, hitCount, missCount, hitPercent);
	}

	private void TrimToSize(int maxSize)
	{
		var enumerator = this.map.GetEnumerator ();

		while (size > maxSize && this.map.Count > 0) {
			if (!enumerator.MoveNext ()) {
				break;
			}

			var key = enumerator.Current.Key;
			var value = enumerator.Current.Value;
			this.map.Remove (key);
			size -= SafeSizeOf (key, value);
			evictionCount++;

			EntryEvicted (key, value);
		}
	}

	private int SafeSizeOf(TKey key, TValue value)
	{
		var size = SizeOf (key, value);

		if (size < 0) {
			throw new System.InvalidProgramException ();
		}

		return size;
	}

	protected virtual TValue Create(TKey key) {
		return default(TValue);
	}

	protected virtual void EntryEvicted(TKey key, TValue value) {}

	protected virtual int SizeOf(TKey key, TValue value) {
		return 1;
	}
}
