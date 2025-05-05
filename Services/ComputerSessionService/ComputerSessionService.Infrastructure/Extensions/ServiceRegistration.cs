using ComputerSessionService.Application.Interfaces;
using ComputerSessionService.Application.Interfaces.Repositories;
using ComputerSessionService.Application.Interfaces.Services;
using ComputerSessionService.Application.Services;
using ComputerSessionService.Infrastructure.Persistence;
using ComputerSessionService.Infrastructure.Repositories;
using ComputerSessionService.Infrastructure.Services;
using InternetCafe.Common.Interfaces;
using InternetCafe.Common.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
namespace ComputerSessionService.Infrastructure.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ComputerSessionDbContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ComputerSessionDbContext).Assembly.FullName)));

            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IComputerRepository, ComputerRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // HTTP Client for AccountService
            services.AddHttpClient<IAccountServiceClient, AccountServiceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["ServiceUrls:AccountService"]);
            });

            // Logging
            services.AddScoped<IAuditLogger, AuditLogger>();

            // Services
            services.AddScoped<IComputerService, ComputerService>();
            services.AddScoped<ISessionService, SessionService>();

            return services;
        }
    }
}
