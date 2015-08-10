using System;
using Newtonsoft.Json;

namespace Koofr.Sdk.Api.V2.Resources.JsonTypeConverters
{
    public class MountTypeEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var fileType = (MountType)value;
            switch (fileType)
            {
                case MountType.Device:
                    writer.WriteValue("device");
                    break;

                case MountType.Export:
                    writer.WriteValue("export");
                    break;
                case MountType.Import:
                    writer.WriteValue("import");
                    break;
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string)reader.Value;
            MountType? fileType = null;

            switch (enumString)
            {
                case "device":
                    fileType = MountType.Device;
                    break;
                case "export":
                    fileType = MountType.Export;
                    break;
                case "import":
                    fileType = MountType.Import;
                    break;
            }
            return fileType;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}