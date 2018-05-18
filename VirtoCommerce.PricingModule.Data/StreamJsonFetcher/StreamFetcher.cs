using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("VirtoCommerce.PricingModule.Test")]
namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher
{
    internal class StreamFetcher : IDisposable
    {
        private readonly JsonSerializer _serializer;
        private readonly Stream _stream;

        private JsonTextReader _jsonTextReader;
        private StreamReader _streamReader;
        

        public StreamFetcher(JsonSerializerFactory jsonSerializerFactory, Stream stream)
        {
            _serializer = jsonSerializerFactory.Create();
            _stream = stream;
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

        public IEnumerable<T> FetchArray<T>(string property)
        {
            bool continuation = false;

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

                    if (Reader.TokenType != JsonToken.EndArray && Reader.TokenType != JsonToken.PropertyName)
                    {
                        continuation = true;
                        var obj = _serializer.Deserialize<T>(Reader);

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

                    return _serializer.Deserialize<T>(Reader);
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
