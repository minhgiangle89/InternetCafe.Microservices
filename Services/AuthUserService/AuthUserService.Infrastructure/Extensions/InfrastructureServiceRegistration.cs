using AuthUserService.Application.Interfaces.Repositories;
using AuthUserService.Application.Interfaces.Services;
using AuthUserService.Application.Interfaces;
using AuthUserService.Application.Services;
using AuthUserService.Infrastructure.Identity;
using AuthUserService.Infrastructure.Repositories;
using InternetCafe.Common.Repositories.IRepositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthUserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthUserService.Infrastructure.Extensions
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AuthUserDbContext>(options => options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(AuthUserDbContext).Assembly.FullName)));

            // Database
            services.AddDatabaseServices(configuration);

            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();

            // UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Identity and Authentication
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenGenerator, TokenGenerator>();

            // Logging
            //services.AddScoped<IAuditLogger, AuditLogger>();

            // Register application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            return services;
        }
    }
}

