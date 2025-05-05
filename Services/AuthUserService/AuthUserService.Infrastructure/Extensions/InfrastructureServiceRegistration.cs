using AuthUserService.Application.Interfaces.Repositories;
using AuthUserService.Application.Interfaces.Services;
using AuthUserService.Application.Interfaces;
using AuthUserService.Application.Services;
using AuthUserService.Infrastructure.Service;
using AuthUserService.Infrastructure.Repositories;
using InternetCafe.Common.Repositories.IRepositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AuthUserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using InternetCafe.Common.Interfaces;

namespace AuthUserService.Infrastructure.Extensions
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AuthUserDbContext>(options => options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AuthUserDbContext).Assembly.FullName)));

            services.AddDatabaseServices(configuration);

            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenGenerator, TokenGenerator>();

            // Logging
            services.AddScoped<IAuditLogger, AuditLogger>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }
    }
}

