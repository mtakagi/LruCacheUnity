using NUnit.Framework;

namespace Editor
{
    public class OrderedDictionaryTest
    {
        private OrderedDictionary<string, string> _accessOrderDict;
        private OrderedDictionary<string, string> _normalOrderDict;
        private readonly OrderedDictionary<string, string> _emptyDict = new OrderedDictionary<string, string>();

        [SetUp]
        public void Init()
        {
            _accessOrderDict = new OrderedDictionary<string, string>(true);
            _accessOrderDict["foo"] = "buzz";
            _accessOrderDict["bar"] = "hoge";
            _accessOrderDict["fuga"] = "fizz";
            // ReSharper disable once NotAccessedVariable
            var tmp = _accessOrderDict["bar"];
            _normalOrderDict = new OrderedDictionary<string, string>();
            _normalOrderDict["foo"] = "buzz";
            _normalOrderDict["bar"] = "hoge";
            _normalOrderDict["fuga"] = "fizz";
            // ReSharper disable once RedundantAssignment
            tmp = _normalOrderDict["bar"];
        }

        [Test]
        public void AccessOrderTest()
        {
            using (var enumerator = _accessOrderDict.GetEnumerator())
            {
                var i = 0;

                while (enumerator.MoveNext())
                {
                    var value = enumerator.Current.Value;
                    switch (i++)
                    {
                        case 0:
                            Assert.AreEqual("buzz", value);
                            break;
                        case 1:
                            Assert.AreEqual("fizz", value);
                            break;
                        case 2:
                            Assert.AreEqual("hoge", value);
                            break;
                        default:
                            Assert.Fail();
                            break;
                    }
                }
            }
        }

        [Test]
        public void NormalOrderTest()
        {
            using (var enumerator = _normalOrderDict.GetEnumerator())
            {
                var i = 0;

                while (enumerator.MoveNext())
                {
                    var value = enumerator.Current.Value;
                    switch (i++)
                    {
                        case 0:
                            Assert.AreEqual("buzz", value);
                            break;
                        case 1:
                            Assert.AreEqual("hoge", value);
                            break;
                        case 2:
                            Assert.AreEqual("fizz", value);
                            break;
                        default:
                            Assert.Fail();
                            break;
                    }
                }
            }
        }

        [Test]
        public void ExceptionTest()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var tmp = _emptyDict[null];
            });
            Assert.Throws<System.ArgumentNullException>(() => { _emptyDict[null] = "hoge"; });
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
            {
                // ReSharper disable once UnusedVariable
                var tmp = _emptyDict["null"];
            });
        }
    }
}