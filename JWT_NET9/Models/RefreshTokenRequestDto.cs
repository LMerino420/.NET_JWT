﻿namespace JWT_NET9.Models
{
    public class RefreshTokenRequestDto
    {
        public Guid UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}
