using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace BunBundle.Model {
    public static class JsonSettings {

        public static JsonSerializerOptions GetSerializeOptions() {
            return new JsonSerializerOptions {
                PropertyNamingPolicy = new JsonSnakeCaseifier(),
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                WriteIndented = true
            };
        }
    }

    class JsonSnakeCaseifier : JsonNamingPolicy {
        public override string ConvertName(string name) {
            return Utils.PascalToSnakecase(name);
        }

    }

}
