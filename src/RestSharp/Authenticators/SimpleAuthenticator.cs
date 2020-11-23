//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using JetBrains.Annotations;

namespace RestSharp.Authenticators
{
    /// <summary>
    /// Simplistic authenticator, which sends both
    /// username and password as request parameters.
    /// </summary>
    [PublicAPI]
    public class SimpleAuthenticator : IAuthenticator
    {
        readonly string _password;
        readonly string _passwordKey;
        readonly string _username;
        readonly string _usernameKey;

        /// <summary>
        /// Specify names and values for the request parameters
        /// </summary>
        /// <param name="usernameKey">Parameter name for username</param>
        /// <param name="username">Username</param>
        /// <param name="passwordKey">Parameter name for password</param>
        /// <param name="password">Password</param>
        public SimpleAuthenticator(string usernameKey, string username, string passwordKey, string password)
        {
            _usernameKey = usernameKey;
            _username    = username;
            _passwordKey = passwordKey;
            _password    = password;
        }

        /// <inheritdoc />
        public void Authenticate(IRestClient client, IRestRequest request)
            => request
                .AddParameter(_usernameKey, _username)
                .AddParameter(_passwordKey, _password);
    }
}