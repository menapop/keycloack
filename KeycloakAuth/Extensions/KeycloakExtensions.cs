﻿using KeycloakAuth.Clients;
using KeycloakAuth.ClaimsTransformations;
using KeycloakAuth.Handlers;
using KeycloakAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace KeycloakAuth.Extensions
{
    public static class KeycloakExtensions
    {
        public static IServiceCollection AddKeycloakJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            KeycloakOptions keycloakOptions = new();
            configuration.GetSection("keycloak").Bind(keycloakOptions);
            services.Configure<KeycloakOptions>(configuration.GetSection("keycloak"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.Audience = $"{keycloakOptions.AuthUrl}/realms/{keycloakOptions.Realm}";
                options.SaveToken = keycloakOptions.SaveToken;
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                options.MetadataAddress = $"{keycloakOptions.AuthUrl}/realms/{keycloakOptions.Realm}/.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = keycloakOptions.Audience,
                    RoleClaimType = keycloakOptions.RoleClaimType,
                    NameClaimType = keycloakOptions.NameClaimType,
                    ClockSkew = keycloakOptions.ClockSkew ?? TimeSpan.FromSeconds(15),
                };

                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrEmpty(context.Request.Query["access_token"]))
                        {
                            context.Token = context.Request.Query["access_token"];

                        }
                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }
                        if (context.Exception.GetType() == typeof(SecurityTokenValidationException))
                        {
                            context.Response.Headers["Token-Validation"] = "false";
                        }
                        return Task.CompletedTask;
                    }

                };
            });

            services.AddScoped<IClaimsTransformation>(_ => new KeycloakRolesClaimsTransformation(keycloakOptions.RoleClaimType, keycloakOptions.Audience));

            return services;
        }


        public static IServiceCollection AddKeycloakClient(this IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddTransient<AttachAccessTokenHandler>();

            services.AddHttpClient("keycloakAccessTokenClient", (serviceProvider, client) =>
            {
                KeycloakOptions settings = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
                client.BaseAddress = new Uri(settings.AuthUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddHttpClient("keycloakClient", (serviceProvider, client) =>
            {
                KeycloakOptions settings = serviceProvider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
                client.BaseAddress = new Uri($"{settings.AuthUrl}/admin/realms/{settings.Realm}/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }).AddHttpMessageHandler<AttachAccessTokenHandler>();

            services.AddScoped<IKeycloakClient, KeycloakClient>();

            return services;    
        }
    }
    }
