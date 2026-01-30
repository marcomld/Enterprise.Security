using Enterprise.Security.Application.Common;
using Enterprise.Security.Application.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Security.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> GetByIdAsync(Guid id);
        Task<Result> ActivateAsync(Guid id);
        Task<Result> DeactivateAsync(Guid id);
        // 👇 CORRECCIÓN AQUÍ: Cambiar Task<Result> por Task<Result<string>>
        Task<Result<string>> CreateAsync(CreateUserDto request);
    }
}
