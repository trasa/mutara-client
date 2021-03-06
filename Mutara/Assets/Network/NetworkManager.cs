﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Mutara.Network.Api;
using Mutara.Network.Api.Auth;
using Mutara.Network.Api.Player;
using Newtonsoft.Json;
using UnityEngine;

namespace Mutara.Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        public AuthEndpoint Auth { get; private set; }
        
        public PlayerEndpoint Player { get; private set; }
        
        /// <summary>
        /// What we get when we log in successfully and want to call
        /// authenticated /authorized endpoints
        /// </summary>
        public string IdToken { get; set; }

        void Awake()
        {
            Debug.Log("NetworkManager awake!");

            Instance = this;
            Instance.Auth = new AuthEndpoint();
            Instance.Player = new PlayerEndpoint();

            // This object will persist until the game is closed
            DontDestroyOnLoad(gameObject);
            // Initialize AWS SDK
            // TODO
            // UnityInitializer.AttachToGameObject(gameObject);
        }

        public abstract class Endpoint
        {
            public async Task<TResponse> Post<TRequest, TResponse>(string path, TRequest request)
                where TRequest : class
                where TResponse : class
            {
                var client = new HttpClient();
                var r = new HttpRequestMessage
                {
                    // TODO a way to set this...
                    RequestUri = new Uri($"https://localhost:5001{path}"),
                    Method = HttpMethod.Post,
                    Content =
                        new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"),
                };
                // request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                r.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.SendAsync(r);
                // TODO all manner of error conditions here...
                string raw = await response.Content.ReadAsStringAsync();
                ApiOkResponse<TResponse> responseObject = JsonConvert.DeserializeObject<ApiOkResponse<TResponse>>(raw);
                return responseObject.Content;
            }
            
            public async Task<TResponse> GetAuth<TResponse>(string path)
                where TResponse : class
            {
                var client = new HttpClient();
                var r = new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://localhost:5001{path}"),
                    Method = HttpMethod.Get,
                };
                r.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                r.Headers.Add("Authorization", $"Bearer {Instance.IdToken}");

                HttpResponseMessage response = await client.SendAsync(r);
                // TODO all manner of error conditions here...
                string raw = await response.Content.ReadAsStringAsync();
                ApiOkResponse<TResponse> responseObject = JsonConvert.DeserializeObject<ApiOkResponse<TResponse>>(raw);
                return responseObject.Content; 
            }
        }



        public class AuthEndpoint : Endpoint
        {
            internal AuthEndpoint()
            {
            }

            public async Task<SignInResponse> SignIn(Guid userId, string password)
            {
                Debug.Log($"Sending SignIn request for {userId}");
                var signInRequest = new SignInRequest
                {
                    UserId = userId,
                    Password = password,
                    ClientVersion = "123", // TODO fix this...
                };
                // TODO error conditions...
                return await Post<SignInRequest, SignInResponse>("/Auth/signin", signInRequest);
            }

            public async Task<CreateAccountResponse> CreateAccount(Guid userId, string password)
            {
                Debug.Log($"Sending CreateAccount request for {userId}");
                var createRequest = new CreateAccountRequest
                {
                    UserId = userId,
                    Password = password,
                };
                // TODO error conditions...
                return await Post<CreateAccountRequest, CreateAccountResponse>("/Auth/create", createRequest);
            }
        }

        public class PlayerEndpoint : Endpoint
        {
            internal PlayerEndpoint() {}

            public async Task<PlayerDetailsResponse> GetPlayerDetails(Guid playerId)
            {
                return await GetAuth<PlayerDetailsResponse>($"/Player/details/{playerId}");
            }
        }
    }
}
