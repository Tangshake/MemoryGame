using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.SynchronousDataService.Register
{
    public interface IRegisterService
    {
        public Task<RegisterModelResponse> RegisterUserAsync(RegisterModelRequest registerModelRequest, string requestUri);
    }
}
