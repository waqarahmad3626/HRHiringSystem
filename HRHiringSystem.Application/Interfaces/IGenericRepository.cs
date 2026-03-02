using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRHiringSystem.Domain.Entities;

namespace HRHiringSystem.Application.Interfaces;

public interface IGenericRepository<T> where T : AuditableEntity
{
    /// <summary>
    /// Add a new entity to the database. Optional createdBy user id will be applied to audit fields.
    /// </summary>
    Task<T> AddAsync(T entity, Guid? createdBy = null);

    /// <summary>
    /// Get an entity by its identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get all entities of this type (excludes soft-deleted items).
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Update an existing entity. Optional updatedBy user id will be applied to audit fields.
    /// </summary>
    Task UpdateAsync(T entity, Guid? updatedBy = null);

    /// <summary>
    /// Soft delete an entity by its identifier. Optional updatedBy user id will be applied to audit fields.
    /// </summary>
    Task DeleteAsync(Guid id, Guid? updatedBy = null);
}
