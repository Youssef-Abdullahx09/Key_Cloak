namespace KeyCloakSolution.Services;

public interface IKeycloakTokenService
{
    Task<KeycloakTokenResponseDto?> GetTokenResponseAsync(
            KeycloakUserDto keycloakUserDto);
}
