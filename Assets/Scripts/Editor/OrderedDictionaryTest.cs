using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class OrderedDictionaryTest {

	private OrderedDictionary<string, string> accessOrderDict;
	private OrderedDictionary<string, string> normalOrderDict;
	private OrderedDictionary<string, string> emptyDict = new OrderedDictionary<string, string> ();

	[SetUp]
	public void Init()
	{
		accessOrderDict = new OrderedDictionary<string, string> (true);
		accessOrderDict ["foo"] = "buzz";
		accessOrderDict ["bar"] = "hoge";
		accessOrderDict ["fuga"] = "fizz";
		var tmp = accessOrderDict ["bar"];
		normalOrderDict = new OrderedDictionary<string, string> ();
		normalOrderDict ["foo"] = "buzz";
		normalOrderDict ["bar"] = "hoge";
		normalOrderDict ["fuga"] = "fizz";
		tmp = normalOrderDict ["bar"];
	}

	[Test]
	public void AccessOrderTest()
	{
		var enumerator = accessOrderDict.GetEnumerator ();
		var i = 0;
	    
		while (enumerator.MoveNext ()) {
			var value = enumerator.Current.Value;
			switch (i++) {
			case 0:
				Assert.AreEqual ("buzz", value);
				break;
			case 1:
				Assert.AreEqual ("fizz", value);
				break;
			case 2:
				Assert.AreEqual ("hoge", value);
				break;
			}
		}
	}

	[Test]
	public void NormalOrderTest()
	{
		var enumerator = normalOrderDict.GetEnumerator ();
		var i = 0;

		while (enumerator.MoveNext ()) {
			var value = enumerator.Current.Value;
			switch (i++) {
			case 0:
				Assert.AreEqual ("buzz", value);
				break;
			case 1:
				Assert.AreEqual ("hoge", value);
				break;
			case 2:
				Assert.AreEqual ("fizz", value);
				break;
			}
		}
	}

	[Test]
	public void ExceptionTest()
	{
		Assert.Throws<System.ArgumentNullException> (() => { var tmp = emptyDict [null]; });
		Assert.Throws<System.ArgumentNullException> (() => { emptyDict [null] = "hoge"; });
		Assert.Throws<System.Collections.Generic.KeyNotFoundException> (() => { var tmp = emptyDict ["null"]; });
	}

}
