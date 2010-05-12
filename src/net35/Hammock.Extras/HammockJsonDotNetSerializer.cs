using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hammock.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Hammock.Extras
{
    /// <summary>
    /// Resolves all property names to JSON conventional standard,
    /// i.e. JSON name "this_is_a_property" will map to the class property ThisIsAProperty.
    /// </summary>
    public class JsonConventionResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(JsonObjectContract contract)
        {
            var properties = base.CreateProperties(contract);

            foreach (var property in properties)
            {
                property.PropertyName = PascalCaseToElement(property.PropertyName);
            }

            return properties;
        }

        private static string PascalCaseToElement(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var result = new StringBuilder();

            result.Append(char.ToLowerInvariant(input[0]));
            for (var i = 1; i < input.Length; i++)
            {
                if (char.IsLower(input[i]))
                {
                    result.Append(input[i]);
                }
                else
                {
                    result.Append("_");
                    result.Append(char.ToLowerInvariant(input[i]));
                }
            }

            return result.ToString();
        }
    }

    public class JsonDotNetSerializer : Utf8Serializer, ISerializer, IDeserializer
    {
        private readonly JsonSerializer _serializer;

        public JsonDotNetSerializer()
            : this(new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ContractResolver = new JsonConventionResolver()
                })
        {

        }

        public JsonDotNetSerializer(JsonSerializerSettings settings)
        {
            _serializer = new JsonSerializer
                              {
                                  ConstructorHandling = settings.ConstructorHandling,
                                  ContractResolver = settings.ContractResolver,
                                  ObjectCreationHandling = settings.ObjectCreationHandling,
                                  MissingMemberHandling = settings.MissingMemberHandling,
                                  DefaultValueHandling = settings.DefaultValueHandling,
                                  NullValueHandling = settings.NullValueHandling
                              };

            foreach (var converter in settings.Converters)
            {
                _serializer.Converters.Add(converter);
            }
        }

        #region IDeserializer Members

        public virtual object Deserialize(string content, Type type)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize(jsonTextReader, type);
                }
            }
        }

        public virtual T Deserialize<T>(string content)
        {
            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        #endregion

        #region ISerializer Members

        public virtual string Serialize(object instance, Type type)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    jsonTextWriter.Formatting = Formatting.Indented;
                    jsonTextWriter.QuoteChar = '"';

                    _serializer.Serialize(jsonTextWriter, instance);

                    var result = stringWriter.ToString();
                    return result;
                }
            }
        }

        public virtual string ContentType
        {
            get { return "application/json"; }
        }

        #endregion
    }
}