using System;
using Newtonsoft.Json;

namespace Koofr.Sdk.Api.V2.Resources.JsonTypeConverters
{
    public class PermissionTypeEnumConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var permissions = (Permissions)value;

            if (permissions.Count > 0)
            {
                writer.WriteStartObject();
                foreach (var permission in permissions)
                {
                    switch (permission.Key)
                    {
                        case PermissionType.Comment:
                            writer.WritePropertyName("COMMENT");
                            writer.WriteValue(permission.Value);
                            break;
                        case PermissionType.CreateLink:
                            writer.WritePropertyName("CREATE_LINK");
                            writer.WriteValue(permission.Value);
                            break;
                        case PermissionType.CreateReceiver:
                            writer.WritePropertyName("CREATE_RECEIVER");
                            writer.WriteValue(permission.Value);
                            break;
                        case PermissionType.Mount:
                            writer.WritePropertyName("MOUNT");
                            writer.WriteValue(permission.Value);
                            break;
                        case PermissionType.Owner:
                            writer.WritePropertyName("OWNER");
                            writer.WriteValue(permission.Value);
                            break;
                        case PermissionType.Read:
                            writer.WritePropertyName("READ");
                            writer.WriteValue(permission.Value);
                            break;
                        case PermissionType.Write:
                            writer.WritePropertyName("WRITE");
                            writer.WriteValue(permission.Value);
                            break;
                    }
                }
                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                // THIS IS VERY, VERY FAC**** UGLY, REFACTOR ASAP
                var returnPermissions = new Permissions();
                dynamic jsonResponse = serializer.Deserialize(reader);

                var jsonComment = jsonResponse.COMMENT;
                if (jsonComment != null)
                {
                    var commentValue = jsonComment.Value as bool?;
                    if (commentValue.HasValue)
                        returnPermissions.Add(PermissionType.Comment, commentValue.Value);
                }

                var jsonCreateLink = jsonResponse.CREATE_LINK;
                if (jsonCreateLink != null)
                {
                    var creteLinkValue = jsonCreateLink.Value as bool?;
                    if (creteLinkValue.HasValue)
                        returnPermissions.Add(PermissionType.CreateLink, creteLinkValue.Value);
                }

                var jsonCreateReceiver = jsonResponse.CREATE_RECEIVER;
                if (jsonCreateReceiver != null)
                {
                    var createReceiverValue = jsonCreateReceiver.Value as bool?;
                    if (createReceiverValue.HasValue)
                        returnPermissions.Add(PermissionType.CreateReceiver, createReceiverValue.Value);
                }

                var jsonMount = jsonResponse.MOUNT;
                if (jsonMount != null)
                {
                    var mountValue = jsonMount.Value as bool?;
                    if (mountValue.HasValue)
                        returnPermissions.Add(PermissionType.Mount, mountValue.Value);
                }

                var jsonOwner = jsonResponse.OWNER;
                if (jsonOwner != null)
                {
                    var ownerValue = jsonOwner.Value as bool?;
                    if (ownerValue.HasValue)
                        returnPermissions.Add(PermissionType.Owner, ownerValue.Value);
                }

                var jsonRead = jsonResponse.READ;
                if (jsonRead != null)
                {
                    var readValue = jsonOwner.Value as bool?;
                    if (readValue.HasValue)
                        returnPermissions.Add(PermissionType.Read, readValue.Value);
                }

                var jsonWrite = jsonResponse.WRITE;
                if (jsonWrite != null)
                {
                    var writeValue = jsonWrite.Value as bool?;
                    if (writeValue.HasValue)
                        returnPermissions.Add(PermissionType.Write, writeValue.Value);
                }

                return returnPermissions;
            }
            return new Permissions();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Permissions);
        }
    }
}