using System.IO;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.PricingModule.Data.StreamJsonFetcher;
using Xunit;

namespace VirtoCommerce.PricingModule.Tests
{

    public class StreamFetcherTest
    {

        [Fact]
        public void TestCollection()
        {

            var data = Serialize(
                new
                {
                    a = new[]
                    {
                        new N { name = "name1" },
                        new N { name = "name2" }
                    },
                    b = new[]
                    {
                        new N { name = "name3" },
                        new N { name = "name4" }
                    }
                });

            using (var fetcher = GetFactory().Create(GetStreamFromString(data)))
            {
                var a = fetcher.FetchArray<N>("a").ToList();

                var b = fetcher.FetchArray<N>("b").ToList();

                Assert.Equal(2, a.Count);
                Assert.Equal(2, b.Count);
                Assert.Equal("name1", a[0].name);
                Assert.Equal("name4", b[1].name);
            }
        }

        [Fact]
        public void TestSingleEntry()
        {
            var data = Serialize(
                new
                {
                    single = "123"
                });

            using (var fetcher = GetFactory().Create(GetStreamFromString(data)))
            {
                var r = fetcher.FetchSingle<string>("single");

                Assert.Equal("123", r);
            }
        }

        [Fact]
        public void CombinedTest()
        {
            var data = Serialize(
                new
                {
                    single1 = "123",
                    multiple1 = new[]
                    {
                        new N { name = "name1" },
                        new N { name = "name2" }
                    },
                    multiple2 = new[]
                    {
                        new N { name = "name3" },
                        new N { name = "name4" }
                    },
                    single2 = "321"

                });

            using (var fetcher = GetFactory().Create(GetStreamFromString(data)))
            {
                var r1 = fetcher.FetchSingle<string>("single1");

                var multiple1 = fetcher.FetchArray<N>("multiple1").ToList();
                var multiple2 = fetcher.FetchArray<N>("multiple2").ToList();

                var r2 = fetcher.FetchSingle<int>("single2");

                Assert.Equal("123", r1);

                Assert.Equal(2, multiple1.Count);
                Assert.Equal("name1", multiple1[0].name);

                Assert.Equal(2, multiple2.Count);
                Assert.Equal("name4", multiple2[1].name);

                Assert.Equal(321, r2);
            }
        }

        [Fact]
        public void CustomSingleTypeTest()
        {
            var data = Serialize(
                new
                {
                    a = new N
                    {
                        name = "name1"
                    }

                });

            using (var fetcher = GetFactory().Create(GetStreamFromString(data)))
            {
                var a = fetcher.FetchSingle<N>("a");

                Assert.Equal("name1", a.name);
            }
        }

        [Fact]
        public void MultiTypesTest()
        {
            var data = Serialize(
                new
                {
                    a = 1,
                    b = new N { name = "name1" },
                    c = new[]
                    {
                        new N { name = "name2" },
                        new N { name = "name3" }
                    },
                    d = "test",
                    e = new[] { 123, 321 },
                    f = new[] { "test2", "test3" }
                });

            using (var fetcher = GetFactory().Create(GetStreamFromString(data)))
            {
                var a = fetcher.FetchSingle<int>("a");

                var b = fetcher.FetchSingle<N>("b");

                var c = fetcher.FetchArray<N>("c").ToList();

                var d = fetcher.FetchSingle<string>("d");

                var e = fetcher.FetchArray<int>("e").ToList();

                var f = fetcher.FetchArray<string>("f").ToList();

                Assert.Equal(1, a);

                Assert.Equal("name1", b.name);

                Assert.Equal(2, c.Count);
                Assert.Equal("name3", c[1].name);

                Assert.Equal("test", d);

                Assert.Equal(123, e[0]);
                Assert.Equal(2, e.Count);

                Assert.Equal(2, f.Count);
                Assert.Equal("test3", f[1]);
            }
        }

        [Fact]
        public void SkipOnePrepertyTest()
        {
            var data = Serialize(
                new
                {
                    a = 1,
                    b = new N { name = "name1" },
                    c = new[]
                    {
                        new N { name = "name2" },
                        new N { name = "name3" }
                    }});

            using (var fetcher = GetFactory().Create(GetStreamFromString(data)))
            {
                var c = fetcher.FetchArray<N>("c").ToList();

                Assert.Equal(2, c.Count);
                Assert.Equal("name3", c[1].name);
            }
        }

        private Stream GetStreamFromString(string value)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(value);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        private static StreamFetcherFactory GetFactory()
        {
            return new StreamFetcherFactory(GetJsonSerializerFactory());
        }

        private static JsonSerializerFactory GetJsonSerializerFactory()
        {
            return new JsonSerializerFactory();
        }
    }

    internal class N
    {
        public string name;
    }
}
