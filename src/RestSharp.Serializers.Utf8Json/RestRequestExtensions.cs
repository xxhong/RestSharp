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

using JetBrains.Annotations;
using Utf8Json;

namespace RestSharp.Serializers.Utf8Json {
    [PublicAPI]
    public static class RestRequestExtensions {
        /// <summary>
        /// Use Utf8Json serializer for a single request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IRestRequest UseUtf8Json(this IRestRequest request) {
            request.JsonSerializer = new Utf8JsonSerializer();
            return request;
        }

        /// <summary>
        /// Use Utf8Json serializer for a single request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="resolver">JSON formatter resolver instance to provide custom options to Utf8Json</param>
        /// <returns></returns>
        public static IRestRequest UseUtf8Json(this IRestRequest request, IJsonFormatterResolver resolver) {
            request.JsonSerializer = new Utf8JsonSerializer(resolver);
            return request;
        }
    }
}
