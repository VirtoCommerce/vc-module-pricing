using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher;

[assembly:InternalsVisibleTo("VirtoCommerce.PricingModule.Test")]
namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher
{
    internal class StreamFetcher : IDisposable
    {
        private readonly JsonSerializerFactory _serializerFactory;
        private readonly Stream _stream;
        private readonly IFetcherResolver _resolver;

        private JsonTextReader _jsonTextReader;
        private StreamReader _streamReader;
        

        public StreamFetcher(JsonSerializerFactory jsonSerializerFactory, Stream stream, IFetcherResolver resolver)
        {
            _serializerFactory = jsonSerializerFactory;
            _stream = stream;
            _resolver = resolver;
        }

        private JsonTextReader Reader
        {
            get
            {
                if (_streamReader == null)
                {
                    _streamReader = new StreamReader(_stream);
                }

                return _jsonTextReader ?? (_jsonTextReader = new JsonTextReader(_streamReader));
            }
        }

        public IEnumerable<T> FetchArray<T>(string property) where T : class
        {
            bool continuation = false;

            var serializer = _serializerFactory.Create();

            while (Reader.Read())
            {
                if (Reader.TokenType == JsonToken.PropertyName || continuation)
                {
                    if (!continuation && Reader.Value.ToString() == property)
                    {
                        Reader.Read();
                    }

                    if (Reader.TokenType == JsonToken.StartArray)
                    {
                        Reader.Read();
                    }

                    if (Reader.TokenType == JsonToken.StartObject)
                    {
                        continuation = true;
                        var obj = serializer.Deserialize<T>(Reader);

                        yield return obj;
                    }

                    if (Reader.TokenType == JsonToken.EndArray)
                    {
                        yield break;
                    }
                }
            }
        }

        public T FetchSingle<T>(string property)
        {
            while (Reader.Read())
            {
                if (Reader.TokenType == JsonToken.PropertyName)
                {
                    if (Reader.Value.ToString() == property)
                    {
                        Reader.Read();
                    }

                    return _resolver.Resolve<T>().Fetch(Reader);
                }
            }

            throw new InvalidOperationException();
        }

        public void Dispose()
        {
            _streamReader?.Dispose();
            _jsonTextReader?.Close();
        }
    }
}
