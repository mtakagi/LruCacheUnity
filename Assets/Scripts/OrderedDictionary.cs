using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Lru cache.
/// </summary>
/// http://d.hatena.ne.jp/Kazuhira/20151226/1451134718
/// http://qiita.com/matarillo/items/c09e0f3e5a61f84a51e2
/// https://github.com/Unity-Technologies/mono/blob/unity-staging/mcs/class/corlib/System.Collections.Generic/Dictionary.cs
/// https://android.googlesource.com/platform/libcore/+/master/ojluni/src/main/java/java/util/HashMap.java
/// https://android.googlesource.com/platform/libcore/+/master/ojluni/src/main/java/java/util/LinkedHashMap.java
/// https://android.googlesource.com/platform/frameworks/support.git/+/795b97d901e1793dac5c3e67d43c96a758fec388/v4/java/android/support/v4/util/LruCache.java
public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue> {

	#region Internal Class

	class LinkedKeyValuePair {
		internal KeyValuePair<TKey, TValue> kvp;
		internal LinkedKeyValuePair before, after;

		public TKey Key { get { return kvp.Key; } }
		public TValue Value { get { return kvp.Value; } }

		public LinkedKeyValuePair(TKey key, TValue value) {
			this.kvp = new KeyValuePair<TKey, TValue>(key, value);
		}

		public LinkedKeyValuePair(KeyValuePair<TKey, TValue> kvp) {
			this.kvp = kvp;
		}

		public static implicit operator KeyValuePair<TKey, TValue>(LinkedKeyValuePair lkvp) {
			return lkvp.kvp;
		}

		public static implicit operator LinkedKeyValuePair(KeyValuePair<TKey, TValue> kvp) {
			return new LinkedKeyValuePair(kvp);
		}
	}

	struct LKVPEnumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
		private readonly LinkedKeyValuePair start;
		private LinkedKeyValuePair current;

		public LKVPEnumerator(LinkedKeyValuePair start) {
			this.start = start;
			this.current = null;
		}

		public bool MoveNext() {
			if (this.current == null) {
				current = start;
			} else {
				current = current.after;
			}

			return current != null;
		}

		public void Reset() { current = start; }

		void System.IDisposable.Dispose() { }

		public KeyValuePair<TKey, TValue> Current
		{
			get { return current; }
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}
	}

	#endregion

	#region Field

	private LinkedKeyValuePair head;
	private LinkedKeyValuePair tail;
	private Dictionary<TKey, LinkedKeyValuePair> dict;

	#endregion

	#region Property

	public int Count { get { return dict.Count; } }
	public bool IsReadOnly { get { return false; } }
	public TValue this [TKey key] {
		get {
			if (key == null) {
				throw new System.ArgumentNullException ("key");
			}

			TValue value;
			if (TryGetValue (key, out value)) {
				return value;
			}

			throw new KeyNotFoundException ();
		}
		set {
			if (key == null) {
				throw new System.ArgumentNullException ("key");
			}

			if (dict.ContainsKey (key)) {
				Remove (key);
			}

			if (value != null) {
				Add (key, value);
			}
		}
	}
	public ICollection<TKey> Keys { get { return dict.Keys; } }
	public ICollection<TValue> Values { get { return dict.Values.Select(kvp => kvp.Value).ToArray(); } }
	public bool AccessOrder { get; private set; }
	public KeyValuePair<TKey, TValue> Eldest { get { return head; } }

	#endregion

	public OrderedDictionary(bool accessOrder = false)
	{
		AccessOrder = accessOrder;
		dict = new Dictionary<TKey, LinkedKeyValuePair> ();
	}

	#region Interface

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> value) {
		var last = NewKVP(value);
		dict [value.Key] = last;
	}

	public void Add(TKey key, TValue value) {
		var last = NewKVP (key, value);
		dict [key] = last;
	}

	public void Clear() {
		dict.Clear ();
		head = tail = null;
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> value){
		return ContainsKey(value.Key);
	}

	public bool ContainsKey(TKey key) {
		for (var e = head; e != null; e = e.after) {
			if (e.Key.Equals(key)) {
				return true;
			}
		}

		return false;
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
		LinkedKeyValuePair[] tmp = new LinkedKeyValuePair[array.Length];
		dict.Values.CopyTo (tmp, arrayIndex);
		array = System.Array.ConvertAll (tmp, element => element.kvp);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
		return new LKVPEnumerator(head);
	}

	IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> value) {
		RemoveKVP (dict [value.Key]);
		return dict.Remove(value.Key);
	}

	public bool Remove(TKey key) {
		RemoveKVP (dict [key]);
		return dict.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue value) {
		LinkedKeyValuePair kvp;
		if (dict.TryGetValue (key, out kvp)) {
			AfterNodeAccess (kvp);
			value = kvp.Value;
			return true;
		}

		value = default(TValue);
		return false;
	}

	#endregion

	#region Linked List

	private LinkedKeyValuePair NewKVP(TKey key, TValue value)
	{
		var lkvp = new LinkedKeyValuePair (key, value);
		InsertLast (lkvp);

		return lkvp;
	}

	private LinkedKeyValuePair NewKVP(KeyValuePair<TKey, TValue> kvp)
	{
		var lkvp = new LinkedKeyValuePair (kvp);
		InsertLast (lkvp);

		return lkvp;
	}

	private void AfterNodeAccess(LinkedKeyValuePair kvp)
	{
		var last = tail;
		if (!AccessOrder || last == kvp) {
			return;
		}

		LinkedKeyValuePair p = kvp, b = p.before, a = p.after;
		p.after = null;

		if (b == null) {
			head = a;
		} else {
			b.after = a;
		}

		if (a != null) {
			a.before = b;
		} else {
			last = b;
		}

		if (last == null) {
			head = p;
		} else {
			p.before = last;
			last.after = p;
		}

		tail = p;
	}

	private void InsertLast(LinkedKeyValuePair kvp)
	{
		var last = tail;
		tail = kvp;

		if (last == null) {
			head = kvp;
		} else {
			kvp.before = last;
			last.after = tail;
		}
	}

	private void RemoveKVP(LinkedKeyValuePair kvp)
	{
		LinkedKeyValuePair p = kvp, b = p.before, a = p.after;

		if (b == null) {
			head = a;
		} else {
			b.after = a;
		}

		if (a == null) {
			tail = b;
		} else {
			a.before = b;
		}

		p = kvp = null;
	}

	#endregion
}
