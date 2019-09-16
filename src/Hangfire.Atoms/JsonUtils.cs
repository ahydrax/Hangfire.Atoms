using Hangfire.Common;

namespace Hangfire.Atoms
{
    public static class JsonUtils
    {
        public static string Serialize<T>(T @object)
            => SerializationHelper.Serialize(@object, SerializationOption.TypedInternal);

        public static T Deserialize<T>(string json)
            => SerializationHelper.Deserialize<T>(json, SerializationOption.TypedInternal);
    }
}
