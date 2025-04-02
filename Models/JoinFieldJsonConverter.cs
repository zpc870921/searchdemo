//using Elastic.Clients.Elasticsearch;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace seachdemo.Models
//{
//    public class JoinFieldJsonConverter : JsonConverter<JoinField>
//    {
//        public override JoinField Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//        {
//            // 处理不同类型的输入
//            if (reader.TokenType == JsonTokenType.String)
//            {
//                string value = reader.GetString();
//                return new JoinField(value); // 只有类型名的简单字符串
//            }
//            else if (reader.TokenType == JsonTokenType.StartObject)
//            {
//                // 解析包含父ID的对象
//                string name = null;
//                string parent = null;

//                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
//                {
//                    if (reader.TokenType == JsonTokenType.PropertyName)
//                    {
//                        string propertyName = reader.GetString();
//                        reader.Read();

//                        if (propertyName.Equals("name", StringComparison.OrdinalIgnoreCase))
//                        {
//                            name = reader.GetString();
//                        }
//                        else if (propertyName.Equals("parent", StringComparison.OrdinalIgnoreCase))
//                        {
//                            parent = reader.GetString();
//                        }
//                    }
//                }

//                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(parent))
//                {
//                    return JoinField.Child(name, parent);
//                }
//                else if (!string.IsNullOrEmpty(name))
//                {
//                    return new JoinField(name);
//                }
//            }

//            // 无法解析，返回空值
//            return null;
//        }

//        public override void Write(Utf8JsonWriter writer, JoinField value, JsonSerializerOptions options)
//        {
//            if (value == null)
//            {
//                writer.WriteNullValue();
//                return;
//            }

//            // 获取JoinField的内部信息
//            string type = value.Name;
//            string parent = value.Parent;

//            if (string.IsNullOrEmpty(parent))
//            {
//                // 简单类型，直接写入字符串
//                writer.WriteStringValue(type);
//            }
//            else
//            {
//                // 包含父ID的复杂类型，写入对象
//                writer.WriteStartObject();
//                writer.WriteString("name", type);
//                writer.WriteString("parent", parent);
//                writer.WriteEndObject();
//            }
//        }
//    }
//} 