using Assets.Source.Core;
using Assets.Source.Utility;
using Contract.DTO.Feature.Identity.Command;
using Contract.DTO.Feature.Identity.Response;
using System;
using System.Threading.Tasks;
using UnityEngine.Analytics;

namespace Assets.Source.Service
{
    public class AuthService : IService
    {
        #region Attributes
        private GameService gameService;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;

        public TokenDTO Token { get; private set; }
        #endregion

        public AuthService() { }

        #region Methods
        public Task InitializeAsync()
        {
            gameService = ServiceProvider.Get<GameService>();

            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<string> GetValidAccessToken()
        {
            if (Token == null)
                throw new InvalidOperationException("Not authenticated.");

            if (Token.AccessExpiry > DateTime.UtcNow.AddMinutes(1))
                return Token.AccessToken;

            if (!await Refresh())
                throw new UnauthorizedAccessException("Token refresh failed.");

            return Token.AccessToken;
        }

        private void ConfigToken(
            TokenDTO token)
        {
            if (token == null)
                throw new Exception("Token response is null");

            if (string.IsNullOrWhiteSpace(token.AccessToken))
                throw new Exception("Access token is missing");

            // Configure http
            HttpCaller.SetBearerToken(token.AccessToken);

            // Apply runtime
            if (Token == token)
                return;

            Token = token;
        }
        #region Senders
        public async Task GoogleAuth()
        {
            string idToken = await GoogleHelper.GetGoogleIdTokenAsync();
            var dto = new GoogleAuthDTO
            {
                IdToken = idToken
            };

            var result = await HttpCaller.PostAsync<GoogleAuthDTO, TokenDTO>(
                $"{Configuration.IDENTITY_CONTROLLER}google", dto);

            ConfigToken(result);
            gameService.Start();
        }

        public async Task Login(
            string email,
            string password)
        {
            var dto = new LoginDTO
            {
                Email = email,
                Password = password
            };

            var result = await HttpCaller.PostAsync<LoginDTO, TokenDTO>(
                $"{Configuration.IDENTITY_CONTROLLER}login", dto);

            ConfigToken(result);
            gameService.Start();
        }

        public async Task Register(
            string email,
            string password,
            string fullName)
        {
            var dto = new RegisterDTO
            {
                Email = email,
                Password = password,
                Name = fullName,
            };

            var result = await HttpCaller.PostAsync<RegisterDTO, TokenDTO>(
                $"{Configuration.IDENTITY_CONTROLLER}register", dto);

            ConfigToken(result);
            gameService.Start();
        }

        public async Task<bool> Refresh()
        {
            try
            {
                // Prime the HTTP client header with the old access token so the backend permits the call
                HttpCaller.SetBearerToken(Token.AccessToken);

                var dto = new RefreshTokenDTO
                {
                    RefreshToken = Token.RefreshToken
                };

                var result = await HttpCaller.PostAsync<RefreshTokenDTO, TokenDTO>(
                    $"{Configuration.IDENTITY_CONTROLLER}refresh", dto);

                ConfigToken(result);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task UpdateProfile(
            string name,
            DateTime dob)
        {
            var dto = new UpdateProfileDTO
            {
                Dob = dob,
                Gender = Gender.Unknown.ToString(),
                Name = name
            };

            await HttpCaller.PutAsync<object, TokenDTO>(
                $"{Configuration.IDENTITY_CONTROLLER}profile", dto);
        }
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}