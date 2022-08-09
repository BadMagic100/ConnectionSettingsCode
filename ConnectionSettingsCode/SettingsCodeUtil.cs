using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ConnectionSettingsCode
{
    public static class SettingsCodeUtil
    {
        private static readonly JsonSerializerSettings serSettings = new()
        {
            Formatting = Formatting.None,
            Culture = CultureInfo.InvariantCulture
        };

        public static string Encode(IEnumerable<object> values)
        {
            string data = JsonConvert.SerializeObject(values, serSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using MemoryStream i = new(bytes);
            using MemoryStream o = new();
            // the actual block scope matters here - we need to dispose the GZ stream to finish the compression before inspecting
            // the data
            using (GZipStream gz = new(o, CompressionMode.Compress))
            {
                i.CopyTo(gz);
            }
            return Convert.ToBase64String(o.ToArray());
        }

        public static IEnumerable<JValue>? Decode(string code)
        {
            try
            {
                byte[] data = Convert.FromBase64String(code);
                using MemoryStream i = new(data);
                using MemoryStream o = new();
                // the actual block scope matters here - we need to dispose the GZ stream to finish the decompression before inspecting
                // the data
                using (GZipStream gz = new(i, CompressionMode.Decompress))
                {
                    gz.CopyTo(o);
                }
                string json = Encoding.UTF8.GetString(o.ToArray());
                return JsonConvert.DeserializeObject<List<JValue>>(json, serSettings);
            }
            catch (FormatException) 
            {
                return null;
            }
            catch (Exception x)
            {
                ConnectionSettingsCode.Instance.LogError($"Failed to deserialize settings code: {x}");
                return null;
            }
        }
    }
}
