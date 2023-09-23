using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Cdm.Authentication.OAuth2;
using Cdm.Authentication.Utils;

namespace Cdm.Authentication.Clients
{
    public class PatreonAuth : AuthorizationCodeFlow, IUserInfoProvider
    {
        public PatreonAuth(Configuration configuration) : base(configuration)
        {
        }
        public const string url = "https://www.patreon.com";
        public override string authorizationUrl => $"{url}/oauth2/authorize";
        public override string accessTokenUrl => $"{url}/api/oauth2/token";
        public const string userInfoUrl = url + "/api/oauth2/v2/identity";

        public async Task<IUserInfo> GetUserInfoAsync(CancellationToken cancellationToken = default)
        {
            var identity = await GetIdentityAsync(cancellationToken);

            return new PatreanUserInfo()
            {
                id = identity.data.id,
                name = identity.data.attributes.full_name,
                email = identity.data.attributes.email,
                picture = identity.data.attributes.image_url,
            };
        }

        public async Task<PatreonIdentity> GetIdentityAsync(CancellationToken cancellationToken = default)
        {
            if (accessTokenResponse == null)
                throw new AccessTokenRequestException(new AccessTokenRequestError()
                {
                    code = AccessTokenRequestErrorCode.InvalidGrant,
                    description = "Authentication required."
                }, null);

            var authenticationHeader = accessTokenResponse.GetAuthenticationHeader();
            var url = UrlBuilder.New(userInfoUrl).SetQueryParameters(new Dictionary<string, string>{
                {"include","memberships"}
            }).ToString() + "&fields%5Buser%5D=about,created,email,first_name,full_name,image_url,last_name";

            return await httpClient.GetAsync<PatreonIdentity>(url, authenticationHeader, cancellationToken);
        }
    }

    public class PatreanUserInfo : IUserInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }

        public string picture { get; set; }
    }


    [DataContract]
    public class PatreonIdentity
    {
        [DataMember(Name = "data", IsRequired = true)]
        public Data data { get; set; }

        [DataContract]
        public class Data
        {
            [DataMember(Name = "id", IsRequired = true)]
            public string id { get; set; }

            [DataMember(Name = "attributes", IsRequired = true)]
            public Attributes attributes { get; set; }

            [DataMember(Name = "relationships", IsRequired = true)]
            public Relationships relationships { get; set; }

            [DataContract]
            public class Attributes
            {

                [DataMember(Name = "email")]
                public string email { get; set; }
                [DataMember(Name = "full_name")]
                public string full_name { get; set; }
                [DataMember(Name = "image_url")]
                public string image_url { get; set; }
            }

            [DataContract]
            public class Relationships
            {
                [DataMember(Name = "memberships", IsRequired = true)]
                public Memberships memberships { get; set; }

                [DataContract]
                public class Memberships
                {
                    [DataMember(Name = "data", IsRequired = true)]
                    public Membership[] data { get; set; }

                    [DataContract]
                    public class Membership
                    {
                        [DataMember(Name = "campaign", IsRequired = true)]
                        public string campaign { get; set; }
                    }
                }
            }
        }
    }
}