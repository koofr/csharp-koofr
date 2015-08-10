using Newtonsoft.Json;

namespace Koofr.Sdk.Api.V2.Resources.JsonTypeConverters
{
    public class FileTypeEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var fileType = (FileType) value;
            switch (fileType)
            {
                case FileType.Dir:
                    writer.WriteValue("dir");
                    break;

                case FileType.File:
                    writer.WriteValue("file");
                    break;
                case FileType.Parent:
                    writer.WriteValue("parent");
                    break;
            }
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumString = (string) reader.Value;
            FileType? fileType = null;

            switch (enumString)
            {
                case "dir":
                    fileType = FileType.Dir;
                    break;
                case "file":
                    fileType = FileType.File;
                    break;
                case "parent":
                    fileType = FileType.Parent;
                    break;
            }
            return fileType;
        }

        public override bool CanConvert(System.Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}