using MemoryGame.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Extensions;

public static class Mappers
{
    public static RegisterModelRequest ToRegisterModelRequest(this RegisterModel registerModel)
    {
        return new RegisterModelRequest
        {
            Email = registerModel.Email,
            Name = registerModel.Name,
            Password = registerModel.Password
        };
    }

    public static VerifyTokenModelRequest ToVerifyCodeModelRequest(this TokenModel codeModel)
    {
        return new VerifyTokenModelRequest
        {
            Token = codeModel.Token,
            Id = codeModel.Id
        };
    }

    public static LoginModelRequest ToLoginModelRequest(this LoginModel loginModel)
    {
        return new()
        {
            Email = loginModel.Email,
            Password = loginModel.Password
        };
    }
}
