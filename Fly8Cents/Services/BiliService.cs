using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Richasy.BiliKernel;
using Richasy.BiliKernel.Bili.User;
using Richasy.BiliKernel.Services.User;
using RichasyKernel;

namespace Fly8Cents.Services;

public class BiliService
{
    public readonly IUserService UserService;

    public BiliService()
    {
        
        UserService = new UserService();
    }

}
