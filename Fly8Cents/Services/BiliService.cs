using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Richasy.BiliKernel;
using Richasy.BiliKernel.Bili.User;
using RichasyKernel;

namespace Fly8Cents.Services;

public sealed class BiliService : IHostedService
{
    private readonly Kernel _kernel = Kernel.CreateBuilder()
        .AddUserService()
        .Build();
    private CancellationToken _cancellationToken;
    public IUserService UserService = null!;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        UserService = _kernel.Services.GetService<IUserService>() ?? throw new InvalidOperationException();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
