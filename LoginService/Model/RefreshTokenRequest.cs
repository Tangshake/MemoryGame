﻿namespace LoginService.Model;

public class RefreshTokenRequest
{
    public int UserId { get; set; }
    public string JwtToken { get; set; }
    public string RefreshToken { get; set; }
}
