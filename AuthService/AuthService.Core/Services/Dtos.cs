namespace AuthService.Services;

public record RegisterCompanyRequest(string CompanyName, string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);

public record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

public record ClientInfo(string? Ip, string? UserAgent);

public record AuthResult(TokenResponse? Tokens, string? Error = null)
{
    public static AuthResult Ok(TokenResponse t) => new(t);
    public static AuthResult Fail(string error) => new(null, error);
}
