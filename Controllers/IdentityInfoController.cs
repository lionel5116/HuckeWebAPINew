using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.Serialization;
using System.Security.Claims;

namespace HuckeWEBAPI.Controllers
{
    public class IdentityInfoController : ApiController
    {

        [Route("api/SelectedUserInfo/{username=username}")]
        [AllowAnonymous]
        public IHttpActionResult GetSelectedUserInfo(string username)
        {
            try
            {
                return GetUserInfo(username);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        private IHttpActionResult GetUserInfo(string username)
        {
            var roles = new List<string>();
            string displayName = String.Empty, emailAddress = String.Empty;
            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain))
            {
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, username);
                if (user != null)
                {
                    displayName = user.DisplayName;
                    emailAddress = user.EmailAddress;
                    var groups = user.GetAuthorizationGroups(); // get the authorization groups - those are the "roles" 
                    foreach (Principal principal in groups)
                    {
                        roles.Add(principal.Name);// do something with the group (or role) in question
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            return Ok(new UserInfo
            {
                loginName = username,
                name = displayName,
                email = emailAddress,
                roles = roles.Distinct()
            });

        }

    }

    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public string loginName { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string email { get; set; }
        [DataMember]
        public IEnumerable<string> roles { get; set; }
    }
}
