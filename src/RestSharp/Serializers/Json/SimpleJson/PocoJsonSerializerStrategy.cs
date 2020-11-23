//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;

namespace RestSharp.Serializers.Json.SimpleJson {
    [GeneratedCode("simple-json", "1.0.0")]
    interface IJsonSerializerStrategy {
        bool TrySerializeNonPrimitiveObject(object input, out object? output);
    }

    [GeneratedCode("simple-json", "1.0.0")]
    class PocoJsonSerializerStrategy : IJsonSerializerStrategy {
        // ReSharper disable once CollectionNeverUpdated.Local
        readonly ReflectionUtils.ThreadSafeDictionary<Type, IDictionary<string, ReflectionUtils.GetDelegate>> _getCache = new(GetterValueFactory);

        static readonly string[] Iso8601Format = {
            @"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z",
            @"yyyy-MM-dd\THH:mm:ss\Z",
            @"yyyy-MM-dd\THH:mm:ssK"
        };

        static string MapClrMemberNameToJsonFieldName(string clrFieldName) => clrFieldName;

        static IDictionary<string, ReflectionUtils.GetDelegate> GetterValueFactory(Type type) {
            var result = new Dictionary<string, ReflectionUtils.GetDelegate>();

            foreach (var propertyInfo in ReflectionUtils.GetProperties(type)) {
                var getMethod = ReflectionUtils.GetGetterMethodInfo(propertyInfo);

                if (!propertyInfo.CanRead) continue;
                if (getMethod.IsStatic || !getMethod.IsPublic) continue;

                result[MapClrMemberNameToJsonFieldName(propertyInfo.Name)] = ReflectionUtils.GetGetMethod(propertyInfo);
            }

            foreach (var fieldInfo in ReflectionUtils.GetFields(type)) {
                if (fieldInfo.IsStatic || !fieldInfo.IsPublic) continue;

                result[MapClrMemberNameToJsonFieldName(fieldInfo.Name)] = ReflectionUtils.GetGetMethod(fieldInfo);
            }

            return result;
        }

        public bool TrySerializeNonPrimitiveObject(object input, out object? output)
            => TrySerializeKnownTypes(input, out output) || TrySerializeUnknownTypes(input, out output);

        static object SerializeEnum(Enum p) => Convert.ToDouble(p, CultureInfo.InvariantCulture);

        static bool TrySerializeKnownTypes(object input, out object? output) {
            var returnValue = true;

            switch (input) {
                case DateTime time:
                    output = time.ToUniversalTime().ToString(Iso8601Format[0], CultureInfo.InvariantCulture);
                    break;
                case DateTimeOffset offset:
                    output = offset.ToUniversalTime().ToString(Iso8601Format[0], CultureInfo.InvariantCulture);
                    break;
                case Guid guid:
                    output = guid.ToString("D");
                    break;
                case Uri:
                    output = input.ToString();
                    break;
                case Enum inputEnum:
                    output = SerializeEnum(inputEnum);
                    break;
                default:
                    returnValue = false;
                    output      = null;
                    break;
            }

            return returnValue;
        }

        bool TrySerializeUnknownTypes(object input, out object? output) {
            if (input == null) throw new ArgumentNullException(nameof(input));

            output = null;
            var type = input.GetType();
            if (type.FullName == null) return false;

            var obj     = new JsonObject();
            var getters = _getCache[type];

            foreach (var getter in getters) {
                if (getter.Value != null) obj.Add(MapClrMemberNameToJsonFieldName(getter.Key), getter.Value(input));
            }

            output = obj;
            return true;
        }
    }
}
