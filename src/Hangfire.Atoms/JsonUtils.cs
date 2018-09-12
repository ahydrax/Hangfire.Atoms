using Newtonsoft.Json;

namespace Hangfire.Atoms
{
    public static class JsonUtils
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.None
        };

        public static string Serialize<T>(T @object)
            => JsonConvert.SerializeObject(@object, SerializerSettings);

        public static T Deserialize<T>(string @object)
            => JsonConvert.DeserializeObject<T>(@object, SerializerSettings);
    }
}
