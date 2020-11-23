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

using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using RestSharp.Serialization.Json;

namespace RestSharp.Tests.TestData {
    public static class Resources {
        static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        public static XDocument LoadFromResource(string fileName) {
            var       resourceName = Assembly.GetManifestResourceNames().First(str => str.EndsWith(fileName));
            using var stream       = Assembly.GetManifestResourceStream(resourceName);
            return XDocument.Load(stream);
        }

        public static string GetResource(string fileName) {
            var       resourceName = Assembly.GetManifestResourceNames().First(str => str.EndsWith(fileName));
            using var stream       = Assembly.GetManifestResourceStream(resourceName);
            using var reader       = new StreamReader(stream);
            return reader.ReadToEnd();
        }


    }
}
