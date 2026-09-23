using RMS.Application.Controlr;
using RMS.Application.Excep;
using RMS.Application.Interface;
using RMS.Domain.Entities;
using RMS.Domain.Interfaces;
using RMS.Domain.Security;
using RMS.Shared.Contracts.Commands;
using RMS.Shared.Contracts.Queries;
using RMS.Shared.Contracts.Responses;
using RMS.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace RMS.Application.Service;

/// <inheritdoc cref="IUser"/>
public sealed class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUserAccessRules accessRules) : IUser
{
    /// <inheritdoc/>
    public async Task<Guid> AddAsync(CreateUserCommand command, CancellationToken token = default)
    {
        Validate(command);
        Normalize(command);

        var permissions = ToPermissionValues(command.Permissions);
        accessRules.EnsurePermissionsAreValid(permissions);

        if (await userRepository.MobileExistsAsync(command.MobileNumber, token: token))
            throw new InvalidOperationException("شماره موبایل قبلاً برای کاربر دیگری ثبت شده است.");

        var user = new UserEntities
        {
            Id = Guid.NewGuid(),
            FirstName = command.FirstName,
            LastName = command.LastName,
            MobileNumber = command.MobileNumber,
            PasswordHash = passwordHasher.Hash(command.Password),
            Role = command.Role
        };

        SetPermissions(user, permissions);
        return await userRepository.AddAsync(user, token) ? user.Id : Guid.Empty;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(UpdateUserCommand command, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(command.Password))
            command.Password = null;

        Validate(command);
        EnsureNotEmpty(command.Id, "شناسهٔ کاربر الزامی است.");
        Normalize(command);

        var permissions = ToPermissionValues(command.Permissions);
        accessRules.EnsurePermissionsAreValid(permissions);

        var user = await userRepository.GetForUpdateAsync(command.Id, token)
            ?? throw new KeyNotFoundException("کاربر موردنظر یافت نشد.");

        if (await userRepository.MobileExistsAsync(command.MobileNumber, command.Id, token))
            throw new InvalidOperationException("شماره موبایل قبلاً برای کاربر دیگری ثبت شده است.");

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.MobileNumber = command.MobileNumber;
        user.Role = command.Role;

        if (!string.IsNullOrWhiteSpace(command.Password))
            user.PasswordHash = passwordHasher.Hash(command.Password);

        SetPermissions(user, permissions);
        return await userRepository.UpdateAsync(user, token);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(DeleteUserCommand command, CancellationToken token = default)
    {
        EnsureNotEmpty(command.Id, "شناسهٔ کاربر الزامی است.");
        return userRepository.DeleteAsync(command.Id, token);
    }

    /// <inheritdoc/>
    public Task<UserResponse?> GetAsync(Guid id, CancellationToken token = default)
    {
        EnsureNotEmpty(id, "شناسهٔ کاربر الزامی است.");
        return userRepository.GetAsync(id, token);
    }

    /// <inheritdoc/>
    public Task<PagedResponse<UserListItemResponse>> GetAllAsync(UserQuery query, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Role.HasValue && !Enum.IsDefined(query.Role.Value))
            throw new ValidationException("نقش کاربر نامعتبر است.");

        query.SearchTerm = string.IsNullOrWhiteSpace(query.SearchTerm)
            ? null
            : query.SearchTerm.Trim();

        return userRepository.GetAllAsync(query, token);
    }

    /// <inheritdoc/>
    public async Task<bool> HasAccessAsync(
        Guid userId,
        UserSection section,
        UserAccessOperation operation,
        CancellationToken token = default)
    {
        if (userId == Guid.Empty)
            return false;

        var access = await userRepository.GetAccessSnapshotAsync(userId, token);
        return access is not null && accessRules.HasAccess(access, section, operation);
    }

    private static void Validate(object command)
    {
        if (!command.TryValidate(out var validationErrors))
            throw new ValidationException(validationErrors.JsonErrors());
    }

    private static void Normalize(CreateUserCommand command)
    {
        command.FirstName = UserInputNormalizer.NormalizeName(command.FirstName);
        command.LastName = UserInputNormalizer.NormalizeName(command.LastName);
        command.MobileNumber = UserInputNormalizer.NormalizeMobile(command.MobileNumber);
    }

    private static void Normalize(UpdateUserCommand command)
    {
        command.FirstName = UserInputNormalizer.NormalizeName(command.FirstName);
        command.LastName = UserInputNormalizer.NormalizeName(command.LastName);
        command.MobileNumber = UserInputNormalizer.NormalizeMobile(command.MobileNumber);
    }

    private static IReadOnlyCollection<UserPermissionValue> ToPermissionValues(
        IEnumerable<UserPermissionCommand> permissions) => permissions
            .Select(permission => new UserPermissionValue(permission.Section, permission.Operations))
            .ToArray();

    private static void SetPermissions(
        UserEntities user,
        IEnumerable<UserPermissionValue> permissions)
    {
        var desiredPermissions = permissions.ToDictionary(
            permission => permission.Section,
            permission => permission.Operations);

        foreach (var currentPermission in user.Permissions.ToArray())
        {
            if (desiredPermissions.Remove(currentPermission.Section, out var operations))
            {
                currentPermission.Operations = operations;
                continue;
            }

            user.Permissions.Remove(currentPermission);
        }

        foreach (var permission in desiredPermissions)
        {
            user.Permissions.Add(new UserPermissionEntities
            {
                FkUser = user.Id,
                Section = permission.Key,
                Operations = permission.Value
            });
        }
    }

    private static void EnsureNotEmpty(Guid id, string message)
    {
        if (id == Guid.Empty)
            throw new ValidationException(message);
    }
}