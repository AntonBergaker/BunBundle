using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace BunBundle.Model {
    public static class JsonSettings {
        public static JsonSerializerOptions GetDeserializeOptions() {
            return new JsonSerializerOptions {
                PropertyNamingPolicy = new JsonPascalCaseifier(),
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
        }
    }

    class JsonSnakeCaseifier : JsonNamingPolicy {
        public override string ConvertName(string name) {
            return Utils.PascalToSnakecase(name);
        }

    }

    class JsonPascalCaseifier : JsonNamingPolicy {
        public override string ConvertName(string name) {
            return Utils.SnakecaseToPascal(name);
        }
    }
}
