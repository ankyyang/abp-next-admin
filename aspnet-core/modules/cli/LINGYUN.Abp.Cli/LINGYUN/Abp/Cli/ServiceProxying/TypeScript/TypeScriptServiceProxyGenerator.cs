﻿using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Cli;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.ServiceProxying;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Modeling;
using Volo.Abp.IO;
using Volo.Abp.Json;

namespace LINGYUN.Abp.Cli.ServiceProxying.TypeScript;
public class TypeScriptServiceProxyGenerator : ServiceProxyGeneratorBase<TypeScriptServiceProxyGenerator>, ITransientDependency
{
    public const string Name = "TS";

    private readonly ITypeScriptProxyGenerator _typeScriptProxyGenerator;

    public TypeScriptServiceProxyGenerator(
        CliHttpClientFactory cliHttpClientFactory,
        IJsonSerializer jsonSerializer,
        ITypeScriptProxyGenerator typeScriptProxyGenerator)
        : base(cliHttpClientFactory, jsonSerializer)
    {
        _typeScriptProxyGenerator = typeScriptProxyGenerator;
    }

    public async override Task GenerateProxyAsync(Volo.Abp.Cli.ServiceProxying.GenerateProxyArgs args)
    {
        var applicationApiDescriptionModel = await GetApplicationApiDescriptionModelAsync(args);
        var outputFolderRoot = args.Output;

        foreach (var module in applicationApiDescriptionModel.Modules)
        {
            Logger.LogInformation($"Generating model script with remote service: {module.Value.RemoteServiceName}.");

            foreach (var controller in module.Value.Controllers)
            {
                Logger.LogInformation($"  [{module.Value.RemoteServiceName}], Generating model script with controller: {controller.Value.ControllerName}.");

                var modelScript = _typeScriptProxyGenerator
                    .CreateModelScript(applicationApiDescriptionModel, controller.Value);

                Logger.LogInformation($"  [{module.Value.RemoteServiceName}], {controller.Value.ControllerName} model script generated.");

                var modelScriptPath = Path.Combine(
                    outputFolderRoot, 
                    module.Value.RemoteServiceName.ToKebabCase(), 
                    controller.Value.ControllerGroupName.ToKebabCase(),
                    "model");

                DirectoryHelper.CreateIfNotExists(modelScriptPath);

                var modelScriptFile = Path.Combine(modelScriptPath, "index.ts");

                Logger.LogInformation($"The model script output file: {modelScriptFile}.");
                Logger.LogInformation($"Saving model script: {modelScriptFile}.");

                FileHelper.DeleteIfExists(modelScriptFile);

                await File.AppendAllTextAsync(modelScriptFile, modelScript);

                Logger.LogInformation($"Saved model script: {modelScriptFile} has successful.");

                // api script

                Logger.LogInformation($"  [{module.Value.RemoteServiceName}], Generating api script with controller: {controller.Value.ControllerName}.");

                var apiScript = _typeScriptProxyGenerator.CreateScript(
                    applicationApiDescriptionModel,
                    module.Value,
                    controller.Value);

                Logger.LogInformation($"  [{module.Value.RemoteServiceName}], {controller.Value.ControllerName} api script generated.");

                var apiScriptPath = Path.Combine(
                    outputFolderRoot,
                    module.Value.RemoteServiceName.ToKebabCase(),
                    controller.Value.ControllerGroupName.ToKebabCase());

                DirectoryHelper.CreateIfNotExists(apiScriptPath);

                var apiScriptFile = Path.Combine(apiScriptPath, "index.ts");

                Logger.LogInformation($"The api script output file: {apiScriptFile}.");
                Logger.LogInformation($"Saving api script: {apiScriptFile}.");

                FileHelper.DeleteIfExists(apiScriptFile);

                await File.AppendAllTextAsync(apiScriptFile, apiScript);

                Logger.LogInformation($"Saved api script: {apiScriptFile} has successful.");
            }
        }

        Logger.LogInformation($"Generate type script proxy has completed.");
    }

    protected async override Task<ApplicationApiDescriptionModel> GetApplicationApiDescriptionModelAsync(Volo.Abp.Cli.ServiceProxying.GenerateProxyArgs args)
    {
        Check.NotNull(args.Url, nameof(args.Url));

        var client = CliHttpClientFactory.CreateClient();

        var url = CliUrls.GetApiDefinitionUrl(args.Url);
        var apiDefinitionResult = await client.GetStringAsync(url + "?includeTypes=true");
        var apiDefinition = JsonSerializer.Deserialize<ApplicationApiDescriptionModel>(apiDefinitionResult);

        var moduleDefinition = apiDefinition.Modules.FirstOrDefault(x => string.Equals(x.Key, args.Module, StringComparison.CurrentCultureIgnoreCase)).Value;
        if (moduleDefinition == null)
        {
            throw new CliUsageException($"Module name: {args.Module} is invalid");
        }

        var apiDescriptionModel = ApplicationApiDescriptionModel.Create();
        apiDescriptionModel.AddModule(moduleDefinition);
        apiDescriptionModel.Types = apiDefinition.Types;

        return apiDescriptionModel;
    }
}