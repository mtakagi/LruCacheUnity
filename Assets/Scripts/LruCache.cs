using System.Collections.Generic;

// https://android.googlesource.com/platform/frameworks/support.git/+/795b97d901e1793dac5c3e67d43c96a758fec388/v4/java/android/support/v4/util/LruCache.java
// https://developer.android.com/reference/android/util/LruCache.html
public class LruCache<TKey, TValue> {
	private readonly OrderedDictionary<TKey, TValue> _map;

	public int Size { get; private set; }
	public int MaxSize { get; private set; }
	public int PutCount { get; private set; }
	public int CreateCount { get; private set; }
	public int EvictionCount { get; private set; }
	public int HitCount { get; private set; }
	public int MissCount { get; private set; }
	public IDictionary<TKey, TValue> Snapshot { get { return new Dictionary<TKey, TValue> (_map); } }

	public LruCache(int maxSize) {
		MaxSize = maxSize;
		_map = new OrderedDictionary<TKey, TValue>(true);
	}

	public TValue Get(TKey key) {
		TValue value;

		if (_map.TryGetValue (key, out value)) {
			HitCount++;
			return value;
		}
			
		MissCount++;
		value = Create (key);

		if (value == null)
		{
			return default(TValue);
		}
		
		CreateCount++;
		Size += SafeSizeOf (key, value);
		_map [key] = value;
		TrimToSize (MaxSize);

		return value;
	}

	public TValue Put(TKey key, TValue value)
	{
		PutCount++;
		Size += SafeSizeOf (key, value);

		TValue previous;
		if (_map.TryGetValue (key, out previous)) {
			Size -= SafeSizeOf (key, previous);
		}
		_map [key] = value;
		TrimToSize (MaxSize);

		return previous;
	}

	public TValue Remove(TKey key)
	{
		var previous = _map [key];

		if (previous == null)
		{
			return default(TValue);
		}
		
		_map.Remove (key);
		Size -= SafeSizeOf (key, previous);

		return previous;	
	}

	public void EvictAll()
	{
		TrimToSize (-1);
	}

	public override string ToString ()
	{
		var accesses = HitCount + MissCount;
		var hitPercent = accesses != 0 ? (100 * HitCount / accesses) : 0;

		return string.Format ("LruCache[maxSize={0},hits={1},misses={2},hitRate={3}]", MaxSize, HitCount, MissCount, hitPercent);
	}

	private void TrimToSize(int maxSize)
	{
		using (var enumerator = _map.GetEnumerator())
		{
			while (Size > maxSize && _map.Count > 0) {
				if (!enumerator.MoveNext ()) {
					break;
				}

				var key = enumerator.Current.Key;
				var value = enumerator.Current.Value;
				_map.Remove (key);
				Size -= SafeSizeOf (key, value);
				EvictionCount++;

				EntryEvicted (key, value);
			}			
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
