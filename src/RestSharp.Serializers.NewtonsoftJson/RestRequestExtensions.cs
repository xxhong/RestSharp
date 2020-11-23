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

using JetBrains.Annotations;
using Newtonsoft.Json;

namespace RestSharp.Serializers.NewtonsoftJson {
    [PublicAPI]
    public static class RestRequestExtensions {
        /// <summary>
        /// Use Newtonsoft.Json serializer for a single request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IRestRequest UseNewtonsoftJson(this IRestRequest request) {
            request.JsonSerializer = new JsonNetSerializer();
            return request;
        }

        /// <summary>
        /// Use Newtonsoft.Json serializer for a single request, with custom settings
        /// </summary>
        /// <param name="request"></param>
        /// <param name="settings">Newtonsoft.Json serializer settings</param>
        /// <returns></returns>
        public static IRestRequest UseNewtonsoftJson(this IRestRequest request, JsonSerializerSettings settings) {
            request.JsonSerializer = new JsonNetSerializer(settings);
            return request;
        }
    }
}
