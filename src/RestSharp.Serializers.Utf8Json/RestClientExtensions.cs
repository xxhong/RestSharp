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

namespace RestSharp.Serializers.Utf8Json
{
    [PublicAPI]
    public static class RestClientExtensions
    {
        /// <summary>
        /// Use Utf8Json serializer with default formatter resolver
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IRestClient UseUtf8Json(this IRestClient client) => client.UseSerializer(() => new Utf8JsonSerializer());

        /// <summary>
        /// Use Utf8Json serializer with custom formatter resolver
        /// </summary>
        /// <param name="client"></param>
        /// <param name="resolver">Utf8Json deserialization formatter resolver</param>
        /// <returns></returns>
        public static IRestClient UseUtf8Json(this IRestClient client, IJsonFormatterResolver resolver)
            => client.UseSerializer(() => new Utf8JsonSerializer(resolver));
    }
}