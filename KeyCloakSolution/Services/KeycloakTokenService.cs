
using KeyCloakSolution.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KeyCloakSolution.Services;

public class KeycloakTokenService : IKeycloakTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly KeycloakSetting _keycloakSetting;

    public KeycloakTokenService(IHttpClientFactory httpClientFactory, IOptions<KeycloakSetting> keycloakSetting)
    {
        _httpClientFactory = httpClientFactory;
        _keycloakSetting = keycloakSetting.Value;
    }

    public async Task<KeycloakTokenResponseDto?> GetTokenResponseAsync(
                KeycloakUserDto keycloakUserDto)
    {
        using (var httpClient = _httpClientFactory.CreateClient())
        {

            var keycloakTokenRequestDto = new KeycloakTokenRequestDto
            {
                GrantType = KeycloakAccessTokenConsts.GrantTypePassword,
                ClientId = _keycloakSetting.ClientId ??
                    throw new Exception(nameof(_keycloakSetting.ClientId)),
                ClientSecret = _keycloakSetting.ClientSecret ??
                    throw new Exception(nameof(_keycloakSetting.ClientSecret)),
                Username = keycloakUserDto.Username,
                Password = keycloakUserDto.Password
            };


            var tokenRequestBody = KeycloakTokenUtils.GetTokenRequestBody(keycloakTokenRequestDto);
            var response = await httpClient
                .PostAsync($"{_keycloakSetting.BaseUrl}/token", tokenRequestBody)
                .ConfigureAwait(false);


            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var keycloakTokenResponseDto = JsonConvert.DeserializeObject<KeycloakTokenResponseDto>(
                                responseJson);

            return keycloakTokenResponseDto;
        }
    }
}
