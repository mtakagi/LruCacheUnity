using NUnit.Framework;

namespace Editor
{
    public class LruCacheTest
    {
        private class CustomCache : LruCache<string, string>
        {
            public CustomCache(int size) : base(size) {}
            
            protected override int SizeOf(string key, string value)
            {
                return value.Length;
            }
        }
        
        private LruCache<string, string> _cache;
        private LruCache<string, string> _customCache;
    
        [SetUp]
        public void Init()
        {
            _cache = new LruCache<string, string>(3);
            _cache.Put("hoge", "fuga");
            _cache.Put("foo", "bar");
            _cache.Put("buzz", "piyo");
            _customCache = new CustomCache(1000);
            _customCache.Put("Sample1",
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            _customCache.Put("Sample2",
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
        }

        [Test]
        public void EvictTest()
        {
            var tmp = _cache.Get("foo");
            Assert.NotNull(tmp);
            _cache.Put("nyaga", "mogya");
            tmp = _cache.Get("hoge");
            Assert.Null(tmp);
            tmp = _cache.Get("nyaga");
            Assert.NotNull(tmp);
        }

        [Test]
        public void CustomCacheTest()
        {
            var sample = _customCache.Get("Sample1");
            Assert.NotNull(sample);
            _customCache.Put("Sample3",
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            sample = _customCache.Get("Sample2");
            Assert.Null(sample);
            sample = _customCache.Get("Sample3");
            Assert.NotNull(sample);
        }
    }
}
