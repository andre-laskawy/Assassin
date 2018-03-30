///-----------------------------------------------------------------
///   File:     SerializerExtension.cs
///   Author:   Andre Laskawy           
///   Date:	    25.11.2017 14:17:16
///   Revision History: 
///   Name:  Andre Laskawy         Date:  25.11.2017 14:17:16      Description: Created
///-----------------------------------------------------------------

namespace Assassin.Common
{
    using MsgPack.Serialization;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Linq.Expressions;

    /// <summary>
    /// Defines the <see cref="SerializerExtension" />
    /// </summary>
    public static class SerializerExtension
    {
        /// <summary>
        /// The default settings
        /// </summary>
        private static JsonSerializerSettings defaultSettings;

        /// <summary>
        /// Serializes the specified model to <c>json</c>.
        /// </summary>
        /// <param name="model">The <see cref="object" /></param>
        /// <returns>
        /// The <see cref="string" /></returns>
        public static string Serialize(this object model)
        {
            string result = string.Empty;

            try
            {
                JsonSerializerSettings defaultSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                defaultSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                defaultSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                defaultSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                result = JsonConvert.SerializeObject(model, defaultSettings);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not serialize Model of Type " + model.GetType(), ex);
            }

            return result;
        }

        /// <summary>
        /// Deserializes the specified <c>json</c>.
        /// </summary>
        /// <param name="json">The <c>json</c>.</param>
        /// <param name="format">The format.</param>
        /// <returns>the deserialized object</returns>
        public static object Deserialize(this string json, Type type)
        {
            try
            {
                JsonSerializerSettings defaultSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                defaultSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                defaultSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                return JsonConvert.DeserializeObject(json, type, defaultSettings);
            }
            catch (Exception ex)
            {
                throw new Exception("Error during content serialization of the package:" + ex.Message);
            }
        }

        /// <summary>
        /// Converts any kind of object to a byte array
        /// </summary>
        /// <typeparam name="T">any specific type</typeparam>
        /// <param name="value">The <see cref="T" /></param>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The <see cref="byte[]" /></returns>
        public static byte[] ToByte<T>(this T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    var serializer = SerializationContext.Default.GetSerializer<T>();
                    serializer.Pack(stream, value);
                    byte[] content = stream.ToArray();
                    return content;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error during content serialization of the package:" + ex.Message);
            }
        }

        /// <summary>
        /// Convert a byte array back to a specific model
        /// </summary>
        /// <param name="bytes">The <see cref="byte[]" /></param>
        /// <param name="type">The <see cref="Type" /></param>
        /// <returns>
        /// The <see cref="object" /></returns>
        public static object FromBytes(this byte[] bytes, Type type)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new InvalidOperationException();
            }

            try
            {
                var serializer = SerializationContext.Default.GetSerializer(type);
                return serializer.Unpack(new MemoryStream(bytes));
            }
            catch(Exception ex)
            {
            }

            return null;
        }

        /// <summary>
        /// To the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="G"></typeparam>
        /// <param name="f">The f.</param>
        /// <returns></returns>
        public static Expression<Func<T, G>> ToExpression<T, G>(this Func<T, G> f)
        {
            return x => f(x);
        }
    }
}
