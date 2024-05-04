﻿using LINGYUN.Abp.Saas.EntityFrameworkCore;
using LINGYUN.Abp.TextTemplating.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace LY.MicroService.BackendAdmin.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class BackendAdminMigrationsDbContext : AbpDbContext<BackendAdminMigrationsDbContext>
{
    public BackendAdminMigrationsDbContext(DbContextOptions<BackendAdminMigrationsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureSaas();
        modelBuilder.ConfigureTextTemplating();
        modelBuilder.ConfigureFeatureManagement();
        modelBuilder.ConfigureSettingManagement();
        modelBuilder.ConfigurePermissionManagement();
    }
}
