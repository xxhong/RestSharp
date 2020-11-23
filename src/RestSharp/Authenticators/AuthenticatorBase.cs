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

namespace RestSharp.Authenticators {
    /// <summary>
    /// 
    /// </summary>
    public abstract class AuthenticatorBase : IAuthenticator {
        /// <summary>
        /// The default constructor that needs a token
        /// </summary>
        /// <param name="token"></param>
        protected AuthenticatorBase(string token)
            => Token = token;

        /// <summary>
        /// Generic token, used differently for each authenticator
        /// </summary>
        protected string Token { get; }

        /// <summary>
        /// Get the authentication header parameter for the request 
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        protected abstract Parameter GetAuthenticationParameter(string accessToken);

        /// <inheritdoc />
        public void Authenticate(IRestClient client, IRestRequest request)
            => request.AddOrUpdateParameter(GetAuthenticationParameter(Token));
    }
}
