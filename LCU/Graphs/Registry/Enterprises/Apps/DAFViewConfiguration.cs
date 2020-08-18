using Fathym;
using Fathym.Business.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.Graphs.Registry.Enterprises.Apps
{
    [Serializable]
    [DataContract]
    public class DAFViewConfiguration : DAFApplicationConfiguration
    {
        [DataMember]
        public virtual string BaseHref { get; set; }

        [DataMember]
        public virtual string NPMPackage { get; set; }

        [DataMember]
        public virtual string PackageVersion { get; set; }

        [DataMember]
        //[JsonConverter(typeof(StringObjectConverter))]
        public virtual MetadataModel StateConfig { get; set; }
    }

    public class StringObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader is JTokenReader jtReader)
            {
                return JsonConvert.DeserializeObject(jtReader.CurrentToken[0]["value"].ToString(), objectType);
            }
            else
                return serializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
