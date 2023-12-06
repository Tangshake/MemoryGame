using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Services
{
    public interface IVerifyTokenApiClient
    {
        Task<bool> VerifyTokenAsync(VerifyTokenModelRequest verifyCodeModelRequest, string requestUri);
    }
}
