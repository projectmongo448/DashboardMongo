using System;
using System.Threading;
using DotNetify;
using DotNetify.Security;
using Microsoft.IdentityModel.Tokens;

namespace Website.Server
{
   public class SetAccessTokenAttribute : Attribute { }

   [Authorize]
   [SetAccessToken]
   public class SecurePageVM : BaseVM
   {
      private Timer _timer;
      private SecurityToken _accessToken;
      private readonly IPrincipalAccessor _principalAccessor;

      private int AccessExpireTime => (int) (_accessToken.ValidTo - DateTime.UtcNow).TotalSeconds;

      public string SecureCaption { get; set; }
      public string SecureData { get; set; }

      public SecurePageVM(IPrincipalAccessor principalAccessor)
      {
         _principalAccessor = principalAccessor;
      }

      public override void Dispose() => _timer?.Dispose();

      public void SetAccessToken(SecurityToken accessToken)
      {
         if (_accessToken?.ValidTo != accessToken.ValidTo)
         {
            _accessToken = accessToken;
            SecureCaption = $"Authenticated user: \"{_principalAccessor.Principal?.Identity.Name}\"";
            Changed(nameof(SecureCaption));

            // IMPORTANT: Create new timer if access token changes to make sure the timer thread uses
            // the new hub caller context with the updated claims principal from the new token.
            _timer?.Dispose();
            _timer = new Timer(state =>
            {
               SecureData = _accessToken != null ? $"Access token will expire in {AccessExpireTime} seconds" : null;
               Changed(nameof(SecureData));
               PushUpdates();
            }, null, 0, 1000);
         }
      }
   }

   [Authorize(Role = "admin")]
   [SetAccessToken]
   public class AdminSecurePageVM : BaseVM
   {
      public string TokenIssuer { get; set; }
      public string TokenValidFrom { get; set; }
      public string TokenValidTo { get; set; }

      public void Refresh()
      {
         /* no op */
      }

      public void SetAccessToken(SecurityToken accessToken)
      {
         TokenIssuer = $"Token issuer: \"{accessToken.Issuer}\"";
         TokenValidFrom = $"Valid from: {accessToken.ValidFrom:R}";
         TokenValidTo = $"Valid to: {accessToken.ValidTo:R}";

         Changed(nameof(TokenIssuer));
         Changed(nameof(TokenValidFrom));
         Changed(nameof(TokenValidTo));
      }
   }
}